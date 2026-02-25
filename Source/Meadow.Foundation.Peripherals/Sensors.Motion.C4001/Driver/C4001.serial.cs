using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Meadow.Foundation.Sensors.Motion;

public partial class C4001
{
    private ISerialPort? _serialPort;

    private const int SerialReadTimeoutMs = 500;
    private const int SensorStopTimeoutMs = 5000;

    /// <summary>
    /// Creates a new C4001 object communicating over serial at 9600 8N1.
    /// </summary>
    public C4001(ISerialPort serialPort)
    {
        _serialPort = serialPort;
        communication = CommunicationType.Serial;
        if (!_serialPort.IsOpen)
            _serialPort.Open();
    }

    private void SerialWrite(string command)
    {
        _serialPort!.Write(Encoding.ASCII.GetBytes(command));
    }

    private string SerialRead(int timeoutMs = SerialReadTimeoutMs)
    {
        var buffer = new byte[200];
        int bytesRead = 0;
        int deadline = Environment.TickCount + timeoutMs;

        while (Environment.TickCount < deadline && bytesRead < buffer.Length)
        {
            if (_serialPort!.BytesToRead > 0)
                bytesRead += _serialPort.Read(buffer, bytesRead, Math.Min(_serialPort.BytesToRead, buffer.Length - bytesRead));
            else
                Thread.Sleep(10);
        }

        return Encoding.ASCII.GetString(buffer, 0, bytesRead);
    }

    /// <summary>
    /// Sends sensorStop and waits for the sensor to echo it back, retrying until confirmed or timeout.
    /// </summary>
    private bool SensorStopSerial()
    {
        int deadline = Environment.TickCount + SensorStopTimeoutMs;
        while (Environment.TickCount < deadline)
        {
            SerialWrite(STOP_SENSOR);
            Thread.Sleep(1000);
            if (SerialRead().Contains(STOP_SENSOR))
                return true;
            Thread.Sleep(400);
        }
        return false;
    }

    /// <summary>
    /// Parses a $DFHPD (Existence) or $DFDMD (Speed) data frame from the sensor.
    /// </summary>
    private AllData ParseDataFrame(string data)
    {
        var result = new AllData();
        int loc = data.IndexOf('$');
        if (loc < 0) return result;

        if (data.IndexOf("$DFHPD", loc, StringComparison.Ordinal) == loc)
        {
            // $DFHPD,X* — char at offset 7 from '$' is '0' or '1'
            result.Status = new SensorStatus { WorkMode = (byte)SensorMode.Existence, WorkStatus = 1, InitStatus = 1 };
            if (loc + 7 < data.Length)
                result.Exist = data[loc + 7] == '1' ? (byte)1 : (byte)0;
        }
        else if (data.IndexOf("$DFDMD", loc, StringComparison.Ordinal) == loc)
        {
            // $DFDMD,num,?,range,speed,energy,...*
            result.Status = new SensorStatus { WorkMode = (byte)SensorMode.Speed, WorkStatus = 1, InitStatus = 1 };
            var parts = data.Substring(loc).Split(',');
            if (parts.Length > 5)
            {
                if (byte.TryParse(parts[1], out var num))
                    result.Target.Number = num;
                if (float.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var range))
                    result.Target.Range = range * 100;   // meters → cm to match I2C units
                if (float.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var speed))
                    result.Target.Speed = speed * 100;
                if (float.TryParse(parts[5].TrimEnd('*', '\r', '\n', ' '), NumberStyles.Any, CultureInfo.InvariantCulture, out var energy))
                    result.Target.Energy = (uint)energy;
            }
        }

        return result;
    }

    /// <summary>
    /// Stop → send query command → read response → start. Returns parsed Response values.
    /// </summary>
    private ResponseData QuerySerial(string command, int responseCount)
    {
        SensorStopSerial();
        SerialWrite(command);
        Thread.Sleep(100);
        var response = SerialRead();
        Thread.Sleep(100);
        SerialWrite(START_SENSOR);
        Thread.Sleep(100);
        return ParseResponse(Encoding.ASCII.GetBytes(response), response.Length, responseCount);
    }

    /// <summary>
    /// Stop → cmd1 → [cmd2] → saveConfig → start. Used for all configuration writes.
    /// </summary>
    private void WriteConfigSerial(string cmd1, string? cmd2 = null)
    {
        SensorStopSerial();
        SerialWrite(cmd1);
        Thread.Sleep(100);
        if (cmd2 != null)
        {
            SerialWrite(cmd2);
            Thread.Sleep(100);
        }
        SerialWrite(SAVE_CONFIG);
        Thread.Sleep(100);
        SerialWrite(START_SENSOR);
        Thread.Sleep(100);
    }

    internal SensorStatus GetStatusSerial()
    {
        SerialRead(100); // flush pending data
        SerialWrite(START_SENSOR);
        int deadline = Environment.TickCount + 3000;
        while (Environment.TickCount < deadline)
        {
            var frame = ParseDataFrame(SerialRead());
            if (frame.Status.InitStatus == 1)
                return frame.Status;
        }
        return new SensorStatus();
    }

    internal bool IsMotionDetectedSerial()
    {
        var data = SerialRead();
        var frame = ParseDataFrame(data);
        if (data.Contains("$DFHPD", StringComparison.Ordinal))
            return frame.Exist == 1;
        if (data.Contains("$DFDMD", StringComparison.Ordinal))
            return frame.Target.Number > 0;
        // No fresh frame — return last cached detection state
        return motionData.Number > 0;
    }

    internal void SetSensorSerial(SensorCommand command)
    {
        switch (command)
        {
            case SensorCommand.Start:
                SerialWrite(START_SENSOR);
                Thread.Sleep(200);
                break;
            case SensorCommand.Stop:
                SensorStopSerial();
                break;
            case SensorCommand.Reset:
                SerialWrite(RESET_SENSOR);
                Thread.Sleep(1500);
                break;
            case SensorCommand.SaveParams:
                SensorStopSerial();
                SerialWrite(SAVE_CONFIG);
                Thread.Sleep(800);
                SerialWrite(START_SENSOR);
                break;
            case SensorCommand.Recover:
                SensorStopSerial();
                SerialWrite(RECOVER_SENSOR);
                Thread.Sleep(800);
                SerialWrite(START_SENSOR);
                Thread.Sleep(500);
                break;
            case SensorCommand.ChangeMode:
                throw new InvalidOperationException("Use SetSensorMode to change mode over serial");
        }
    }

    internal void SetSensorModeSerial(SensorMode mode)
    {
        SensorStopSerial();
        SerialWrite(mode == SensorMode.Existence ? EXIST_MODE : SPEED_MODE);
        Thread.Sleep(100);
        SerialWrite(SAVE_CONFIG);
        Thread.Sleep(500);
        SerialWrite(START_SENSOR);
        Thread.Sleep(100);
    }

    internal bool SetTrigSensitivitySerial(byte sensitivity)
    {
        if (sensitivity > 9) return false;
        WriteConfigSerial($"setSensitivity 255 {sensitivity}");
        return true;
    }

    internal byte GetTrigSensitivitySerial()
    {
        var r = QuerySerial("getSensitivity", 1);
        return r.Status ? (byte)r.Response1 : (byte)0;
    }

    internal bool SetKeepSensitivitySerial(byte sensitivity)
    {
        if (sensitivity > 9) return false;
        WriteConfigSerial($"setSensitivity {sensitivity} 255");
        return true;
    }

    internal byte GetKeepSensitivitySerial()
    {
        var r = QuerySerial("getSensitivity", 2);
        return r.Status ? (byte)r.Response2 : (byte)0;
    }

    internal bool SetDelaySerial(byte trig, ushort keep)
    {
        // trig is in 0.01s units → seconds; keep is in 0.5s units → seconds
        var trigSec = (trig * 0.01f).ToString("F1", CultureInfo.InvariantCulture);
        var keepSec = (keep * 0.5f).ToString("F1", CultureInfo.InvariantCulture);
        WriteConfigSerial($"setLatency {trigSec} {keepSec}");
        return true;
    }

    internal byte GetTrigDelaySerial()
    {
        var r = QuerySerial("getLatency", 1);
        return r.Status ? (byte)(r.Response1 * 100) : (byte)0;
    }

    internal ushort GetKeepTimeoutSerial()
    {
        var r = QuerySerial("getLatency", 2);
        return r.Status ? (ushort)(r.Response2 * 2) : (ushort)0;
    }

    internal bool SetDetectionRangeSerial(ushort min, ushort max, ushort trig)
    {
        if (max < 240 || max > 2000) return false;
        if (min < 30 || min > max) return false;
        if (trig < min || trig > max) return false;

        // Values are in cm; serial protocol takes meters with one decimal place
        var minM = (min / 100.0f).ToString("F1", CultureInfo.InvariantCulture);
        var maxM = (max / 100.0f).ToString("F1", CultureInfo.InvariantCulture);
        var trigM = (trig / 100.0f).ToString("F1", CultureInfo.InvariantCulture);
        WriteConfigSerial($"setRange {minM} {maxM}", $"setTrigRange {trigM}");
        return true;
    }

    internal ushort GetTrigRangeSerial()
    {
        var r = QuerySerial("getTrigRange", 1);
        return r.Status ? (ushort)(r.Response1 * 100) : (ushort)0;
    }

    internal ushort GetMaxRangeSerial()
    {
        var r = QuerySerial("getRange", 2);
        return r.Status ? (ushort)(r.Response2 * 100) : (ushort)0;
    }

    internal ushort GetMinRangeSerial()
    {
        var r = QuerySerial("getRange", 2);
        return r.Status ? (ushort)(r.Response1 * 100) : (ushort)0;
    }

    internal byte GetTargetNumberSerial()
    {
        var frame = ParseDataFrame(SerialRead());
        if (frame.Target.Number != 0)
        {
            motionTimeoutCount = 0;
            motionData.Number = frame.Target.Number;
            motionData.Range = frame.Target.Range / 100.0f;  // cm → meters
            motionData.Speed = frame.Target.Speed / 100.0f;
            motionData.Energy = frame.Target.Energy;
        }
        else
        {
            if (++motionTimeoutCount > 10)
            {
                motionData.Number = 0;
                motionData.Range = 0;
                motionData.Speed = 0;
                motionData.Energy = 0;
            }
        }
        return motionData.Number;
    }

    internal bool SetDetectThresholdSerial(ushort min, ushort max, ushort threshold)
    {
        var minM = (min / 100.0f).ToString("F1", CultureInfo.InvariantCulture);
        var maxM = (max / 100.0f).ToString("F1", CultureInfo.InvariantCulture);
        WriteConfigSerial($"setRange {minM} {maxM}", $"setThrFactor {threshold}");
        return true;
    }

    internal bool SetIoPolaritySerial(byte polarity)
    {
        if (polarity > 1) return false;
        WriteConfigSerial($"setGpioMode 1 {polarity}");
        return true;
    }

    internal byte GetIoPolaritySerial()
    {
        var r = QuerySerial("getGpioMode 1", 2);
        return r.Status ? (byte)r.Response2 : (byte)0;
    }

    internal bool SetPwmSerial(byte pwm1, byte pwm2, byte timer)
    {
        if (pwm1 > 100 || pwm2 > 100) return false;
        WriteConfigSerial($"setPwm {pwm1} {pwm2} {timer}");
        return true;
    }

    internal PwmData GetPwmSerial()
    {
        var r = QuerySerial("getPwm", 3);
        if (!r.Status) return new PwmData();
        return new PwmData { Pwm1 = (byte)r.Response1, Pwm2 = (byte)r.Response2, Timer = (byte)r.Response3 };
    }

    /// <summary>
    /// Parses a "Response X Y Z" query reply from the sensor.
    /// </summary>
    internal ResponseData ParseResponse(byte[] data, int length, int count)
    {
        var response = new ResponseData();
        int startIdx = -1;

        for (int i = 0; i < length - 2; i++)
        {
            if (data[i] == 'R' && data[i + 1] == 'e' && data[i + 2] == 's')
            {
                startIdx = i;
                break;
            }
        }

        if (startIdx < 0)
        {
            response.Status = false;
            return response;
        }

        response.Status = true;
        var spaceIndices = new List<int>();

        for (int i = startIdx; i < length; i++)
        {
            if (data[i] == ' ')
                spaceIndices.Add(i + 1);
        }

        if (spaceIndices.Count >= 1) response.Response1 = ParseFloat(data, spaceIndices[0], length);
        if (spaceIndices.Count >= 2) response.Response2 = ParseFloat(data, spaceIndices[1], length);
        if (count == 3 && spaceIndices.Count >= 3) response.Response3 = ParseFloat(data, spaceIndices[2], length);

        return response;
    }

    private static float ParseFloat(byte[] data, int start, int maxLength)
    {
        int end = start;
        while (end < maxLength && data[end] != ' ' && data[end] != '\n' && data[end] != '\r')
            end++;
        string numberStr = Encoding.ASCII.GetString(data, start, end - start);
        return float.TryParse(numberStr, NumberStyles.Any, CultureInfo.InvariantCulture, out float result)
            ? result
            : 0.0f;
    }
}

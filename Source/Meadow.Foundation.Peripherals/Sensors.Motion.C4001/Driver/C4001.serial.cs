using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Meadow.Foundation.Sensors.Motion;

public partial class C4001
{
    //The baud rate is 9600, 8 bits, no parity, with one stop bit
    readonly ISerialMessagePort? serialMessagePort;

    static readonly byte[] suffixDelimiter = { 13 }; //ASCII return
    static readonly int portSpeed = 9600;

    /// <summary>
    /// Creates a new C4001 object communicating over serial
    /// </summary>
    /// <param name="serialMessage">The serial message port</param>
    internal C4001(ISerialMessagePort serialMessage)
    {
        serialMessagePort = serialMessage;
        serialMessagePort.MessageReceived += SerialMessagePort_MessageReceived;

        communication = CommunicationType.Serial;
    }

    internal SensorStatus GetStatusSerial() => throw new NotImplementedException();
    internal bool IsMotionDetectedSerial() => throw new NotImplementedException();
    internal void SetSensorSerial(SensorCommand command) => throw new NotImplementedException();
    internal void SetSensorModeSerial(SensorMode mode) => throw new NotImplementedException();
    internal bool SetTrigSensitivitySerial(byte sensitivity) => throw new NotImplementedException();
    internal byte GetTrigSensitivitySerial() => throw new NotImplementedException();
    internal bool SetKeepSensitivitySerial(byte sensitivity) => throw new NotImplementedException();
    internal byte GetKeepSensitivitySerial() => throw new NotImplementedException();
    internal bool SetDelaySerial(byte delay) => throw new NotImplementedException();
    internal byte GetTrigDelaySerial() => throw new NotImplementedException();
    internal ushort GetKeepTimeoutSerial() => throw new NotImplementedException();
    internal bool SetDetectionRangeSerial(ushort range) => throw new NotImplementedException();
    internal ushort GetTrigRangeSerial() => throw new NotImplementedException();
    internal ushort GetMaxRangeSerial() => throw new NotImplementedException();
    internal ushort GetMinRangeSerial() => throw new NotImplementedException();
    internal byte GetTargetNumberSerial() => throw new NotImplementedException();
    internal bool SetDetectThresholdSerial(ushort min, ushort max, ushort threshold) => throw new NotImplementedException();
    internal bool SetIoPolaritySerial(byte polarity) => throw new NotImplementedException();
    internal bool SetPwmI2cSerial(byte pwm1, byte pwm2, byte timer) => throw new NotImplementedException();

    private void SerialMessagePort_MessageReceived(object sender, SerialMessageData e)
    {
    }

    internal ResponseData ParseResponse(byte[] data, int length, int count)
    {
        var response = new ResponseData();
        int startIdx = -1;

        // Find the index where "Res" starts
        for (int i = 0; i < length - 2; i++)
        {
            if (data[i] == 'R' && data[i + 1] == 'e' && data[i + 2] == 's')
            {
                startIdx = i;
                break;
            }
        }

        if (startIdx <= 0)
        {
            response.Status = false;
            return response;
        }

        response.Status = true;
        var spaceIndices = new List<int>();

        for (int i = startIdx; i < length; i++)
        {
            if (data[i] == ' ')
            {
                spaceIndices.Add(i + 1); // position after space
            }
        }

        if (spaceIndices.Count > 0)
        {
            if (spaceIndices.Count >= 1)
                response.Response1 = ParseFloat(data, spaceIndices[0], length);

            if (spaceIndices.Count >= 2)
                response.Response2 = ParseFloat(data, spaceIndices[1], length);

            if (count == 3 && spaceIndices.Count >= 3)
                response.Response3 = ParseFloat(data, spaceIndices[2], length);
        }
        else
        {
            response.Response1 = 0.0f;
            response.Response2 = 0.0f;
            response.Response3 = 0.0f;
        }

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

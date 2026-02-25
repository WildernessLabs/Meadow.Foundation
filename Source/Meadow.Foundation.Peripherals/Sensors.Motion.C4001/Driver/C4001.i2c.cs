using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Motion;

public partial class C4001 : II2cPeripheral
{
    /// <inheritdoc/>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    private II2cCommunications? I2cComms = null;

    /// <summary>
    /// Create a new C4001 object connected via I2C
    /// </summary>
    public C4001(II2cBus i2cBus, byte address = (byte)Addresses.Default)
    {
        I2cComms = new I2cCommunications(i2cBus, address);
        communication = CommunicationType.I2C;
    }

    internal SensorStatus GetStatusI2c()
    {
        byte val = I2cComms!.ReadRegister((byte)Registers.STATUS);
        return new SensorStatus
        {
            WorkStatus = (byte)(val & 0x01),
            WorkMode = (byte)((val & 0x02) >> 1),
            InitStatus = (byte)((val & 0x80) >> 7),
        };
    }

    internal bool IsMotionDetectedI2c()
    {
        byte val = I2cComms!.ReadRegister((byte)Registers.RESULT_STATUS);
        return (val & 0x01) != 0;
    }

    internal void SetSensorI2c(SensorCommand command)
    {
        byte register;
        int delayMs;

        switch (command)
        {
            case SensorCommand.Start:
                register = (byte)Registers.CTRL0;
                delayMs = 200;
                break;
            case SensorCommand.Stop:
                register = (byte)Registers.CTRL0;
                delayMs = 200;
                break;
            case SensorCommand.Reset:
                register = (byte)Registers.CTRL0;
                delayMs = 1500;
                break;
            case SensorCommand.SaveParams:
                register = (byte)Registers.CTRL1;
                delayMs = 500;
                break;
            case SensorCommand.Recover:
                register = (byte)Registers.CTRL1;
                delayMs = 800;
                break;
            case SensorCommand.ChangeMode:
                register = (byte)Registers.CTRL1;
                delayMs = 1500;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, "Unknown sensor command");
        }

        I2cComms!.WriteRegister(register, (byte)command);
        Thread.Sleep(delayMs);
    }

    internal bool SetSensorModeI2c(SensorMode mode)
    {
        var status = GetStatusI2c();
        if (status.WorkMode == (byte)mode)
        {
            return true;
        }
        SetSensorI2c(SensorCommand.ChangeMode);
        status = GetStatusI2c();
        return status.WorkMode == (byte)mode;
    }

    internal bool SetTrigSensitivityI2c(byte sensitivity)
    {
        if (sensitivity > 9)
            return false;
        I2cComms!.WriteRegister((byte)Registers.TRIG_SENSITIVITY, sensitivity);
        SetSensorI2c(SensorCommand.SaveParams);
        return true;
    }

    internal byte GetTrigSensitivityI2c()
    {
        return I2cComms!.ReadRegister((byte)Registers.TRIG_SENSITIVITY);
    }

    internal bool SetKeepSensitivityI2c(byte sensitivity)
    {
        if (sensitivity > 9)
            return false;
        I2cComms!.WriteRegister((byte)Registers.KEEP_SENSITIVITY, sensitivity);
        SetSensorI2c(SensorCommand.SaveParams);
        return true;
    }

    internal byte GetKeepSensitivityI2c()
    {
        return I2cComms!.ReadRegister((byte)Registers.KEEP_SENSITIVITY);
    }

    internal bool SetDelayI2c(TimeSpan trig, TimeSpan keep)
    {
        // trig: 0–2 s in 0.01s units → 0–200
        var trigRaw = (int)Math.Round(trig.TotalSeconds * 100);
        // keep: 1–1500 s in 0.5s units → 2–3000
        var keepRaw = (int)Math.Round(keep.TotalSeconds * 2);

        if (trigRaw < 0 || trigRaw > 200)
            return false;
        if (keepRaw < 4 || keepRaw > 3000)
            return false;

        byte[] data =
        [
            (byte)trigRaw,
            (byte)(keepRaw & 0xFF),
            (byte)((keepRaw >> 8) & 0xFF),
        ];
        I2cComms!.WriteRegister((byte)Registers.TRIG_DELAY, data);
        SetSensorI2c(SensorCommand.SaveParams);
        return true;
    }

    internal byte GetTrigDelayI2c()
    {
        return I2cComms!.ReadRegister((byte)Registers.TRIG_DELAY);
    }

    internal TimeSpan GetKeepTimeoutI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.KEEP_TIMEOUT_L, buffer);
        ushort raw = (ushort)((buffer[1] << 8) | buffer[0]);
        return TimeSpan.FromSeconds(raw * 0.5);
    }

    internal bool SetDetectionRangeI2c(Length min, Length max, Length trig)
    {
        // Protocol values are in cm (ushort)
        var minCm = (ushort)Math.Round(min.Centimeters);
        var maxCm = (ushort)Math.Round(max.Centimeters);
        var trigCm = (ushort)Math.Round(trig.Centimeters);

        if (maxCm < 240 || maxCm > 2000)
            return false;
        if (minCm < 30 || minCm > maxCm)
            return false;
        if (trigCm < minCm || trigCm > maxCm)
            return false;
        if (I2cComms is null)
            return false;

        byte[] data =
        [
            (byte)(minCm & 0xFF),
            (byte)((minCm >> 8) & 0xFF),
            (byte)(maxCm & 0xFF),
            (byte)((maxCm >> 8) & 0xFF),
            (byte)(trigCm & 0xFF),
            (byte)((trigCm >> 8) & 0xFF),
        ];
        I2cComms.WriteRegister((byte)Registers.E_MIN_RANGE_L, data);
        SetSensorI2c(SensorCommand.SaveParams);
        return true;
    }

    internal ushort GetTrigRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.E_TRIG_RANGE_L, buffer);
        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    internal ushort GetMaxRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.E_MAX_RANGE_L, buffer);
        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    internal ushort GetMinRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.E_MIN_RANGE_L, buffer);
        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    /// <summary>
    /// Performs a single I2C read and updates all cached motion data.
    /// Must be called before reading range, speed, or energy.
    /// Matches the reference driver pattern where getTargetNumber() is the sole refresh point.
    /// </summary>
    private void UpdateMotionDataI2c()
    {
        byte[] temp = new byte[7];

        // Retry on transient bus-busy errors (e.g. when sharing I2C1 with onboard MCPs on ProjectLab)
        Exception? lastEx = null;
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                I2cComms!.ReadRegister((byte)Registers.RESULT_OBJ_MUN, temp);
                lastEx = null;
                break;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                Thread.Sleep(15);
            }
        }
        if (lastEx != null) throw lastEx;

        if (temp[0] == 1)
        {
            var rangeRaw = BitConverter.ToInt16(temp, 1);

            // A range ≤ 0 is always invalid when a target is reported (minimum real range is
            // 30 cm). The most common cause is I2C bus contention filling bytes with 0xFF,
            // which sign-extends to a large negative value, but any non-positive value is rejected.
            if (rangeRaw <= 0)
            {
                if (++motionTimeoutCount > 10)
                {
                    motionData.Number = 0;
                    motionData.Range = 0;
                    motionData.Speed = 0;
                    motionData.Energy = 0;
                }
                return;
            }

            motionTimeoutCount = 0;
            motionData.Number = 1;
            motionData.Range = rangeRaw / 100.0f;
            motionData.Speed = BitConverter.ToInt16(temp, 3) / 100.0f;   // signed: negative = approaching
            motionData.Energy = BitConverter.ToUInt16(temp, 5);
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
    }

    /// <summary>
    /// Performs the I2C read and refreshes all cached values.
    /// Call this first, then read range/speed/energy from cache.
    /// </summary>
    internal byte GetTargetNumberI2c()
    {
        UpdateMotionDataI2c();
        return motionData.Number;
    }

    internal bool SetDetectThresholdI2c(ushort min, ushort max, ushort threshold)
    {
        if (max > 2500)
            return false;
        if (min > max)
            return false;
        if (I2cComms is null)
            return false;

        byte[] data =
        [
            (byte)(threshold & 0xFF),
            (byte)((threshold >> 8) & 0xFF),
            (byte)(min & 0xFF),
            (byte)((min >> 8) & 0xFF),
            (byte)(max & 0xFF),
            (byte)((max >> 8) & 0xFF),
        ];
        I2cComms.WriteRegister((byte)Registers.CFAR_THR_L, data);
        SetSensorI2c(SensorCommand.SaveParams);
        return true;
    }

    internal bool SetIoPolarityI2c(byte value) => throw new NotSupportedException("IO polarity is not supported over I2C");
    internal byte GetIoPolarityI2c() => 0;                // not supported over I2C; returns 0 as default
    internal bool SetPwmI2c(byte pwm1, byte pwm2, byte timer) => throw new NotSupportedException("PWM is not supported over I2C");
    internal PwmData GetPwmI2c() => new PwmData();        // not supported over I2C; returns empty struct

    internal ushort GetTMinRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.T_MIN_RANGE_L, buffer);
        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    internal ushort GetTMaxRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.T_MAX_RANGE_L, buffer);
        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    internal ushort GetThresholdRangeI2c()
    {
        Span<byte> buffer = stackalloc byte[2];
        I2cComms!.ReadRegister((byte)Registers.CFAR_THR_L, buffer);
        return (ushort)(buffer[0] | (buffer[1] << 8));
    }

    internal void SetFrettingDetectionI2c(SwitchState state)
    {
        Span<byte> buffer = stackalloc byte[1];
        buffer[0] = (byte)state;
        I2cComms!.WriteRegister((byte)Registers.MICRO_MOTION, buffer);
        SetSensorI2c(SensorCommand.SaveParams);
    }

    internal SwitchState GetFrettingDetectionI2c()
    {
        Span<byte> buffer = stackalloc byte[1];
        I2cComms!.ReadRegister((byte)Registers.MICRO_MOTION, buffer);
        return (SwitchState)buffer[0];
    }
}

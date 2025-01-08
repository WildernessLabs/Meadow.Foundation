using Meadow.Modbus;
using Meadow.Peripherals;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.MotorControllers.StepperOnline;

public class BLD510B : ModbusPolledDevice
{
    public event EventHandler<ErrorConditions> ErrorConditionsChanged;

    private ErrorConditions _lastError;
    private ushort _state;

    /// <summary>
    /// The default Modbus address for the V10x device.
    /// </summary>
    public const int DefaultModbusAddress = 1;

    /// <summary>
    /// The default baud rate for communication with the V10x device.
    /// </summary>
    public const int DefaultBaudRate = 9600;

    public BLD510B(ModbusRtuClient client, byte modbusAddress = 0x01, TimeSpan? refreshPeriod = null)
        : base(client, modbusAddress, refreshPeriod)
    {
        MapHoldingRegistersToField(
            startRegister: 0x801b,
            registerCount: 1,
            fieldName: nameof(_state),
            conversionFunction: StateCheckerFunction
            );
    }

    public async Task<bool> GetStartStopTerminal()
    {
        var r = await ReadHoldingRegisters(0x8000, 1);

        return ((r[0] >> 8) & 1) != 0;
    }

    public async Task SetStartStopTerminal(bool startEnabled)
    {
        var current = (int)(await ReadHoldingRegisters(0x8000, 1))[0];
        if (startEnabled)
        { // set bit 0
            current = current | (1 << 8);
        }
        else
        {
            current = current & ~(1 << 8);
        }
        await WriteHoldingRegister(0x8000, (ushort)current);
    }

    public async Task<RotationDirection> GetDirectionTerminal()
    {
        var r = await ReadHoldingRegisters(0x8000, 1);
        return ((r[0] >> 9) & 1) == 0 ? RotationDirection.Clockwise : RotationDirection.CounterClockwise;
    }

    public async Task SetDirectionTerminal(RotationDirection direction)
    {
        int current = ReadHoldingRegisters(0x8000, 1).Result[0];
        if (direction == RotationDirection.Clockwise)
        { // clear bit 1
            current = current & ~(1 << 9);
        }
        else
        {
            current = current | (1 << 9);
        }
        await WriteHoldingRegister(0x8000, (ushort)current);
    }

    public async Task<bool> GetBrakeTerminal()
    {
        var r = await ReadHoldingRegisters(0x8000, 1);
        return ((r[0] >> 10) & 1) != 0;
    }

    public async Task SetBrakeTerminal(bool brakeEnabled)
    {
        int current = ReadHoldingRegisters(0x8000, 1).Result[0];
        if (brakeEnabled)
        { // set bit 2
            current = current | (1 << 10);
        }
        else
        {
            current = current & ~(1 << 10);
        }
        await WriteHoldingRegister(0x8000, (ushort)current);
    }

    public async Task<SpeedControl> GetSpeedControl()
    {
        var r = await ReadHoldingRegisters(0x8000, 1);
        return (SpeedControl)((r[0] >> 11) & 1);
    }

    public async Task SetSpeedControl(SpeedControl speedControl)
    {
        int current = ReadHoldingRegisters(0x8000, 1).Result[0];
        if (speedControl == SpeedControl.AnalogPot)
        { // clear bit 4
            current = current & ~(1 << 11);
        }
        else
        {
            current = current | (1 << 11);
        }
        await WriteHoldingRegister(0x8000, (ushort)current);
    }

    public async Task<byte> GetNumberOfMotorPolePairs()
    {
        var r = await ReadHoldingRegisters(0x8000, 1);
        return (byte)(r[0] & 0xff);
    }

    public async Task SetNumberOfMotorPolePairs(byte numberOfMotorPolePairs)
    {
        var current = (int)(await ReadHoldingRegisters(0x8000, 1))[0];
        current &= 0xff00;
        current |= numberOfMotorPolePairs;
        // always disable EN if we're doing this operation
        current &= ~(1 << 8);
        await WriteHoldingRegister(0x8000, (ushort)current);
    }

    public async Task<byte> GetStartupTorque()
    {
        var r = await ReadHoldingRegisters(0x8002, 1);
        return (byte)(r[0] >> 8);
    }

    public async Task SetStartupTorque(byte value)
    {
        var r = await ReadHoldingRegisters(0x8002, 1);
        var current = r[0] & 0x00ff;
        current |= value << 8;
        await WriteHoldingRegister(0x8002, (ushort)current);
    }

    public async Task<byte> GetStartupSpeed()
    {
        var r = await ReadHoldingRegisters(0x8002, 1);
        return (byte)(r[0] & 0xff);
    }

    public async Task SetStartupSpeed(byte value)
    {
        var r = await ReadHoldingRegisters(0x8002, 1);
        var current = r[0] & 0xff00;
        current |= value;
        await WriteHoldingRegister(0x8002, (ushort)current);
    }

    public async Task<TimeSpan> GetAccelerationTime()
    {
        var r = await ReadHoldingRegisters(0x8003, 1);
        return TimeSpan.FromSeconds((r[0] >> 8) / 10d);
    }

    public async Task SetAccelerationTime(TimeSpan value)
    {
        if (value.TotalSeconds < 0 || value.TotalSeconds > 25.5) throw new ArgumentOutOfRangeException();
        var r = await ReadHoldingRegisters(0x8002, 1);
        var current = r[0] & 0x00ff;
        current |= (byte)(value.TotalSeconds * 10) << 8;
        await WriteHoldingRegister(0x8003, (ushort)current);
    }

    public async Task<TimeSpan> GetDecelerationTime()
    {
        var r = await ReadHoldingRegisters(0x8003, 1);
        return TimeSpan.FromSeconds((r[0] & 0xff) / 10d);
    }

    public async Task SetDecelerationTime(TimeSpan value)
    {
        if (value.TotalSeconds < 0 || value.TotalSeconds > 25.5) throw new ArgumentOutOfRangeException();
        var r = await ReadHoldingRegisters(0x8003, 1);
        var current = r[0] & 0xff00;
        current |= (byte)(value.TotalSeconds * 10);
        await WriteHoldingRegister(0x8003, (ushort)current);
    }

    public async Task<byte> GetMaxCurrent()
    {
        var r = await ReadHoldingRegisters(0x8004, 1);
        return (byte)(r[0] >> 8);
    }

    public async Task SetMaxCurrent(byte value)
    {
        var r = await ReadHoldingRegisters(0x8004, 1);
        var current = r[0] & 0x00ff;
        current |= value << 8;
        await WriteHoldingRegister(0x8004, (ushort)current);
    }

    public async Task<MotorType> GetMotorType()
    {
        var r = await ReadHoldingRegisters(0x8004, 1);
        return (MotorType)(r[0] & 0xff);
    }

    public async Task SetMotorType(MotorType value)
    {
        var r = await ReadHoldingRegisters(0x8004, 1);
        var current = r[0] & 0xff00;
        current |= (int)value;
        await WriteHoldingRegister(0x8004, (ushort)current);
    }

    /// <summary>
    /// The desired speed when using control mode of RS485, this is ignored when control is analog
    /// </summary>
    public async Task<ushort> GetDesiredSpeed()
    {
        var r = await ReadHoldingRegisters(0x8005, 1);
        return r[0];
    }

    public async Task SetDesiredSpeed(ushort speed)
    {
        // swap endianness
        var s = speed << 8 | speed >> 8;
        await WriteHoldingRegister(0x8005, (ushort)s);
    }

    public async Task<byte> GetModbusAddress()
    {
        var r = await ReadHoldingRegisters(0x8007, 1);
        return (byte)r[0];
    }

    public async Task SetModbusAddress(byte value)
    {
        if (value <= 0 || value > 250) throw new ArgumentOutOfRangeException();

        await WriteHoldingRegister(0x8007, value);
    }

    public async Task<ushort> GetActualSpeed()
    {
        ushort[] data;
        do
        {
            data = await ReadHoldingRegisters(0x8018, 1);
        } while (data.Length == 0);

        // swap endianness
        return (ushort)(data[0] >> 8 | data[0] << 8);
    }

    public ErrorConditions ErrorConditions
    {
        get => _lastError;
    }

    private object StateCheckerFunction(ushort[] data)
    {
        // we use this function to set events
        var state = (ErrorConditions)(data[0] >> 8);

        if (state != _lastError)
        {
            _lastError = state;
            ErrorConditionsChanged?.Invoke(this, state);
        }

        return data[0];
    }
}

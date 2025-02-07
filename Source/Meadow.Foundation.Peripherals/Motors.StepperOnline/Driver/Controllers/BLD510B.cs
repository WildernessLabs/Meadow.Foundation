using Meadow.Modbus;
using Meadow.Peripherals;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.MotorControllers.StepperOnline;

/// <summary>
/// Represents the BLD510B motor controller, a Modbus-controlled device.
/// </summary>
public class BLD510B : ModbusPolledDevice
{
    /// <summary>
    /// Event triggered when the error conditions change.
    /// </summary>
    public event EventHandler<ErrorConditions>? ErrorConditionsChanged = null;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="BLD510B"/> class.
    /// </summary>
    /// <param name="client">The Modbus RTU client to communicate with the device.</param>
    /// <param name="modbusAddress">The Modbus address of the device.</param>
    /// <param name="refreshPeriod">The refresh period for polling the device.</param>
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

    /// <summary>
    /// Gets the status of the start/stop terminal.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation.
    /// The result contains a boolean indicating the terminal status.
    /// </returns>
    public async Task<bool> GetStartStopTerminal()
    {
        var r = await ReadHoldingRegisters(0x8000, 1);
        return ((r[0] >> 8) & 1) != 0;
    }

    /// <summary>
    /// Sets the status of the start/stop terminal.
    /// </summary>
    /// <param name="startEnabled">True to enable start; otherwise, false.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task SetStartStopTerminal(bool startEnabled)
    {
        var current = (int)(await ReadHoldingRegisters(0x8000, 1))[0];
        if (startEnabled)
        {
            // set bit 8
            current |= (1 << 8);
        }
        else
        {
            // clear bit 8
            current &= ~(1 << 8);
        }
        await WriteHoldingRegister(0x8000, (ushort)current);
    }

    /// <summary>
    /// Gets the rotation direction from the direction terminal.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation.
    /// The result contains the <see cref="RotationDirection"/>.
    /// </returns>
    public async Task<RotationDirection> GetDirectionTerminal()
    {
        var r = await ReadHoldingRegisters(0x8000, 1);
        return ((r[0] >> 9) & 1) == 0 ? RotationDirection.Clockwise : RotationDirection.CounterClockwise;
    }

    /// <summary>
    /// Sets the rotation direction for the direction terminal.
    /// </summary>
    /// <param name="direction">The desired <see cref="RotationDirection"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task SetDirectionTerminal(RotationDirection direction)
    {
        int current = (await ReadHoldingRegisters(0x8000, 1))[0];
        if (direction == RotationDirection.Clockwise)
        {
            // clear bit 9
            current &= ~(1 << 9);
        }
        else
        {
            // set bit 9
            current |= (1 << 9);
        }
        await WriteHoldingRegister(0x8000, (ushort)current);
    }

    /// <summary>
    /// Gets the state of the brake terminal.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation.
    /// The result contains a boolean indicating the brake status.
    /// </returns>
    public async Task<bool> GetBrakeTerminal()
    {
        var r = await ReadHoldingRegisters(0x8000, 1);
        return ((r[0] >> 10) & 1) != 0;
    }

    /// <summary>
    /// Sets the brake terminal state.
    /// </summary>
    /// <param name="brakeEnabled">True to enable brake; otherwise, false.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task SetBrakeTerminal(bool brakeEnabled)
    {
        int current = (await ReadHoldingRegisters(0x8000, 1))[0];
        if (brakeEnabled)
        {
            // set bit 10
            current |= (1 << 10);
        }
        else
        {
            // clear bit 10
            current &= ~(1 << 10);
        }
        await WriteHoldingRegister(0x8000, (ushort)current);
    }

    /// <summary>
    /// Gets the current speed control mode (RS485 or Analog Pot).
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation.
    /// The result is a <see cref="SpeedControl"/> enum value.
    /// </returns>
    public async Task<SpeedControl> GetSpeedControl()
    {
        var r = await ReadHoldingRegisters(0x8000, 1);
        return (SpeedControl)((r[0] >> 11) & 1);
    }

    /// <summary>
    /// Sets the speed control mode (RS485 or Analog Pot).
    /// </summary>
    /// <param name="speedControl">The desired <see cref="SpeedControl"/> mode.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    public async Task SetSpeedControl(SpeedControl speedControl)
    {
        int current = (await ReadHoldingRegisters(0x8000, 1))[0];
        if (speedControl == SpeedControl.AnalogPot)
        {
            // clear bit 11
            current &= ~(1 << 11);
        }
        else
        {
            // set bit 11
            current |= (1 << 11);
        }
        await WriteHoldingRegister(0x8000, (ushort)current);
    }

    /// <summary>
    /// Gets the configured number of motor pole pairs.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation.
    /// The result is the number of motor pole pairs.
    /// </returns>
    public async Task<byte> GetNumberOfMotorPolePairs()
    {
        var r = await ReadHoldingRegisters(0x8000, 1);
        return (byte)(r[0] & 0xff);
    }

    /// <summary>
    /// Sets the number of motor pole pairs.
    /// </summary>
    /// <param name="numberOfMotorPolePairs">The number of pole pairs to configure.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetNumberOfMotorPolePairs(byte numberOfMotorPolePairs)
    {
        var current = (int)(await ReadHoldingRegisters(0x8000, 1))[0];
        current &= 0xff00;
        current |= numberOfMotorPolePairs;
        // Disable EN (bit 8) if we're doing this operation
        current &= ~(1 << 8);
        await WriteHoldingRegister(0x8000, (ushort)current);
    }

    /// <summary>
    /// Gets the configured startup torque.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation.
    /// The result is the startup torque value (0-255).
    /// </returns>
    public async Task<byte> GetStartupTorque()
    {
        var r = await ReadHoldingRegisters(0x8002, 1);
        return (byte)(r[0] >> 8);
    }

    /// <summary>
    /// Sets the configured startup torque.
    /// </summary>
    /// <param name="value">The startup torque (0-255).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetStartupTorque(byte value)
    {
        var r = await ReadHoldingRegisters(0x8002, 1);
        var current = r[0] & 0x00ff;
        current |= value << 8;
        await WriteHoldingRegister(0x8002, (ushort)current);
    }

    /// <summary>
    /// Gets the configured startup speed.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation.
    /// The result is the startup speed value (0-255).
    /// </returns>
    public async Task<byte> GetStartupSpeed()
    {
        var r = await ReadHoldingRegisters(0x8002, 1);
        return (byte)(r[0] & 0xff);
    }

    /// <summary>
    /// Sets the configured startup speed.
    /// </summary>
    /// <param name="value">The startup speed (0-255).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetStartupSpeed(byte value)
    {
        var r = await ReadHoldingRegisters(0x8002, 1);
        var current = r[0] & 0xff00;
        current |= value;
        await WriteHoldingRegister(0x8002, (ushort)current);
    }

    /// <summary>
    /// Gets the configured acceleration time.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is a <see cref="TimeSpan"/>
    /// representing the time it takes to accelerate.
    /// </returns>
    public async Task<TimeSpan> GetAccelerationTime()
    {
        var r = await ReadHoldingRegisters(0x8003, 1);
        return TimeSpan.FromSeconds((r[0] >> 8) / 10d);
    }

    /// <summary>
    /// Sets the acceleration time.
    /// </summary>
    /// <param name="value">
    /// A <see cref="TimeSpan"/> (0-25.5 seconds) representing how long it takes to accelerate.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the specified <paramref name="value"/> is out of the 0-25.5 second range.
    /// </exception>
    public async Task SetAccelerationTime(TimeSpan value)
    {
        if (value.TotalSeconds < 0 || value.TotalSeconds > 25.5)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Acceleration time must be between 0 and 25.5 seconds.");
        }
        var r = await ReadHoldingRegisters(0x8002, 1);
        var current = r[0] & 0x00ff;
        current |= (byte)(value.TotalSeconds * 10) << 8;
        await WriteHoldingRegister(0x8003, (ushort)current);
    }

    /// <summary>
    /// Gets the configured deceleration time.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is a <see cref="TimeSpan"/>
    /// representing the time it takes to decelerate.
    /// </returns>
    public async Task<TimeSpan> GetDecelerationTime()
    {
        var r = await ReadHoldingRegisters(0x8003, 1);
        return TimeSpan.FromSeconds((r[0] & 0xff) / 10d);
    }

    /// <summary>
    /// Sets the deceleration time.
    /// </summary>
    /// <param name="value">
    /// A <see cref="TimeSpan"/> (0-25.5 seconds) representing how long it takes to decelerate.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the specified <paramref name="value"/> is out of the 0-25.5 second range.
    /// </exception>
    public async Task SetDecelerationTime(TimeSpan value)
    {
        if (value.TotalSeconds < 0 || value.TotalSeconds > 25.5)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Deceleration time must be between 0 and 25.5 seconds.");
        }
        var r = await ReadHoldingRegisters(0x8003, 1);
        var current = r[0] & 0xff00;
        current |= (byte)(value.TotalSeconds * 10);
        await WriteHoldingRegister(0x8003, (ushort)current);
    }

    /// <summary>
    /// Gets the configured maximum current limit.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is a byte representing the maximum current setting (0-255).
    /// </returns>
    public async Task<byte> GetMaxCurrent()
    {
        var r = await ReadHoldingRegisters(0x8004, 1);
        return (byte)(r[0] >> 8);
    }

    /// <summary>
    /// Sets the maximum current limit.
    /// </summary>
    /// <param name="value">A byte (0-255) representing the maximum current setting.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetMaxCurrent(byte value)
    {
        var r = await ReadHoldingRegisters(0x8004, 1);
        var current = r[0] & 0x00ff;
        current |= value << 8;
        await WriteHoldingRegister(0x8004, (ushort)current);
    }

    /// <summary>
    /// Gets the configured motor type.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is the <see cref="MotorType"/> 
    /// for the current motor configuration.
    /// </returns>
    public async Task<MotorType> GetMotorType()
    {
        var r = await ReadHoldingRegisters(0x8004, 1);
        return (MotorType)(r[0] & 0xff);
    }

    /// <summary>
    /// Sets the motor type.
    /// </summary>
    /// <param name="value">The <see cref="MotorType"/> to set.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetMotorType(MotorType value)
    {
        var r = await ReadHoldingRegisters(0x8004, 1);
        var current = r[0] & 0xff00;
        current |= (int)value;
        await WriteHoldingRegister(0x8004, (ushort)current);
    }

    /// <summary>
    /// Gets the desired speed when using RS485 control mode. 
    /// Ignored when using analog control.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is a <see cref="ushort"/> 
    /// representing the desired speed setting.
    /// </returns>
    public async Task<ushort> GetDesiredSpeed()
    {
        var r = await ReadHoldingRegisters(0x8005, 1);
        return r[0];
    }

    /// <summary>
    /// Sets the desired speed when using RS485 control mode.
    /// Ignored when using analog control mode.
    /// </summary>
    /// <param name="speed">
    /// A <see cref="ushort"/> representing the desired speed value.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetDesiredSpeed(ushort speed)
    {
        // Swap endianness
        var s = (ushort)((speed << 8) | (speed >> 8));
        await WriteHoldingRegister(0x8005, s);
    }

    /// <summary>
    /// Gets the Modbus address configured on the controller.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is a <see cref="byte"/> representing the current Modbus address.
    /// </returns>
    public async Task<byte> GetModbusAddress()
    {
        var r = await ReadHoldingRegisters(0x8007, 1);
        return (byte)r[0];
    }

    /// <summary>
    /// Sets the Modbus address for the controller.
    /// </summary>
    /// <param name="value">A <see cref="byte"/> representing the new Modbus address (1-250).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="value"/> is out of the valid range (1-250).
    /// </exception>
    public async Task SetModbusAddress(byte value)
    {
        if (value <= 0 || value > 250) throw new ArgumentOutOfRangeException(nameof(value), "Modbus address must be between 1 and 250.");
        await WriteHoldingRegister(0x8007, value);
    }

    /// <summary>
    /// Gets the actual speed of the motor, as reported by the controller.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is a <see cref="ushort"/> 
    /// representing the current actual speed.
    /// </returns>
    public async Task<ushort> GetActualSpeed()
    {
        ushort[] data;
        do
        {
            data = await ReadHoldingRegisters(0x8018, 1);
        }
        while (data.Length == 0);

        // Swap endianness
        return (ushort)((data[0] >> 8) | (data[0] << 8));
    }

    /// <summary>
    /// Gets the current error conditions for this controller.
    /// </summary>
    public ErrorConditions ErrorConditions
    {
        get => _lastError;
    }

    /// <summary>
    /// A function used to process the state register data and raise events
    /// when error conditions change.
    /// </summary>
    /// <param name="data">An array of register values read from the device.</param>
    /// <returns>The raw <see cref="ushort"/> state value.</returns>
    private object StateCheckerFunction(ushort[] data)
    {
        // We use this function to set events
        var state = (ErrorConditions)(data[0] >> 8);

        if (state != _lastError)
        {
            _lastError = state;
            ErrorConditionsChanged?.Invoke(this, state);
        }

        return data[0];
    }
}

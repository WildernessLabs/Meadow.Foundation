using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion;

/// <summary>
/// Represents the Xtrinsic MAG3110 Three-Axis, Digital Magnetometer
/// </summary>
public partial class Mag3110 :
    ByteCommsSensorBase<(MagneticField3D? MagneticField3D, Units.Temperature? Temperature)>,
    ITemperatureSensor, IMagnetometer, II2cPeripheral
{
    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers;
    private event EventHandler<IChangeResult<MagneticField3D>> _fieldHandlers;

    event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
    {
        add => _temperatureHandlers += value;
        remove => _temperatureHandlers -= value;
    }

    event EventHandler<IChangeResult<MagneticField3D>> ISamplingSensor<MagneticField3D>.Updated
    {
        add => _fieldHandlers += value;
        remove => _fieldHandlers -= value;
    }

    /// <summary>
    /// Interrupt port used to detect then end of a conversion
    /// </summary>
    protected readonly IDigitalInterruptPort? interruptPort;

    /// <summary>
    /// The current magnetic field value
    /// </summary>
    public MagneticField3D? MagneticField3D => Conditions.MagneticField3D;

    /// <summary>
    /// Current temperature of the die
    /// </summary>
    public Units.Temperature? Temperature => Conditions.Temperature;

    /// <summary>
    /// Change or get the standby status of the sensor
    /// </summary>
    public bool Standby
    {
        get
        {
            var controlRegister = BusComms.ReadRegister(Registers.CONTROL_1);
            return (controlRegister & 0x03) == 0;
        }
        set
        {
            var controlRegister = BusComms.ReadRegister(Registers.CONTROL_1);
            if (value)
            {
                controlRegister &= 0xfc; // ~0x03
            }
            else
            {
                controlRegister |= 0x01;
            }
            BusComms.WriteRegister(Registers.CONTROL_1, controlRegister);
        }
    }

    /// <summary>
    /// Indicate if there is any data ready for reading (x, y or z).
    /// </summary>
    /// <remarks>
    /// See section 5.1.1 of the datasheet.
    /// </remarks>
    public bool IsDataReady => (BusComms.ReadRegister(Registers.DR_STATUS) & 0x08) > 0;


    /// <summary>
    /// Enable or disable interrupts.
    /// </summary>
    /// <remarks>
    /// Interrupts can be triggered when a conversion completes (see section 4.2.5
    /// of the datasheet).  The interrupts are tied to the ZYXDR bit in the DR Status
    /// register.
    /// </remarks>
    public bool DigitalInputsEnabled
    {
        get => digitalInputsEnabled;
        set
        {
            Standby = true;
            var cr2 = BusComms.ReadRegister(Registers.CONTROL_2);
            if (value)
            {
                cr2 |= 0x80;
            }
            else
            {
                cr2 &= 0x7f;
            }
            BusComms.WriteRegister(Registers.CONTROL_2, cr2);
            digitalInputsEnabled = value;
        }
    }

    private bool digitalInputsEnabled;

    /// <summary>
    /// Create a new MAG3110 object using the default parameters for the component
    /// </summary>
    /// <param name="interruptPort">Interrupt port used to detect end of conversions</param>
    /// <param name="address">Address of the MAG3110 (default = 0x0e)</param>
    /// <param name="i2cBus">I2C bus object - default = 400 KHz)</param>        
    public Mag3110(II2cBus i2cBus, IDigitalInterruptPort? interruptPort = null, byte address = (byte)Addresses.Default)
        : base(i2cBus, address)
    {
        var deviceID = BusComms.ReadRegister(Registers.WHO_AM_I);
        if (deviceID != 0xc4)
        {
            throw new Exception("Unknown device ID, " + deviceID + " returned, 0xc4 expected");
        }

        if (interruptPort != null)
        {
            this.interruptPort = interruptPort;
            this.interruptPort.Changed += DigitalInputPortChanged;
        }
        Reset();
    }

    /// <summary>
    /// Reset the sensor
    /// </summary>
    public void Reset()
    {
        Standby = true;
        BusComms.WriteRegister(Registers.CONTROL_1, 0x00);
        BusComms.WriteRegister(Registers.CONTROL_2, 0x80);
        WriteBuffer.Span[0] = Registers.X_OFFSET_MSB;
        WriteBuffer.Span[1] = WriteBuffer.Span[2] = WriteBuffer.Span[3] = 0;
        WriteBuffer.Span[4] = WriteBuffer.Span[5] = WriteBuffer.Span[6] = 0;

        BusComms.Write(WriteBuffer.Span[0..7]);
    }

    /// <summary>
    /// Raise events for subscribers and notify of value changes
    /// </summary>
    /// <param name="changeResult">The updated sensor data</param>
    protected override void RaiseEventsAndNotify(IChangeResult<(MagneticField3D? MagneticField3D, Units.Temperature? Temperature)> changeResult)
    {
        if (changeResult.New.MagneticField3D is { } mag)
        {
            _fieldHandlers?.Invoke(this, new ChangeResult<MagneticField3D>(mag, changeResult.Old?.MagneticField3D));
        }
        if (changeResult.New.Temperature is { } temp)
        {
            _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
        }
        base.RaiseEventsAndNotify(changeResult);
    }

    /// <summary>
    /// Reads data from the sensor
    /// </summary>
    /// <returns>The latest sensor reading</returns>
    protected override Task<(MagneticField3D? MagneticField3D, Units.Temperature? Temperature)> ReadSensor()
    {
        (MagneticField3D? MagneticField3D, Units.Temperature? Temperature) conditions;

        var controlRegister = BusComms.ReadRegister(Registers.CONTROL_1);
        controlRegister |= 0x02;
        BusComms.WriteRegister(Registers.CONTROL_1, controlRegister);

        BusComms.ReadRegister(Registers.X_MSB, ReadBuffer.Span[0..6]);

        conditions.MagneticField3D = new MagneticField3D(
            new MagneticField((short)((ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1]), MagneticField.UnitType.MicroTesla),
            new MagneticField((short)((ReadBuffer.Span[2] << 8) | ReadBuffer.Span[3]), MagneticField.UnitType.MicroTesla),
            new MagneticField((short)((ReadBuffer.Span[4] << 8) | ReadBuffer.Span[5]), MagneticField.UnitType.MicroTesla)
            );

        conditions.Temperature = new Units.Temperature((sbyte)BusComms.ReadRegister(Registers.TEMPERATURE), Units.Temperature.UnitType.Celsius);

        return Task.FromResult(conditions);
    }

    /// <summary>
    /// Interrupt for the MAG3110 conversion complete interrupt.
    /// </summary>
    private void DigitalInputPortChanged(object sender, DigitalPortResult e)
    {
        // TODO
        /*
        if (OnReadingComplete != null)
        {
            Read();
            var readings = new SensorReading();
            readings.X = X;
            readings.Y = Y;
            readings.Z = Z;
            OnReadingComplete(readings);
        }*/
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
        => (await Read()).Temperature!.Value;

    async Task<MagneticField3D> ISensor<MagneticField3D>.Read()
        => (await Read()).MagneticField3D!.Value;
}
using Meadow.Hardware;
using Meadow.Units;
using System.Linq;
using System;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Driver Class for the Ads1263 analog-to-digital (ADC) converter
/// </summary>
public partial class Ads1263 : IAnalogInputController, IDigitalInputOutputController, ISpiPeripheral, IDisposable
{
    /// <summary>
    /// Ads1263 pin definitions
    /// </summary>
    public PinDefinitions Pins { get; }

    // Internal state tracking variables
    private byte ioEnable, ioDir, ioOut, gain1, gain2;
    private double vRef1, vRef2;

    #region SPI
    /// <summary>
    /// Gets the underlying ISpiCommunications instance
    /// </summary>
    protected ISpiCommunications SpiComms { get; }

    /// <inheritdoc/>
    public SpiClockConfiguration.Mode DefaultSpiBusMode { get; } = SpiClockConfiguration.Mode.Mode1;

    /// <inheritdoc/>
    public SpiClockConfiguration.Mode SpiBusMode { get => SpiComms.BusMode; set => SpiComms.BusMode = value; }

    /// <inheritdoc/>
    public Frequency DefaultSpiBusSpeed { get; } = new Frequency(1000000);

    /// <inheritdoc/>
    public Frequency SpiBusSpeed { get => SpiComms.BusSpeed; set => SpiComms.BusSpeed = value; }

    /// <summary> Did we create the port(s) used by the peripheral </summary>
    private readonly bool createdPort = false;
    
    private readonly IDigitalOutputPort chipSelectPort;
    #endregion

    /// <summary>
    /// object for using lock() while modifying GPIO outputs
    /// </summary>
    protected object _lock = new();

    /// <summary>
    /// Ads1263 class constructor
    /// </summary>
    /// <param name="spiBus">The SPI bus</param>
    /// <param name="chipSelectPin">Chip select pin</param>
    public Ads1263(ISpiBus spiBus, IPin chipSelectPin) : this(spiBus, chipSelectPin.CreateDigitalOutputPort())
    {
        createdPort = true;
    }

    /// <summary>
    /// Ads1263 class constructor
    /// </summary>
    /// <param name="spiBus">The SPI bus</param>
    /// <param name="chipSelectPort">Chip select port</param>
    public Ads1263(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
    {
        SpiComms = new SpiCommunications(spiBus, this.chipSelectPort = chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);

        Pins = new PinDefinitions(this)
        {
            Controller = this
        };

        gain1 = 1;
        gain2 = 1;
        vRef1 = 2.5;
        vRef2 = 2.5;

        Initialize();
    }

    /// <summary>
    /// Read the cached reference voltage used during setup
    /// </summary>
    internal Voltage GetADCReferenceVoltage(IPin pin)
    {
        var vRef = (byte)pin.Key switch { 0x00 => vRef1, _ => vRef2 };
        return new Voltage(vRef, Voltage.UnitType.Volts);
    }

    #region Configuration
    private void Initialize()
    {
        // read Device ID to confirm communications
        var deviceId = ReadRegister(Register.ID);
        // TODO: Device ID should start with 0x2 or 0x3 for Ads1263

        // use default configuration of ADCs
        ConfigureADC1();
        ConfigureADC2();

        // read current GPIO states (Note: output states always read as 0)
        ioEnable = ReadRegister(Register.GPIOCON);
        ioDir = ReadRegister(Register.GPIODIR);
        ioOut = 0x00;
    }

    // TODO: more ADC features (chopping, VBias, Reference reversal, one-shot conversion, conversion delay, Current sources, Test DAC)
    // TODO: Data Ready input pin and events

    /// <summary>
    /// Configures the ADC1 hardware (32-bit ADC).
    /// </summary>
    /// <param name="positiveSource">Enumeration specifying the positive ADC input</param>
    /// <param name="negativeSource">Enumeration specifying the negative ADC input</param>
    /// <param name="gain">Enumeration specifying channel gain.</param>
    /// <param name="rate">Enumeration specifying the sampling rate internal to the ADC</param>
    /// <param name="filter">Enumeration specifying the filtering configuration.</param>
    /// <param name="referenceVoltage">Reference voltage which needs to be specified if using external references.</param>
    /// <param name="positiveReference">Enumeration specifying the positive reference source</param>
    /// <param name="negativeReference">Enumeration specifying the negative reference source</param>
    /// <remarks>See the datasheet for valid combinations of <paramref name="rate"/> and <paramref name="filter"/>.</remarks>
    public void ConfigureADC1(AdcSource positiveSource = AdcSource.AIN0, AdcSource negativeSource = AdcSource.AIN1,
        Adc1Gain gain = Adc1Gain.Gain_1, Adc1DataRate rate = Adc1DataRate.SPS_20, Adc1Filter filter = Adc1Filter.FIR,
        double referenceVoltage = 5.0, Adc1ReferenceP positiveReference = Adc1ReferenceP.Internal, Adc1ReferenceN negativeReference = Adc1ReferenceN.Internal)
    {
        // Verify combinations of filter and data rate settings
        if (filter == Adc1Filter.FIR && ((byte)rate > (byte)Adc1DataRate.SPS_20 || rate == Adc1DataRate.SPS_16p6)) 
            throw new ArgumentException("FIR filter can only be used with 2.5, 5, 10, or 20 samples per second");
        // Note: The three fastest data rates bypass the second filter stage, so filter may be ignored.

        WriteRegister(Register.MODE1, (byte)filter << 5);
        WriteRegister(Register.MODE2, (byte)gain << 4 | (byte)rate);
        WriteRegister(Register.INPMUX, (byte)positiveSource << 4 | (byte)negativeSource);
        WriteRegister(Register.REFMUX, (byte)positiveReference << 3 | (byte) negativeReference);

        gain1 = (byte)(1 << (byte)gain);
        vRef1 = (positiveReference, negativeReference) switch {
            (Adc1ReferenceP.Internal, Adc1ReferenceN.Internal) => 2.5,
            (_,_) => referenceVoltage
        };
    }

    /// <summary>
    /// Configures the ADC2 hardware (24-bit ADC).
    /// </summary>
    /// <param name="positiveSource">Enumeration specifying the positive ADC input</param>
    /// <param name="negativeSource">Enumeration specifying the negative ADC input</param>
    /// <param name="gain">Enumeration specifying channel gain.</param>
    /// <param name="rate">Enumeration specifying the sampling rate internal to the ADC</param>
    /// <param name="referenceVoltage">Reference voltage which needs to be specified if using external references</param>
    /// <param name="reference">Enumeration specifying the positive and negative reference source</param>
    public void ConfigureADC2(
        AdcSource positiveSource = AdcSource.AIN0, AdcSource negativeSource = AdcSource.AIN1,
        Adc2Gain gain = Adc2Gain.Gain_1, Adc2DataRate rate = Adc2DataRate.SPS_10, 
        double referenceVoltage = 5.0, Adc2Reference reference = Adc2Reference.Internal)
    {
        WriteRegister(Register.ADC2CFG, (byte)rate << 6 | (byte)reference << 3 | (byte)gain);
        WriteRegister(Register.ADC2MUX, (byte)positiveSource << 4 | (byte)negativeSource);

        gain2 = (byte)(1 << (byte)gain);
        vRef2 = reference switch
        {
            Adc2Reference.Internal => 2.5,
            _ => referenceVoltage
        };
    }

    /// <summary>
    /// Read a single byte register from the Ads1263
    /// </summary>
    /// <param name="register"><see cref="Register"/> to be read</param>
    /// <returns>byte value of register</returns>
    public byte ReadRegister(Register register)
    {
        Span<byte> command = stackalloc byte[] { (byte)((byte)OpCode.RREG | (byte)register), 0x00 };
        Span<byte> response = stackalloc byte[1];
        SpiComms.Exchange(command, response);
        return response[0];
    }

    /// <summary>
    /// Write a single byte register to the Ads1263
    /// </summary>
    /// <param name="register"><see cref="Register"/> to be written</param>
    /// <param name="data">byte (in int variable) to write</param>
    public void WriteRegister(Register register, int data)
    {
        Span<byte> command = stackalloc byte[] { (byte)((byte)OpCode.WREG | (byte)register), 0x00, (byte)data };
        SpiComms.Write(command);
    }

    #endregion

    #region AnalogInputPort
    /// <inheritdoc />
    public IAnalogInputPort CreateAnalogInputPort(IPin pin, int sampleCount, TimeSpan sampleInterval, Voltage referenceVoltage)
    {
        var channel = pin.SupportedChannels?.OfType<IAnalogChannelInfo>().FirstOrDefault();

        return channel == null
            ? throw new NotSupportedException($"Pin {pin.Name} does not support ADC")
            : (IAnalogInputPort)new AnalogInputPort(this, pin, channel, sampleCount, sampleInterval);
    }

    /// <inheritdoc />
    public IAnalogInputArray CreateAnalogInputArray(params IPin[] pins)
    {
        throw new NotImplementedException();
    }

    private void ADCStart(IPin pin)
    {
        var key = (byte)pin.Key;

        var opCode = key switch { 0x00 => OpCode.START1, _ => OpCode.START2 };
        // Span<byte> response = stackalloc byte[6]; // status, 32 bits, checksum

        SpiComms.Write((byte)opCode);
        // SpiComms.Read(response);
    }

    private void ADCStop(IPin pin)
    {
        var key = (byte)pin.Key;

        var opCode = key switch { 0x00 => OpCode.STOP1, _ => OpCode.STOP2 };
        // Span<byte> response = stackalloc byte[6]; // status, 32 bits, checksum

        SpiComms.Write((byte)opCode);
        // SpiComms.Read(response);
    }

    private Voltage ReadAnalog(IPin pin)
    {
        var key = (byte)pin.Key;

        var opCode = key switch { 0x00 => OpCode.RDATA1, _ => OpCode.RDATA2 };
        Span<byte> response = stackalloc byte[6]; // status, 32 bits, checksum

        SpiComms.Write((byte)opCode);
        SpiComms.Read(response);

        // TODO: Handle the status and checksum bytes

        // NOTE: Always treats the response as a 32 bit number (24-bit result is padded with zeros)
        int rawValue = response[1] << 24 | response[2] << 16 | response[3] << 8 | response[4];
        var vRef = key switch { 0x00 => vRef1, _ => vRef2 };
        var gain = key switch { 0x00 => gain1, _ => gain2 };

        // TODO: verify conversion/scaling
        var result = (vRef / gain) * (rawValue  / (double)0x7FFFFFFF);

        return new Voltage(result, Voltage.UnitType.Volts);
    }

    /// <summary> Helper function to convert the internal temperature sensor reading </summary>
    /// <param name="tempSensorVoltage">Voltage read from temp sensor channel</param>
    /// <returns>Temperature of IC</returns>
    public static Temperature ConvertTempSensor(Voltage tempSensorVoltage)
    {
        return new Temperature(((tempSensorVoltage.Microvolts - 122400) / 420) + 25, Temperature.UnitType.Celsius);
    }

    #endregion

    #region DigitalInputPort
    /// <summary>
    /// Creates a new DigitalInputPort using the specified GPIO pin
    /// </summary>
    /// <param name="pin">The pin representing the port</param>
    /// <param name="resistorMode">The port resistor mode (Not available on Ads1263)</param>
    /// <returns>IDigitalInputPort</returns>
    public IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode = ResistorMode.Disabled)
    {
        var channel = pin.SupportedChannels?.OfType<IDigitalChannelInfo>().FirstOrDefault();

        if (channel == null)
            throw new NotSupportedException($"Pin {pin.Name} does not support GPIO");

        PreValidatedSetPortEnable(pin);
        PreValidatedSetPortDirection(pin, PortDirectionType.Input);
        var port = new DigitalInputPort(pin);
        return port;
    }

    /// <summary>
    /// Reads the GPIODAT register. Assumes pin is already an input. Output pins always read 0.
    /// </summary>
    private bool ReadPort(IPin pin)
    {
        var ioInputs = ReadRegister(Register.GPIODAT);
        return BitHelpers.GetBitValue(ioInputs, (byte)pin.Key);
    }
    #endregion

    #region DigitalOutputPort
    /// <summary>
    /// Creates a new DigitalOutputPort using the specified pin and initial state
    /// </summary>
    /// <param name="pin">The pin representing the port</param>
    /// <param name="initialState">Whether the pin is initially high or low</param>
    /// <param name="outputType">The output type (always <see cref="OutputType.PushPull"/> for Ads1263)</param>
    /// <returns>IDigitalOutputPort</returns>
    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType outputType = OutputType.PushPull)
    {
        var channel = pin.SupportedChannels?.OfType<IDigitalChannelInfo>().FirstOrDefault();

        if (channel == null)
            throw new NotSupportedException($"Pin {pin.Name} does not support GPIO");

        // setup the port on the device for output
        PreValidatedSetPortEnable(pin);
        PreValidatedSetPortDirection(pin, PortDirectionType.Output);

        // create the port model object
        var port = new DigitalOutputPort(pin, initialState);

        port.SetPinState += PreValidatedWriteToPort;

        port.State = initialState;

        return port;
    }
    #endregion

    #region Digital 
    /// <summary>
    /// Sets the GPIO configuration of a port using pre-cached information. This overload
    /// assumes the pin has been pre-verified as valid.
    /// </summary>
    private void PreValidatedSetPortEnable(IPin pin, bool enable = true)
    {
        if (BitHelpers.GetBitValue(ioEnable, (byte)pin.Key))
        { return; }

        ioEnable = BitHelpers.SetBit(ioEnable, (byte)pin.Key, enable);
        WriteRegister(Register.GPIOCON, ioEnable);
    }

    /// <summary>
    /// Sets the direction of a port using pre-cached information. This overload
    /// assumes the pin has been pre-verified as valid.
    /// </summary>
    private void PreValidatedSetPortDirection(IPin pin, PortDirectionType direction)
    {
        if (direction == PortDirectionType.Input)
        {
            if (BitHelpers.GetBitValue(ioDir, (byte)pin.Key))
            { return; }
        }
        else
        {
            if (!BitHelpers.GetBitValue(ioDir, (byte)pin.Key))
            { return; }
        }

        ioDir = BitHelpers.SetBit(ioDir, (byte)pin.Key, (byte)direction);
        WriteRegister(Register.GPIODIR, ioDir);
    }

    /// <summary>
    /// Sets a particular pin's value. If that pin is not 
    /// in output mode, this method will first set its 
    /// mode to output.
    /// 
    /// This overload takes in cached details that are assumed
    /// to be accurate for better performance.
    /// </summary>
    private void PreValidatedWriteToPort(IPin pin, bool value)
    {
        lock (_lock)
        {
            PreValidatedSetPortDirection(pin, PortDirectionType.Output);

            // update the output cache
            ioOut = BitHelpers.SetBit(ioOut, (byte)pin.Key, value);

            // write to the output latch (actually does the output setting)
            WriteRegister(Register.GPIODAT, ioOut);
        }
    }
    #endregion

    #region IDisposable
    /// <summary>
    /// Is the object disposed
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of the object
    /// </summary>
    /// <param name="disposing">Is disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing && createdPort)
            {
                chipSelectPort?.Dispose();
            }

            IsDisposed = true;
        }
    }
    #endregion
}
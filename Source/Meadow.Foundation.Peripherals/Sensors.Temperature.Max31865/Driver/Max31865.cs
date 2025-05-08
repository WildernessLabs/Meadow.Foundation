using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature;

/// <summary>
/// Driver class for the MAX31865 resistance-to-digital converter.
/// Provides temperature readings from Resistance Temperature Detectors such as the PT100 and PT1000.
/// </summary>
/// <remarks>
/// This class implements the ITemperatureSensor interface for temperature readings
/// and ISpiPeripheral interface for SPI communication. It follows the IDisposable pattern
/// for proper resource management.
/// </remarks>
public class Max31865 : SamplingSensorBase<Temperature>, ITemperatureSensor, ISpiPeripheral, IDisposable
{
    private static class Configuration
    {
        public const byte Bias = 0b_1000_0000;
        public const byte ModeAuto = 0b_0100_0000;
        public const byte ModeOff = 0x00;
        public const byte OneShot = 0b_0010_0000;
        public const byte ThreeWire = 0b_0001_0000;
        public const byte TwoFourWire = 0b_0000_0000;
        public const byte FaultStat = 0b_0000_0010;
        public const byte Filter50Hz = 0b_0000_0001;
        public const byte Filter60Hz = 0b_0000_0000;
    }

    private static class Registers
    {
        public const byte ConfigurationRead = 0x00;
        public const byte RtdMsb = 0x01;
        public const byte RtdLsb = 0x02;
        public const byte HFaultMsb = 0x03;
        public const byte HFaultLsb = 0x04;
        public const byte LFaultMsb = 0x05;
        public const byte LFaultLsb = 0x06;
        public const byte FaultStat = 0x07;
        public const byte ConfigurationWrite = 0x80;
    }

    public static class Fault
    {
        public const byte HighThresh = 0x80;
        public const byte LowThresh = 0x40;
        public const byte RefInLow = 0x20;
        public const byte RefInHigh = 0x10;
        public const byte RtdInLow = 0x08;
        public const byte OvUv = 0x04;
    }

    public enum Wires : byte
    {
        /// <summary>
        /// Two wire sensor.
        /// </summary>
        TwoWire = 0,

        /// <summary>
        /// Three wire sensor.
        /// </summary>
        ThreeWire = 1,

        /// <summary>
        /// Four wire sensor.
        /// </summary>
        FourWire = 0
    }

    /// <summary>
    /// Type of temperature sensor.
    /// </summary>
    public enum KnownSensorType : short
    {
        /// <summary>
        /// Platinum Thermometer 100 - Resistance Temperature Detector (RTD) with a nominal resistance of 100 ohms at 0°C.
        /// </summary>
        PT100 = 100,

        /// <summary>
        /// Platinum Thermometer 1000 - Resistance Temperature Detector (RTD) with a nominal resistance of 1000 ohms at 0°C.
        /// </summary>
        PT1000 = 1000
    }

    /// <summary>
    /// Notch frequencies for the noise rejection filter
    /// </summary>
    public enum ConversionFilterMode : byte
    {
        /// <summary>
        /// Reject 50Hz and its harmonics
        /// </summary>
        Filter50Hz = 65,

        /// <summary>
        /// Reject 60Hz and its harmonics
        /// </summary>
        Filter60Hz = 55
    }

    /// <summary>
    /// Internal SPI communication handler for device operations.
    /// </summary>
    private readonly SpiCommunications spiComms;

    /// <summary>
    /// Default SPI bus speed (4 MHz) for MAX31865 communication.
    /// </summary>
    private readonly Frequency defaultSpeed = new Frequency(4, Frequency.UnitType.Megahertz);

    /// <summary>
    /// Chip select digital output port for SPI communication.
    /// </summary>
    private readonly IDigitalOutputPort? port;

    /// <summary>
    /// Indicates whether the digital output port was created internally.
    /// </summary>
    private readonly bool portCreated = false;

    private readonly KnownSensorType knownSensorType;

    private readonly Wires wires;

    private readonly int referenceResistor;

    private readonly ConversionFilterMode conversionFilterMode;

    private readonly IResistanceTemperatureDetectorConverter rtdConverter;

    /// <summary>
    /// Gets whether this object has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// Gets the default SPI bus speed for MAX31865 communication.
    /// </summary>
    public Frequency DefaultSpiBusSpeed => defaultSpeed;

    /// <summary>
    /// Gets the default SPI bus mode (Mode 1) for MAX31865 communication.
    /// </summary>
    public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode1;

    /// <summary>
    /// Initializes a new instance of the <see cref="Max31865"/> class using an <see cref="IPin"/> for chip select.
    /// </summary>
    /// <param name="bus">The SPI bus instance.</param>
    /// <param name="chipSelect">The chip select pin. If null, chip select must be managed externally.</param>
    /// <param name="knownSensorType">The type of Platinum Resistance Thermometer.</param>
    /// <param name="wires">The number of wires the Platinum Resistance Thermometer has.</param>
    /// <param name="referenceResistor">The reference resistor value in Ohms.</param>
    /// <param name="filterMode">Noise rejection filter mode.</param>
    public Max31865(ISpiBus bus, IPin? chipSelect, KnownSensorType knownSensorType, Wires wires, int referenceResistor, ConversionFilterMode filterMode)
        : this(bus, chipSelect?.CreateDigitalOutputPort(true), knownSensorType, wires, referenceResistor, filterMode)
    {
        portCreated = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Max31865"/> class using a digital output port for chip select.
    /// </summary>
    /// <param name="bus">The SPI bus instance.</param>
    /// <param name="chipSelect">The chip select digital output port. If null, chip select must be managed externally.</param>
    /// <param name="knownSensorType">The type of Platinum Resistance Thermometer.</param>
    /// <param name="wires">The number of wires the Platinum Resistance Thermometer has.</param>
    /// <param name="referenceResistor">The reference resistor value in Ohms.</param>
    /// <param name="filterMode">Noise rejection filter mode.</param>
    public Max31865(ISpiBus bus, IDigitalOutputPort? chipSelect, KnownSensorType knownSensorType, Wires wires, int referenceResistor, ConversionFilterMode filterMode)
    {
        port = chipSelect;
        spiComms = new SpiCommunications(bus, chipSelect, DefaultSpiBusSpeed);
        this.knownSensorType = knownSensorType;
        this.wires = wires;
        rtdConverter = new DirectMathematicalMethod();
        conversionFilterMode = filterMode;

        Initialize();
    }

    /// <summary>
    /// Gets or sets the SPI bus mode for communication.
    /// </summary>
    public SpiClockConfiguration.Mode SpiBusMode
    {
        get => spiComms.BusMode;
        set => spiComms.BusMode = value;
    }

    /// <summary>
    /// Gets or sets the SPI bus speed for communication.
    /// </summary>
    public Frequency SpiBusSpeed
    {
        get => spiComms.BusSpeed;
        set => spiComms.BusSpeed = value;
    }

    /// <inheritdoc />
    protected override async Task<Temperature> ReadSensor()
    {
        var rtd = await ReadInternal();

        return new Temperature(this.rtdConverter.Convert(rtd, (float)knownSensorType, referenceResistor));
    }

    /// <summary>
    /// Starts continuously sampling the sensor.
    ///
    /// This method also starts raising `Changed` events and IObservable
    /// subscribers getting notified. Use the `readIntervalDuration` parameter
    /// to specify how often events and notifications are raised/sent.
    /// </summary>
    /// <param name="updateInterval">A `TimeSpan` that specifies how long to
    /// wait between readings. This value influences how often `*Updated`
    /// events are raised and `IObservable` consumers are notified.
    ///</param>
    public override void StartUpdating(TimeSpan? updateInterval = null)
    {
        lock (samplingLock)
        {
            if (IsSampling) { return; }
            IsSampling = true;

            if (updateInterval.HasValue)
            {
                TimeSpan valueOrDefault = updateInterval.GetValueOrDefault();
                base.UpdateInterval = valueOrDefault;
            }

            SamplingTokenSource = new CancellationTokenSource();
            CancellationToken ct = SamplingTokenSource.Token;
            Task.Run(async delegate
            {
                while (!ct.IsCancellationRequested)
                {
                    var result = await ReadSensor();
                    
                    // create a new change result from the new value
                    ChangeResult<Units.Temperature> changeResult = new ChangeResult<Units.Temperature>()
                    {
                        New = result,
                        Old = Temperature
                    };
                    Temperature = changeResult.New;
                    RaiseEventsAndNotify(changeResult);

                    await Task.Delay(base.UpdateInterval);
                }

                base.Observers.ForEach(delegate (IObserver<IChangeResult<Units.Temperature>> x)
                {
                    x.OnCompleted();
                });
            }, SamplingTokenSource.Token).RethrowUnhandledExceptions(SamplingTokenSource.Token);
        }
    }

    /// <summary>
    /// Stops sampling the temperature
    /// </summary>
    public override void StopUpdating()
    {
        lock (samplingLock)
        {
            if (!IsSampling) { return; }
            IsSampling = false;
        }
    }

    public byte ReadFault()
    {
        return this.spiComms.ReadRegister(Registers.FaultStat);
    }

    private async Task<ushort> ReadInternal()
    {
        ClearFault();
        Enable(Configuration.Bias);
        await Task.Delay(10);

        Enable(Configuration.OneShot);
        await Task.Delay(65);

        var rtd = this.spiComms.ReadRegisterAsUShort(Registers.RtdMsb, ByteOrder.LittleEndian);

        // Disable bias current again to reduce self-heating.
        Disable(Configuration.Bias);

        // // remove the fault bit
        rtd >>= 1;

        return rtd;
    }

    private void ClearFault()
    {
        byte value = this.spiComms.ReadRegister(Registers.ConfigurationRead);

        // Page 14 (Fault Status Clear (D1)) of the technical documentation
        value = (byte)(value & ~0x2C);
        value |= Configuration.FaultStat;

        this.spiComms.WriteRegister(Registers.ConfigurationWrite, value);
    }

    private void Enable(byte configuration)
    {
        byte value = this.spiComms.ReadRegister(Registers.ConfigurationRead);

        value |= configuration;

        this.spiComms.WriteRegister(Registers.ConfigurationWrite, value);
    }

    private void Disable(byte configuration)
    {
        byte value = this.spiComms.ReadRegister(Registers.ConfigurationRead);

        value &= (byte)~configuration;

        this.spiComms.WriteRegister(Registers.ConfigurationWrite, value);
    }

    private void Initialize()
    {
        byte value = 
            wires == Wires.ThreeWire ? Configuration.ThreeWire : Configuration.TwoFourWire |
            conversionFilterMode == ConversionFilterMode.Filter50Hz ? Configuration.Filter50Hz : Configuration.Filter60Hz;

        this.spiComms.WriteRegister(Registers.ConfigurationWrite, value);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources;
    /// <c>false</c> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                if (portCreated)
                {
                    port?.Dispose();
                }
            }

            IsDisposed = true;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public interface IResistanceTemperatureDetectorConverter
{
    float Convert(float raw, float nominal, float referenceResistor);
}

public class DirectMathematicalMethod : IResistanceTemperatureDetectorConverter
{
    private const float RTD_A = 3.9083e-3f;
    private const float RTD_B = -5.775e-7f;

    public float Convert(float rtdRaw, float rtdNominal, float refResistor)
    {
        float Z1, Z2, Z3, Z4, resistance, temp;
        
        // This math originates from:
        // http://www.analog.com/media/en/technical-documentation/application-notes/AN709_0.pdf
        resistance = rtdRaw;
        resistance /= 32768;
        resistance *= refResistor;

        Z1 = -RTD_A;
        Z2 = RTD_A * RTD_A - (4 * RTD_B);
        Z3 = (4 * RTD_B) / rtdNominal;
        Z4 = 2 * RTD_B;

        temp = Z2 + (Z3 * resistance);
        temp = (MathF.Sqrt(temp) + Z1) / Z4;

        if (temp >= 0)
        {
            return temp;
        }

        resistance /= rtdNominal;
        resistance *= 100; // normalize to 100 ohm

        float rpoly = resistance;

        temp = -242.02f;
        temp += 2.2228f * rpoly;
        rpoly *= resistance; // square
        temp += 2.5859e-3f * rpoly;
        rpoly *= resistance; // ^3
        temp -= 4.8260e-6f * rpoly;
        rpoly *= resistance; // ^4
        temp -= 2.8183e-8f * rpoly;
        rpoly *= resistance; // ^5
        temp += 1.5243e-10f * rpoly;

        return temp;
    }
}
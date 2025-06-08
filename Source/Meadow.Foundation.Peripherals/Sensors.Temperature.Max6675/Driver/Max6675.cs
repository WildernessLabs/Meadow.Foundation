using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature;

/// <summary>
/// Driver class for the MAX6675 thermocouple-to-digital converter.
/// Provides temperature readings from K-type thermocouples with 12-bit resolution.
/// </summary>
/// <remarks>
/// This class implements the ITemperatureSensor interface for temperature readings
/// and ISpiPeripheral interface for SPI communication. It follows the IDisposable pattern
/// for proper resource management.
/// </remarks>
public class Max6675 : ITemperatureSensor, ISpiPeripheral, IDisposable
{
    /// <summary>
    /// Internal SPI communication handler for device operations.
    /// </summary>
    private readonly SpiCommunications spiComms;

    /// <summary>
    /// Default SPI bus speed (4 MHz) for MAX6675 communication.
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

    /// <summary>
    /// Gets whether this object has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// Gets the default SPI bus speed for MAX6675 communication.
    /// </summary>
    public Frequency DefaultSpiBusSpeed => defaultSpeed;

    /// <summary>
    /// Gets the default SPI bus mode (Mode 0) for MAX6675 communication.
    /// </summary>
    public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

    /// <summary>
    /// Initializes a new instance of the <see cref="Max6675"/> class using an <see cref="IPin"/> for chip select.
    /// </summary>
    /// <param name="bus">The SPI bus instance.</param>
    /// <param name="chipSelect">The chip select pin. If null, chip select must be managed externally.</param>
    public Max6675(ISpiBus bus, IPin? chipSelect)
        : this(bus, chipSelect?.CreateDigitalOutputPort(true))
    {
        portCreated = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Max6675"/> class using a digital output port for chip select.
    /// </summary>
    /// <param name="bus">The SPI bus instance.</param>
    /// <param name="chipSelect">The chip select digital output port. If null, chip select must be managed externally.</param>
    public Max6675(ISpiBus bus, IDigitalOutputPort? chipSelect)
    {
        port = chipSelect;
        spiComms = new SpiCommunications(bus, chipSelect, DefaultSpiBusSpeed);
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

    /// <summary>
    /// Reads the current temperature from the MAX6675 thermocouple converter.
    /// </summary>
    /// <returns>A task containing the temperature in Celsius.</returns>
    /// <exception cref="Exception">Thrown when the thermocouple input is open (not connected).</exception>
    /// <remarks>
    /// The MAX6675 returns 12-bit temperature data with a resolution of 0.25°C.
    /// If bit D2 of the raw data is high, it indicates an open thermocouple connection.
    /// </remarks>
    public Task<Units.Temperature> Read()
    {
        Span<byte> buffer = stackalloc byte[2];
        spiComms.Read(buffer);
        var raw = (ushort)((buffer[0] << 8) | buffer[1]);

        // if bit D2 is high, the thermocouple is open
        if ((raw & (1 << 2)) != 0)
        {
            throw new Exception("Thermocouple input is open");
        }

        var t = Convert.ToSingle((raw >> 3)) * 0.25;
        return Task.FromResult(new Units.Temperature(t, Units.Temperature.UnitType.Celsius));
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
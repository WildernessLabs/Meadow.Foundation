using Meadow.Foundation.Sensors.Gnss;
using Meadow.Hardware;

namespace Meadow.Foundation.mikroBUS.Sensors.Gnss;

/// <summary>
/// Represents a mikroBUS GNSS 10 board (Neo M8) 
/// </summary>
public class CGNSS10 : NeoM8
{
    /// <summary>
    /// Creates a new CGNSS10 object
    /// </summary>
    public CGNSS10(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalOutputPort? resetPort = null)
        : base(spiBus, chipSelectPort, resetPort)
    { }

    /// <summary>
    /// Creates a new CGNSS10 object
    /// </summary>
    public CGNSS10(ISpiBus spiBus, IPin chipSelectPin, IPin? resetPin = null)
        : base(spiBus, chipSelectPin, resetPin)
    { }
}

/// <summary>
/// Represents a CGNSS10Serial device, a specialized implementation of the NeoM8 GPS module that communicates over a
/// serial connection.
/// </summary>
/// <remarks>This class is designed to work with a MikroBusConnector, utilizing its serial port and reset pin for
/// communication and control. It provides an interface for interacting with the CGNSS10Serial hardware
/// module.</remarks>
public class CGNSS10Serial : NeoM8
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CGNSS10Serial"/> class,  providing access to the GNSS module via
    /// the specified MikroBus connector.
    /// </summary>
    /// <remarks>This constructor sets up the communication with the GNSS module by utilizing  the serial port
    /// and reset pin provided by the <paramref name="mikroBusConnector"/>.  Ensure that the <see
    /// cref="MikroBusConnector"/> is properly configured before  initializing this class.</remarks>
    /// <param name="mikroBusConnector">The <see cref="MikroBusConnector"/> instance that provides the serial port name  and reset pin required to
    /// communicate with the GNSS module.</param>
    public CGNSS10Serial(MikroBusConnector mikroBusConnector)
        : base(
            mikroBusConnector.SerialPortName,
            mikroBusConnector.Pins.RST)
    { }
}

/// <summary>
/// Represents the CGNSS10 SPI-based GPS module, providing functionality for communication with the device over an SPI
/// interface.
/// </summary>
/// <remarks>This class is specifically designed for use with the CGNSS10 GPS module and requires a <see
/// cref="MikroBusConnector"/> to initialize the SPI bus and associated control pins.</remarks>
public class CGNSS10Spi : NeoM8
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CGNSS10Spi"/> class, providing SPI communication and control for
    /// the CGNSS10 module.
    /// </summary>
    /// <remarks>This constructor sets up the SPI communication and control pins for the CGNSS10 module using
    /// the provided <see cref="MikroBusConnector"/>. Ensure that the <paramref name="mikroBusConnector"/> is properly
    /// configured and connected to the appropriate hardware before initializing this class.</remarks>
    /// <param name="mikroBusConnector">The <see cref="MikroBusConnector"/> instance that provides the SPI bus and associated control pins (chip select
    /// and reset) required to communicate with the CGNSS10 module.</param>
    public CGNSS10Spi(MikroBusConnector mikroBusConnector)
        : base(
            mikroBusConnector.SpiBus,
            mikroBusConnector.Pins.CS,
            mikroBusConnector.Pins.RST)
    { }
}

/// <summary>
/// Represents a CGNSS10 I2C-based GPS module, providing communication and control functionality over the I2C bus.
/// </summary>
/// <remarks>This class is specifically designed for use with the CGNSS10 GPS module and inherits from the <see
/// cref="NeoM8"/> base class. It initializes the module using the I2C bus and reset pin provided by the specified <see
/// cref="MikroBusConnector"/>.</remarks>
public class CGNSS10I2c : NeoM8
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CGNSS10I2c"/> class, providing access to GNSS functionality over an
    /// I2C interface.
    /// </summary>
    /// <remarks>This constructor sets up the GNSS module using the default I2C address and the reset pin
    /// provided by the specified <see cref="MikroBusConnector"/>. Ensure that the <paramref name="mikroBusConnector"/>
    /// is properly configured before initializing this class.</remarks>
    /// <param name="mikroBusConnector">The <see cref="MikroBusConnector"/> instance that provides the I2C bus and reset pin required for communication
    /// with the GNSS module.</param>
    public CGNSS10I2c(MikroBusConnector mikroBusConnector)
        : base(
            mikroBusConnector.I2cBus,
            (byte)NeoM8.Addresses.Default,
            mikroBusConnector.Pins.RST)
    { }
}
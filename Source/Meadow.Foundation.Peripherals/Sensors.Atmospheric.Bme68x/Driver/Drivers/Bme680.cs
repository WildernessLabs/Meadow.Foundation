using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// BME680 Temperature, Pressure, Humidity and gas sensor
    /// </summary>
    /// <remarks>
    /// This class implements the functionality necessary to read the temperature, pressure, humidity and VOS
    /// </remarks>
    namespace Meadow.Foundation.Sensors.Atmospheric
    {
        /// <summary>
        /// Bme680 Temperature, Pressure and Humidity Sensor
        /// </summary>
        /// <remarks>
        /// This class implements the functionality necessary to read the temperature, pressure and humidity
        /// from the Bosch BME680 sensor
        /// </remarks>
        public partial class Bme680 : Bme68x
        {
            /// <summary>
            /// Creates a new instance of the Bme680 class
            /// </summary>
            /// <param name="i2cBus">I2C Bus to use for communicating with the sensor</param>
            /// <param name="address">I2C address of the sensor</param>
            public Bme680(II2cBus i2cBus, byte address = (byte)Addresses.Default) : base(i2cBus, address)
            { }

            /// <summary>
            /// Creates a new instance of the Bme680 class
            /// </summary>
            /// <param name="device">The Meadow device to create the chip select port</param>
            /// <param name="spiBus">The SPI bus connected to the device</param>
            /// <param name="chipSelectPin">The chip select pin</param>
            public Bme680(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin) :
                base(spiBus, device.CreateDigitalOutputPort(chipSelectPin))
            { }

            /// <summary>
            /// Creates a new instance of the Bme680 class
            /// </summary>
            /// <param name="spiBus">The SPI bus connected to the device</param>
            /// <param name="chipSelectPort">The chip select pin</param>
            /// <param name="configuration">The BMP68x configuration (optional)</param>
            public Bme680(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, Configuration? configuration = null) :
                base(spiBus, chipSelectPort)
            { }
        }
    }
    /// </summary>
    /// <remarks>
    /// This class implements the functionality necessary to read the temperature, pressure, humidity and VOS
    /// from the Bosch BME680 sensor
    /// </remarks>
    public partial class Bme680 : Bme68x
    {
        /// <summary>
        /// Creates a new instance of the Bme680 class
        /// </summary>
        /// <param name="i2cBus">I2C Bus to use for communicating with the sensor</param>
        /// <param name="address">I2C address of the sensor</param>
        public Bme680(II2cBus i2cBus, byte address = (byte)Addresses.Default) : base(i2cBus, address)
        { }

        /// <summary>
        /// Creates a new instance of the Bme680 class
        /// </summary>
        /// <param name="device">The Meadow device to create the chip select port</param>
        /// <param name="spiBus">The SPI bus connected to the device</param>
        /// <param name="chipSelectPin">The chip select pin</param>
        public Bme680(IMeadowDevice device, ISpiBus spiBus, IPin chipSelectPin) :
            base(spiBus, device.CreateDigitalOutputPort(chipSelectPin))
        { }

        /// <summary>
        /// Creates a new instance of the Bme680 class
        /// </summary>
        /// <param name="spiBus">The SPI bus connected to the device</param>
        /// <param name="chipSelectPort">The chip select pin</param>
        /// <param name="configuration">The BMP68x configuration (optional)</param>
        public Bme680(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, Configuration? configuration = null) :
            base(spiBus, chipSelectPort)
        { }
    }
}
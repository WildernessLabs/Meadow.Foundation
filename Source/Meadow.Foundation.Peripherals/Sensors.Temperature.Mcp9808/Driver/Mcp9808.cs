using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// Represents a Mcp9808 temperature sensor
    /// </summary>
    public partial class Mcp9808 : ByteCommsSensorBase<Units.Temperature>,
        ITemperatureSensor, II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Raised when the temeperature value changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        /// <summary>
        /// Creates a new Mcp9808 object
        /// </summary>
        /// <param name="i2CBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Mcp9808(II2cBus i2CBus, byte address = (byte)Addresses.Default)
            : base(i2CBus, address, readBufferSize: 8, writeBufferSize: 8)
        {
            BusComms?.WriteRegister(Registers.REG_CONFIG, (ushort)0x0);
        }

        /// <summary>
		/// Wake the the device if it's in sleep state
		/// </summary>
        public void Wake()
        {
            ushort config = BusComms?.ReadRegisterAsUShort(Registers.REG_CONFIG, ByteOrder.BigEndian) ?? 0;

            config = (ushort)(config & (~Registers.CONFIG_SHUTDOWN));

            BusComms?.WriteRegister(Registers.REG_CONFIG, config);
        }

        /// <summary>
		/// Set the device into a low power sleep state
		/// </summary>
        public void Sleep()
        {
            ushort config = BusComms?.ReadRegisterAsUShort(Registers.REG_CONFIG, ByteOrder.BigEndian) ?? 0;

            BusComms?.WriteRegister(Registers.REG_CONFIG, (ushort)(config | Registers.CONFIG_SHUTDOWN));
        }

        /// <summary>
		/// Read the device ID 
		/// </summary>
        public ushort GetDeviceId()
        {
            return BusComms?.ReadRegisterAsUShort(Registers.DEVICE_ID, ByteOrder.BigEndian) ?? 0;
        }

        /// <summary>
		/// Read the manufacture ID 
		/// </summary>
        public ushort GetManufactureId()
        {
            return BusComms?.ReadRegisterAsUShort(Registers.MANUFACTURER_ID, ByteOrder.BigEndian) ?? 0;
        }

        /// <summary>
		/// Read resolution
		/// </summary>
        public byte GetResolution()
        {
            return BusComms?.ReadRegister(Registers.RESOLUTION) ?? 0;
        }

        /// <summary>
		/// Set resolution
		/// </summary>
        public void SetResolution(byte resolution)
        {
            BusComms?.WriteRegister(Registers.RESOLUTION, resolution);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<Units.Temperature> ReadSensor()
        {
            ushort value = BusComms?.ReadRegisterAsUShort(Registers.AMBIENT_TEMP, ByteOrder.BigEndian) ?? 0;
            double temp = value & 0x0FFF;

            temp /= 16.0;

            if ((value & 0x1000) != 0)
            {
                temp -= 256;
            }

            return Task.FromResult(new Units.Temperature(temp, Units.Temperature.UnitType.Celsius));
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<Units.Temperature> changeResult)
        {
            TemperatureUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}
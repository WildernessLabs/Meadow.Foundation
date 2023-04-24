using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// Represents a Mcp9808 temperature sensor
    /// </summary>
    public partial class Mcp9808 : ByteCommsSensorBase<Units.Temperature>, ITemperatureSensor
    {
        /// <summary>
        /// Raised when the temeperature value changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        const ushort MCP_CONFIG_SHUTDOWN = 0x0100;   // shutdown config
        const ushort MCP_CONFIG_CRITLOCKED = 0x0080; // critical trip lock
        const ushort MCP_CONFIG_WINLOCKED = 0x0040; // alarm window lock
        const ushort MCP_CONFIG_INTCLR = 0x0020;     // interrupt clear
        const ushort MCP_CONFIG_ALERTSTAT = 0x0010;  // alert output status
        const ushort MCP_CONFIG_ALERTCTRL = 0x0008;  // alert output control
        const ushort MCP_CONFIG_ALERTSEL = 0x0004;  // alert output select
        const ushort MCP_CONFIG_ALERTPOL = 0x0002;   // alert output polarity
        const ushort MCP_CONFIG_ALERTMODE = 0x0001;  // alert output mode

        const byte MCP_REG_CONFIG = 0x01;        // config
        const byte MCP_UPPER_TEMP = 0x02;     // upper alert boundary
        const byte MCP_LOWER_TEMP = 0x03;     // lower alert boundery
        const byte MCP_CRIT_TEMP = 0x04;     // critical temperature
        const byte MCP_AMBIENT_TEMP = 0x05;   // ambient temperature
        const byte MCP_MANUFACTURER_ID = 0x06; // manufacturer ID
        const byte MCP_DEVICE_ID = 0x07;    // device ID
        const byte MCP_RESOLUTION = 0x08;     // resolution

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
            BusComms?.WriteRegister(MCP_REG_CONFIG, (ushort)0x0);
        }

        /// <summary>
		/// Wake the the device if it's in sleep state
		/// </summary>
        public void Wake()
        {
            ushort config = BusComms?.ReadRegisterAsUShort(MCP_REG_CONFIG, ByteOrder.BigEndian) ?? 0;

            config = (ushort)(config & (~MCP_CONFIG_SHUTDOWN));

            BusComms?.WriteRegister(MCP_REG_CONFIG, config);
        }

        /// <summary>
		/// Set the device into a low power sleep state
		/// </summary>
        public void Sleep()
        {
            ushort config = BusComms?.ReadRegisterAsUShort(MCP_REG_CONFIG, ByteOrder.BigEndian) ?? 0;

            BusComms?.WriteRegister(MCP_REG_CONFIG, (ushort)(config | MCP_CONFIG_SHUTDOWN));
        }

        /// <summary>
		/// Read the device ID 
		/// </summary>
        public ushort GetDeviceId()
        {
            return BusComms?.ReadRegisterAsUShort(MCP_DEVICE_ID, ByteOrder.BigEndian) ?? 0;
        }

        /// <summary>
		/// Read the manufacture ID 
		/// </summary>
        public ushort GetManufactureId()
        {
            return BusComms?.ReadRegisterAsUShort(MCP_MANUFACTURER_ID, ByteOrder.BigEndian) ?? 0;
        }

        /// <summary>
		/// Read resolution
		/// </summary>
        public byte GetResolution()
        {
            return BusComms?.ReadRegister(MCP_RESOLUTION) ?? 0;
        }

        /// <summary>
		/// Set resolution
		/// </summary>
        public void SetResolution(byte resolution)
        {
            BusComms?.WriteRegister(MCP_RESOLUTION, resolution);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<Units.Temperature> ReadSensor()
        {
            ushort value = BusComms?.ReadRegisterAsUShort(MCP_AMBIENT_TEMP, ByteOrder.BigEndian) ?? 0;
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
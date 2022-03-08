using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class Mcp9808 : ByteCommsSensorBase<Units.Temperature>, ITemperatureSensor
    {
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

        // <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        public Mcp9808(II2cBus i2CBus, byte address = (byte)Addresses.Default)
            : base(i2CBus, address, readBufferSize: 8, writeBufferSize: 8)
        {
            Peripheral.WriteRegister(MCP_REG_CONFIG, (ushort)0x0);
        }

        /// <summary>
		/// Wake the the device if it's in sleep state
		/// </summary>
        public void Wake()
        {
            ushort config = Peripheral.ReadRegisterAsUShort(MCP_REG_CONFIG, ByteOrder.BigEndian);

            config = (ushort)(config & (~MCP_CONFIG_SHUTDOWN));

            Peripheral.WriteRegister(MCP_REG_CONFIG, config);
        }

        /// <summary>
		/// Set the device into a low power sleep state
		/// </summary>
        public void Sleep()
        {
            ushort config = Peripheral.ReadRegisterAsUShort(MCP_REG_CONFIG, ByteOrder.BigEndian);

            Peripheral.WriteRegister(MCP_REG_CONFIG, (ushort)(config | MCP_CONFIG_SHUTDOWN));
         }

        /// <summary>
		/// Read the device ID 
		/// </summary>
        public ushort GetDeviceId()
        {
            return Peripheral.ReadRegisterAsUShort(MCP_DEVICE_ID, ByteOrder.BigEndian);
        }

        /// <summary>
		/// Read the manufacture ID 
		/// </summary>
        public ushort GetManufactureId()
        {
            return Peripheral.ReadRegisterAsUShort(MCP_MANUFACTURER_ID, ByteOrder.BigEndian);
        }

        /// <summary>
		/// Read resolution
		/// </summary>
        public byte GetResolution()
        {
            return Peripheral.ReadRegister(MCP_RESOLUTION);
        }

        /// <summary>
		/// Set resolution
		/// </summary>
        public void SetResolution(byte resolution)
        {
            Peripheral.WriteRegister(MCP_RESOLUTION, resolution);
        }

        protected override async Task<Units.Temperature> ReadSensor()
        {
            return await Task.Run(() =>
            {
                ushort value = Peripheral.ReadRegisterAsUShort(MCP_AMBIENT_TEMP, ByteOrder.BigEndian);
                double temp = value & 0x0FFF;

                temp /= 16.0;

                if ((value & 0x1000) != 0)
                {
                    temp -= 256;
                }

                var newTemp = new Units.Temperature(temp);
                Temperature = newTemp;

                return newTemp;
            });
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Units.Temperature> changeResult)
        {
            TemperatureUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}
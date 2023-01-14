using Meadow.Modbus;
using System.Threading.Tasks;

namespace Meadow.Foundation.Thermostats
{
    /// <summary>
    /// TEMCO T8 Thermostat Driver
    /// </summary>
    public partial class T8
    {
        private ModbusRtuClient _modbusClient;

        /// <summary>
        /// The thermostats address on the Modbus RTU bus
        /// </summary>
        public byte ModbusAddress { get; private set; }

        /// <summary>
        /// Creates an instance of a T8 thermostat
        /// </summary>
        /// <param name="modbusClient"></param>
        /// <param name="modbusAddress"></param>
        public T8(ModbusRtuClient modbusClient, byte modbusAddress)
        {
            if (!modbusClient.IsConnected)
            {
                modbusClient.Connect();
            }

            _modbusClient = modbusClient;
            ModbusAddress = modbusAddress;
        }

        /// <summary>
        /// Reads the thermostat's current temperature
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetCurrentTemperature()
        {
            return await ReadRegisterAsDouble(Register.CurrentTemperature, 0.1);
        }

        /// <summary>
        /// Reads the thermostat's current occupied setpoint
        /// </summary>
        /// <returns></returns>
        public async Task<double> GetOccupiedSetpoint()
        {
            return await ReadRegisterAsDouble(Register.OccupiedSetPoint, 0.1);
        }

        private async Task<double> ReadRegisterAsDouble(Register register, double scale)
        {
            var registerContents = await _modbusClient.ReadHoldingRegisters(ModbusAddress, (ushort)register, 1);
            return registerContents[0] * scale;
        }
    }
}
using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Foundation;
using Meadow.Units;
using System.Threading.Tasks;
using System;

namespace Meadow.Foundation.Sensors.Environmental
{
    // https://github.com/EnviroDIY/YosemitechModbus/

    /// <summary>
    /// Represents a Yosemitech Y4000 Multiparameter Sonde water quality sensor 
    /// for dissolved oxygen, conductivity, turbidity, pH, chlorophyll, 
    /// blue green algae, oil-in-water and temperature
    /// </summary>
    public partial class Y4000 //: PollingSensorBase<(Temperature? temperature, Concentration? concentration)>
    {
        readonly IModbusBusClient modbusClient;

        /// <summary>
        /// 9600 baud 8-N-1
        /// </summary>
        readonly ISerialPort serialPort;

        readonly byte ModbusAddress = 0x01;

        /// <summary>
        /// Creates a new Y4000 object
        /// </summary>
        public Y4000(IMeadowDevice device, SerialPortName serialPortName, IPin? enablePin = null)
        {   // 9600 baud 8-N-1
            serialPort = device.CreateSerialPort(serialPortName, 9600, 8, Parity.None, StopBits.One);
            serialPort.WriteTimeout = serialPort.ReadTimeout = TimeSpan.FromSeconds(5);

            if (enablePin != null)
            {
                var enablePort = device.CreateDigitalOutputPort(enablePin, false);
                modbusClient = new ModbusRtuClient(serialPort, enablePort);
            }
            else
            {
                modbusClient = new ModbusRtuClient(serialPort);
            }
        }

        /// <summary>
        /// Initialize sensor
        /// </summary>
        /// <returns></returns>
        public Task Initialize()
        {
            Console.WriteLine("Initialize");
            return modbusClient.Connect();
        }

        public async Task<ushort[]> GetISDN()
        {
            Console.WriteLine("GetISDN");

            var data = await modbusClient.ReadHoldingRegisters(0xFF, Registers.ISDN.Offset, Registers.ISDN.Length);

            return data;
        }

        public async Task<ushort[]> GetSerialNumber()
        {
            Console.WriteLine("GetSerialNumber");

            var data = await modbusClient.ReadHoldingRegisters(ModbusAddress, Registers.SerialNumber.Offset, Registers.SerialNumber.Length);

            return data;
        }

        public async Task<ushort[]> GetVersion()
        {
            Console.WriteLine("GetVersion");

            var data = await modbusClient.ReadHoldingRegisters(ModbusAddress, Registers.Version.Offset, Registers.Version.Length);
            return data;
        }

        public async Task<Measurements> GetMeasurements()
        {
            Console.WriteLine("GetData");

            var values = await modbusClient.ReadHoldingRegistersFloat(ModbusAddress, Registers.Data.Offset, Registers.Data.Length / 2);

            return new Measurements(values);
        }

        public async Task<TimeSpan> GetBrushInterval()
        {
            var value = await modbusClient.ReadHoldingRegisters(ModbusAddress, Registers.BrushInterval.Offset, Registers.BrushInterval.Length);
            return TimeSpan.FromMinutes(value[0]);
        }

        //31: sign
        //30..23: exponent (8 bit exponent)
        //22..0: fraction (23 bit fraction)
        public double ConvertUShortsToDouble(ushort high, ushort low)
        {
            // Combine the high and low values into a single uint
            uint input = (uint)(((high & 0x00FF) << 24) | 
                                ((high & 0xFF00) << 8) | 
                                 (low & 0x00FF) << 8 | 
                                  low >> 8);

          //  var input = ((uint)high << 16) | low;
            // Get the sign bit
            uint signBit = (input >> 31) & 1;
            int sign = 1 - (int)(2 * signBit);
            // Get the exponent bits
            var exponentBits = ((input >> 23) & 0xFF);
            var exponent = exponentBits - 127;
            // Get the fraction
            var fractionBits = (input & 0x7FFFFF);
            var fraction = 1.0 + fractionBits / Math.Pow(2, 23);

            // get the value
            return sign * fraction * Math.Pow(2, exponent);
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provides access to the Sensiron SGP40 VOC sensor
    /// </summary>
    public partial class Sgp40 : ByteCommsSensorBase<int>
    {
        /// <summary>
        /// </summary>
        public event EventHandler<ChangeResult<int>> VocIndexUpdated = delegate { };

        /// <summary>
        /// The VOC Index, from the last reading.
        /// </summary>
        public int VocIndex => Conditions;

        /// <summary>
        /// Serial number of the device.
        /// </summary>
        public ulong SerialNumber { get; private set; }

        private byte[]? _compensationData = null;

        /// <summary>
        /// Creates a new SGP40 VOC sensor.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x40).</param>
        /// <param name="i2cBus">I2CBus.</param>
        public Sgp40(II2cBus i2cBus, byte address = (byte)Address.Default)
            : base(i2cBus, address, 9, 8)
        {
            Initialize();
        }

        /// <summary>
        /// Initalize the sensor
        /// </summary>
        protected void Initialize()
        {
            // write buffer for initialization commands only can be two bytes.
            Span<byte> tx = WriteBuffer.Span[0..2];

            Peripheral?.Write(sgp4x_get_serial_number);

            Thread.Sleep(1); // per the data sheet

            Peripheral?.Read(ReadBuffer.Span[0..9]);

            var bytes = ReadBuffer.ToArray();

            SerialNumber = (ulong)(bytes[0] << 40 | bytes[1] << 32 | bytes[3] << 24 | bytes[4] << 16 | bytes[6] << 8 | bytes[7] << 0);
        }

        /// <summary>
        /// This command triggers the built-in self-test checking for integrity of both hotplate and MOX material
        /// </summary>
        /// <returns>true on sucessful test, otherwise false</returns>
        public bool RunSelfTest()
        {
            Peripheral?.Write(sgp40_execute_self_test);

            Thread.Sleep(325); // test requires 320ms to complete

            Peripheral?.Read(ReadBuffer.Span[0..3]);

            return ReadBuffer.Span[0..1][0] == 0xd4;
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected async override Task<int> ReadSensor()
        {
            return await Task.Run(() =>
            {
                if(_compensationData != null)
                {
                    Peripheral?.Write(_compensationData);
                }
                else
                {
                    Peripheral?.Write(sgp40_measure_raw_signal_uncompensated);
                }

                Thread.Sleep(30); // per the data sheet

                Peripheral?.Read(ReadBuffer.Span[0..3]);

                var data = ReadBuffer.Span[0..3].ToArray();

                return data[0] << 8 | data[1];
            });
        }

        /// <summary>
        /// Inheritance-safe way to raise events and notify observers.
        /// </summary>
        /// <param name="changeResult"></param>
        protected override void RaiseEventsAndNotify(IChangeResult<int> changeResult)
        {
            VocIndexUpdated?.Invoke(this, new ChangeResult<int>(VocIndex, changeResult.Old));

            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// This command turns the hotplate off and stops the measurement. Subsequently, the sensor enters idle mode.
        /// </summary>
        public void TurnHeaterOff()
        {
            Peripheral?.Write(sgp4x_turn_heater_off);
        }

        /// <summary>
        /// Set the compensation data
        /// </summary>
        /// <param name="humidity">Humidity compensation</param>
        /// <param name="temperature">Temperature compensation</param>
        public void SetCompensationData(RelativeHumidity humidity, Units.Temperature temperature)
        {
            _compensationData = new byte[8];

            Array.Copy(sgp40_measure_raw_signal, 0, _compensationData, 0, 2);

            var rh = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder((ushort)(humidity.Percent * 65535 / 100)));
            _compensationData[2] = rh[0];
            _compensationData[3] = rh[1];
            _compensationData[4] = Crc(rh);

            var t = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder((ushort)(temperature.Celsius * 65535 / 175)));
            _compensationData[5] = t[0];
            _compensationData[6] = t[1];
            _compensationData[7] = Crc(t);
        }

        public void ClearCompensationData()
        {
            _compensationData = null;
        }

        private byte Crc(byte[] data)
        {
            if (data.Length != 2) throw new ArgumentException();

            byte crc = 0xFF;
            for (int i = 0; i < 2; i++)
            {
                crc ^= data[i];
                for (byte bit = 8; bit > 0; --bit)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ 0x31);
                    }
                    else
                    {
                        crc = (byte)(crc << 1);
                    }
                }
            }
            return crc;
        }
    }
}
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents an AGS01DB MEMS VOC gas / air quality sensor
    /// Pinout (left to right, label side down): VDD, SDA, GND, SCL
    /// Note: requires pullup resistors on SDA/SCL
    /// </summary>
    public partial class Ags01Db : ByteCommsSensorBase<Concentration>, II2cPeripheral
    {
        const byte CRC_POLYNOMIAL = 0x31;
        const byte CRC_INIT = 0xFF;

        const byte ASG_DATA_MSB = 0x00;
        const byte ASG_DATA_LSB = 0x02;
        const byte ASG_VERSION_MSB = 0x0A;
        const byte ASG_VERSION_LSB = 0x01;

        /// <summary>
        /// Raised when the concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> ConcentrationUpdated = delegate { };

        /// <summary>
        /// The current concentration value
        /// </summary>
        public Concentration? Concentration { get; private set; }

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Create a new Ags01Db object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Ags01Db(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address, readBufferSize: 3, writeBufferSize: 3)
        {
        }

        /// <summary>
        /// Get ASG01DB VOC Gas Concentration and update the Concentration property
        /// </summary>
        protected override Task<Concentration> ReadSensor()
        {
            WriteBuffer.Span[0] = ASG_DATA_MSB;
            WriteBuffer.Span[1] = ASG_DATA_LSB;

            BusComms.Exchange(WriteBuffer.Span[0..1], ReadBuffer.Span);

            var value = ReadBuffer.Span[0] << 8 | ReadBuffer.Span[1];

            var voc = value / 10.0;//ppm

            Concentration = new Concentration(voc, Units.Concentration.UnitType.PartsPerMillion);

            return Task.FromResult(Concentration.Value);
        }

        /// <summary>
        /// Get ASG01DB Version
        /// </summary>
        /// <returns>Version</returns>
        public byte GetVersion()
        {
            // Details in the Datasheet P5
            // Write command MSB, LSB
            WriteBuffer.Span[0] = ASG_VERSION_MSB;
            WriteBuffer.Span[1] = ASG_VERSION_LSB;

            BusComms.Exchange(WriteBuffer.Span[0..1], ReadBuffer.Span[0..1]);

            // CRC check
            if (!CheckCrc8(ReadBuffer[..1].ToArray(), 1, ReadBuffer.Span[1]))
            {
                return unchecked((byte)-1);
            }

            return ReadBuffer.Span[0];
        }

        /// <summary>
        /// Raise change events for subscribers
        /// </summary>
        /// <param name="changeResult">The change result with the current sensor data</param>
        protected void RaiseChangedAndNotify(IChangeResult<Concentration> changeResult)
        {
            ConcentrationUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// 8-bit CRC Checksum Calculation
        /// </summary>
        /// <param name="data">Raw Data</param>
        /// <param name="length">Data Length</param>
        /// <param name="crc8">Raw CRC8</param>
        /// <returns>Checksum is true or false</returns>
        private bool CheckCrc8(byte[] data, int length, byte crc8)
        {
            // Details in the Datasheet P6
            byte crc = CRC_INIT;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];

                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ CRC_POLYNOMIAL);
                    }
                    else
                    {
                        crc = (byte)(crc << 1);
                    }
                }
            }

            return (crc == crc8);
        }
    }
}
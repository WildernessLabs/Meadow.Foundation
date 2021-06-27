using System;
using System.Buffers.Binary;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Environmental
{

    /// <summary>
    /// Represents an AGS01DB MEMS VOC gas / air quality sensor
    /// Pinout (left to right, label side down): VDD, SDA, GND, SCL
    /// Note: requires pullup resistors on SDA/SCL
    /// </summary>
    public class Ags01Db : ByteCommsSensorBase<Units.Concentration>
    {
        private const byte CRC_POLYNOMIAL = 0x31;
        private const byte CRC_INIT = 0xFF;

        private const byte ASG_DATA_MSB = 0x00;
        private const byte ASG_DATA_LSB = 0x02;
        private const byte ASG_VERSION_MSB = 0x0A;
        private const byte ASG_VERSION_LSB = 0x01;

        //==== events
        public event EventHandler<IChangeResult<Units.Concentration>> ConcentrationUpdated = delegate { };

        public Concentration? Concentration { get; private set; }


        public Ags01Db(II2cBus i2cBus, byte address = 0x11)
            : base(i2cBus, address, readBufferSize: 3, writeBufferSize: 3)
        {

        }

        /// <summary>
        /// Get ASG01DB VOC Gas Concentration and
        /// Update the Concentration property.
        /// </summary>
        protected override Task<Units.Concentration> ReadSensor()
        {
            return Task.Run(() =>
            {
                WriteBuffer.Span[0] = ASG_DATA_MSB;
                WriteBuffer.Span[1] = ASG_DATA_LSB;

                Peripheral.Exchange(WriteBuffer.Span[0..1], ReadBuffer.Span);

                // sensor.WriteBytes(data);
                // var readBuffer = sensor.ReadBytes(3);

                var value = ReadBuffer.Span[0] << 8 | ReadBuffer.Span[1];

                var voc = value / 10.0;//should be ppm

                Concentration = new Concentration(voc, Units.Concentration.UnitType.PartsPerMillion);

                return Concentration.Value;
            });
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

            Peripheral.Exchange(WriteBuffer.Span[0..1], ReadBuffer.Span[0..1]);

            // CRC check
            if (!CheckCrc8(ReadBuffer.Slice(0, 1).ToArray(), 1, ReadBuffer.Span[1]))
            {
                return unchecked((byte)-1);
            }

            return ReadBuffer.Span[0];
        }

        protected void RaiseChangedAndNotify(IChangeResult<Units.Concentration> changeResult)
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
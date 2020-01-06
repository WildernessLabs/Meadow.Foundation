using System;
using System.Buffers.Binary;
using System.Threading;
using Meadow.Hardware;

namespace Environmental.Ags01Db
{
    public class Ags01Db
    {
        #region Properties

        /// <summary>
        /// ASG01DB VOC (Volatile Organic Compounds) Gas Concentration (ppm)
        /// </summary>
        public double Concentration => GetConcentration(); 

        /// <summary>
        /// ASG01DB Version
        /// </summary>
        public byte Version => GetVersion(); 

        #endregion Properties

        #region Constants

        private const byte CRC_POLYNOMIAL = 0x31;
        private const byte CRC_INIT = 0xFF;

        private const byte ASG_DATA_MSB = 0x00;
        private const byte ASG_DATA_LSB = 0x02;
        private const byte ASG_VERSION_MSB = 0x0A;
        private const byte ASG_VERSION_LSB = 0x01;

        #endregion Constants

        #region Member variables / fields

        II2cPeripheral sensor;

        int _lastMeasurement;

        #endregion Member variables / fields

        #region Constructors

        private Ags01Db() { }

        public Ags01Db(II2cBus i2cBus, byte address = 0x11)
        {
            sensor = new I2cPeripheral(i2cBus, address);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get ASG01DB VOC Gas Concentration
        /// </summary>
        /// <returns>Concentration (ppm)</returns>
        private double GetConcentration()
        {
            // The time of two measurements should be more than 2s.
            while (Environment.TickCount - _lastMeasurement < 2000)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(Environment.TickCount - _lastMeasurement));
            }


            // Details in the Datasheet P5
            // Write command MSB, LSB
            var data = new byte[] { ASG_DATA_MSB, ASG_DATA_LSB };

            sensor.WriteBytes(data);
            var readBuffer = sensor.ReadBytes(3);

            // CRC check
            if (!CheckCrc8(readBuffer, 2, readBuffer[1]))
            {
                return -1;
            }

            ushort res = BinaryPrimitives.ReadUInt16BigEndian(readBuffer.AsSpan(0, 2));

            return res / 10.0;
        }

        /// <summary>
        /// Get ASG01DB Version
        /// </summary>
        /// <returns>Version</returns>
        private byte GetVersion()
        {
            // Details in the Datasheet P5
            // Write command MSB, LSB
            var data = new byte[] { ASG_VERSION_MSB, ASG_VERSION_LSB };

            sensor.WriteBytes(data);
            var readBuffer = sensor.ReadBytes(2);

            // CRC check
            if (!CheckCrc8(readBuffer, 1, readBuffer[1]))
            {
                return unchecked((byte)-1);
            }

            return readBuffer[0];
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

        #endregion
    }
}
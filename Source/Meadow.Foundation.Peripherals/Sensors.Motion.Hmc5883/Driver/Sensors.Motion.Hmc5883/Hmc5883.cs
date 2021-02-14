using Meadow.Foundation.Spatial;
using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    ///     Driver for the Hmc5883 digital compass.
    ///     
    /// This driver is untested
    /// </summary>
    public class Hmc5883
    {
        /// <summary>
        /// Register of HMC5883L
        /// </summary>
        enum Register : byte
        {
            HMC_CONFIG_REG_A_ADDR = 0x00,
            HMC_CONFIG_REG_B_ADDR = 0x01,
            HMC_MODE_REG_ADDR = 0x02,
            HMC_X_MSB_REG_ADDR = 0x03,
            HMC_Z_MSB_REG_ADDR = 0x05,
            HMC_Y_MSB_REG_ADDR = 0x07,
            HMC_STATUS_REG_ADDR = 0x09
        }

        /// <summary>
        /// HMC5883L Gain Setting
        /// </summary>
        public enum Gain
        {
            /// <summary>
            /// 1370, recommended sensor field range: ±0.88 Ga
            /// </summary>
            Gain1370 = 0x00,

            /// <summary>
            /// 1090, recommended sensor field range: ±1.3 Ga
            /// </summary>
            Gain1090 = 0x01,

            /// <summary>
            /// 820, recommended sensor field range: ±1.9 Ga
            /// </summary>
            Gain0820 = 0x02,

            /// <summary>
            /// 660, recommended sensor field range: ±2.5 Ga
            /// </summary>
            Gain0660 = 0x03,

            /// <summary>
            /// 440, recommended sensor field range: ±4.0 Ga
            /// </summary>
            Gain0440 = 0x04,

            /// <summary>
            /// 390, recommended sensor field range: ±4.7 Ga
            /// </summary>
            Gain0390 = 0x05,

            /// <summary>
            /// 330, recommended sensor field range: ±5.6 Ga
            /// </summary>
            Gain0330 = 0x06,

            /// <summary>
            /// 230, recommended sensor field range: ±8.1 Ga
            /// </summary>
            Gain0230 = 0x07,
        }

        /// <summary>
        /// Number of samples averaged (1 to 8) per measurement output.
        /// </summary>
        public enum SamplesAmount : byte
        {
            /// <summary>
            /// 1 (Default) samples per measurement output.
            /// </summary>
            One = 0b_0000_0000,

            /// <summary>
            /// 2 samples per measurement output.
            /// </summary>
            Two = 0b_0010_0000,

            /// <summary>
            /// 4 samples per measurement output.
            /// </summary>
            Four = 0b_0100_0000,

            /// <summary>
            /// 8 samples per measurement output.
            /// </summary>
            Eight = 0b_0110_0000
        }

        /// <summary>
        /// HMC5883L Typical Data Output Rate (Hz)
        /// </summary>
        public enum OutputRate
        {
            /// <summary>
            /// 0.75 Hz
            /// </summary>
            Rate0_75 = 0x00,

            /// <summary>
            /// 1.5 Hz
            /// </summary>
            Rate1_5 = 0x01,

            /// <summary>
            /// 3 Hz
            /// </summary>
            Rate3 = 0x02,

            /// <summary>
            /// 7.5 Hz
            /// </summary>
            Rate7_5 = 0x03,

            /// <summary>
            /// 15 Hz
            /// </summary>
            Rate15 = 0x04,

            /// <summary>
            /// 30 Hz
            /// </summary>
            Rate30 = 0x05,

            /// <summary>
            /// 75 Hz
            /// </summary>
            Rate75 = 0x06,
        }

        /// <summary>
        /// The status of HMC5883L device
        /// </summary>
        public enum Status : byte
        {
            Ready = 0b_0000_0001,
            Lock = 0b_0000_0010,
            RegulatorEnabled = 0b_0000_0100
        }

        /// <summary>
        /// Measurement configuration.
        /// </summary>
        public enum MeasurementConfiguration : byte
        {
            Normal = 0b_0000_0000,
            PositiveBiasConfiguration = 0b_0000_0001,
            NegativeBias = 0b_0000_0010
        }

        /// <summary>
        /// HMC5883L measuring mode 
        /// </summary>
        public enum MeasuringMode
        {
            Continuous = 0x00,
            Single = 0x01
        }

        protected static II2cPeripheral i2cPeripheral;

        protected byte measuringMode;
        protected byte outputRate;
        protected byte gain;
        protected byte sampleAmount;
        protected byte measurementConfig;

        /// <summary>
        /// HMC5883L Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x1E;

        /// <summary>
        /// HMC5883L Direction as a Vector
        /// </summary>
        public Vector Direction => GetDirection();

        /// <summary>
        /// HMC5883L Heading (DEG)
        /// </summary>
        public double Heading => DirectionToHeading(GetDirection());

        /// <summary>
        /// HMC5883L Status
        /// </summary>
        public Status DeviceStatus => GetStatus();

        

        

        /// <summary>
        ///     Event to be raised when the compass changes
        /// </summary>
        public event SensorVectorEventHandler DirectionChanged = delegate { };
        
        

        

        protected Hmc5883() { }

        public Hmc5883(II2cBus i2cBus, byte address = 0x1E,
            Gain gain = Gain.Gain1090,
            MeasuringMode measuringMode = MeasuringMode.Continuous,
            OutputRate outputRate = OutputRate.Rate15,
            SamplesAmount samplesAmount = SamplesAmount.One,
            MeasurementConfiguration measurementConfig = MeasurementConfiguration.Normal)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            this.gain = (byte)gain;
            this.measuringMode = (byte)measuringMode;
            this.outputRate = (byte)outputRate;
            sampleAmount = (byte)samplesAmount;
            this.measurementConfig = (byte)measurementConfig;

            Initialize();
        }

        

        

        protected virtual void Initialize()
        {
            byte configA = (byte)(sampleAmount | (outputRate << 2) | measurementConfig);
            byte configB = (byte)(gain << 5);

            i2cPeripheral.WriteBytes(new byte[] { (byte)Register.HMC_CONFIG_REG_A_ADDR, configA});
            i2cPeripheral.WriteBytes(new byte[] { (byte)Register.HMC_CONFIG_REG_B_ADDR, configB});
            i2cPeripheral.WriteBytes(new byte[] { (byte)Register.HMC_MODE_REG_ADDR, measuringMode});
        }

        /// <summary>
        /// Read raw data from HMC5883L
        /// </summary>
        /// <returns>Raw Data</returns>
        private Vector GetDirection()
        {
            ushort x = i2cPeripheral.ReadUShort((byte)Register.HMC_X_MSB_REG_ADDR, ByteOrder.BigEndian);
            ushort y = i2cPeripheral.ReadUShort((byte)Register.HMC_Y_MSB_REG_ADDR, ByteOrder.BigEndian);
            ushort z = i2cPeripheral.ReadUShort((byte)Register.HMC_Z_MSB_REG_ADDR, ByteOrder.BigEndian);

            return new Vector(x, y, z);
        }

        /// <summary>
        /// Calculate heading
        /// </summary>
        /// <param name="direction">HMC5883L Direction</param>
        /// <returns>Heading (DEG)</returns>
        protected double DirectionToHeading(Vector direction)
        {
            double deg = Math.Atan2(direction.Y, direction.X) * 180 / Math.PI;

            if (deg < 0) {
                deg += 360;
            }

            return deg;
        }

        /// <summary>
        /// Reads device status
        /// </summary>
        private Status GetStatus()
        {
            i2cPeripheral.WriteByte((byte)Register.HMC_STATUS_REG_ADDR);
            byte status = i2cPeripheral.ReadBytes(1)[0];

            return (Status)status;
        }

        
    }
}
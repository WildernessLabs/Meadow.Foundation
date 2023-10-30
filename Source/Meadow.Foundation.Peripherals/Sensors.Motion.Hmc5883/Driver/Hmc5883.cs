using Meadow.Foundation.Spatial;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Driver for the Hmc5883 digital compass
    /// 
    /// This driver is untested
    /// </summary>
    public partial class Hmc5883 : ByteCommsSensorBase<Vector>, II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Event to be raised when the compass changes
        /// </summary>
        public event EventHandler<IChangeResult<Vector>> DirectionUpdated = delegate { };

        internal byte measuringMode;
        internal byte outputRate;
        internal byte gain;
        internal byte sampleAmount;
        internal byte measurementConfig;

        /// <summary>
        /// HMC5883L Direction as a Vector
        /// </summary>
        public Vector? Direction => Conditions;

        /// <summary>
        /// HMC5883L Heading (DEG)
        /// </summary>
        public Azimuth? Heading => DirectionToHeading(Conditions);

        /// <summary>
        /// HMC5883L Status
        /// </summary>
        public Statuses DeviceStatus => GetStatus();

        /// <summary>
        /// Create a new Hmc5883 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="gain">Gain</param>
        /// <param name="measuringMode">Measuring mode</param>
        /// <param name="outputRate">Output rate</param>
        /// <param name="sampleAmount">Sample amount</param>
        /// <param name="measurementConfig">Measurement configuration</param>
        public Hmc5883(II2cBus i2cBus, byte address = (byte)Addresses.Default,
            GainLevels gain = GainLevels.Gain1090,
            MeasuringModes measuringMode = MeasuringModes.Continuous,
            DataOutputRates outputRate = DataOutputRates.Rate15,
            SampleAmounts sampleAmount = SampleAmounts.One,
            MeasurementConfigurations measurementConfig = MeasurementConfigurations.Normal)
                : base(i2cBus, address)
        {
            this.gain = (byte)gain;
            this.measuringMode = (byte)measuringMode;
            this.outputRate = (byte)outputRate;
            this.sampleAmount = (byte)sampleAmount;
            this.measurementConfig = (byte)measurementConfig;

            Initialize();
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        protected virtual void Initialize()
        {
            byte configA = (byte)(sampleAmount | (outputRate << 2) | measurementConfig);
            byte configB = (byte)(gain << 5);

            BusComms?.WriteRegister(Registers.HMC_CONFIG_REG_A_ADDR, configA);
            BusComms?.WriteRegister(Registers.HMC_CONFIG_REG_B_ADDR, configB);
            BusComms?.WriteRegister(Registers.HMC_MODE_REG_ADDR, measuringMode);
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<Vector> changeResult)
        {
            DirectionUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<Vector> ReadSensor()
        {
            ushort x = BusComms!.ReadRegisterAsUShort(Registers.HMC_X_MSB_REG_ADDR, ByteOrder.BigEndian);
            ushort y = BusComms!.ReadRegisterAsUShort(Registers.HMC_Y_MSB_REG_ADDR, ByteOrder.BigEndian);
            ushort z = BusComms!.ReadRegisterAsUShort(Registers.HMC_Z_MSB_REG_ADDR, ByteOrder.BigEndian);
            return Task.FromResult(new Vector(x, y, z));
        }

        /// <summary>
        /// Calculate heading
        /// </summary>
        /// <param name="direction">HMC5883L Direction</param>
        /// <returns>Heading (DEG)</returns>
        public static Azimuth DirectionToHeading(Vector direction)
        {
            double deg = Math.Atan2(direction.Y, direction.X) * 180 / Math.PI;

            if (deg < 0)
            {
                deg += 360;
            }

            return new Azimuth(deg);
        }

        /// <summary>
        /// Reads device status
        /// </summary>
        private Statuses GetStatus()
        {
            BusComms!.Write(Registers.HMC_STATUS_REG_ADDR);
            BusComms!.Read(ReadBuffer.Span[0..1]);
            return (Statuses)ReadBuffer.Span[0];
        }
    }
}
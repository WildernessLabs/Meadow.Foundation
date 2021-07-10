using Meadow.Foundation.Spatial;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    ///     Driver for the Hmc5883 digital compass.
    ///     
    /// This driver is untested
    /// </summary>
    public partial class Hmc5883 : ByteCommsSensorBase<Vector>
    {
        //==== events
        /// <summary>
        ///     Event to be raised when the compass changes
        /// </summary>
        public event EventHandler<IChangeResult<Vector>> DirectionUpdated = delegate { };

        //==== internas
        protected byte measuringMode;
        protected byte outputRate;
        protected byte gain;
        protected byte sampleAmount;
        protected byte measurementConfig;

        //==== Properties
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

        public Hmc5883(II2cBus i2cBus, byte address = Addresses.HMC5883_ADDRESS,
            GainLevels gain = GainLevels.Gain1090,
            MeasuringModes measuringMode = MeasuringModes.Continuous,
            DataOutputRates outputRate = DataOutputRates.Rate15,
            SampleAmounts samplesAmount = SampleAmounts.One,
            MeasurementConfigurations measurementConfig = MeasurementConfigurations.Normal)
                : base(i2cBus, address)
        {

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

            Peripheral.WriteRegister(Registers.HMC_CONFIG_REG_A_ADDR, configA);
            Peripheral.WriteRegister(Registers.HMC_CONFIG_REG_B_ADDR, configB);
            Peripheral.WriteRegister(Registers.HMC_MODE_REG_ADDR, measuringMode);
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Vector> changeResult)
        {
            this.DirectionUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        protected override Task<Vector> ReadSensor()
        {
            return Task.Run(() => {
                ushort x = Peripheral.ReadRegisterAsUShort(Registers.HMC_X_MSB_REG_ADDR, ByteOrder.BigEndian);
                ushort y = Peripheral.ReadRegisterAsUShort(Registers.HMC_Y_MSB_REG_ADDR, ByteOrder.BigEndian);
                ushort z = Peripheral.ReadRegisterAsUShort(Registers.HMC_Z_MSB_REG_ADDR, ByteOrder.BigEndian);
                return new Vector(x, y, z);
            });
        }

        /// <summary>
        /// Calculate heading
        /// </summary>
        /// <param name="direction">HMC5883L Direction</param>
        /// <returns>Heading (DEG)</returns>
        public static Azimuth DirectionToHeading(Vector direction)
        {
            double deg = Math.Atan2(direction.Y, direction.X) * 180 / Math.PI;

            if (deg < 0) {
                deg += 360;
            }

            return new Azimuth(deg);
        }

        /// <summary>
        /// Reads device status
        /// </summary>
        private Statuses GetStatus()
        {
            Peripheral.Write(Registers.HMC_STATUS_REG_ADDR);
            Peripheral.Read(ReadBuffer.Span[0..1]);
            return (Statuses)ReadBuffer.Span[0];
        }
    }
}
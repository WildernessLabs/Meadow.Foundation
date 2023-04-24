using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Represents Mma7660fc 3-axis acclerometer
    /// </summary>
    public partial class Mma7660fc : ByteCommsSensorBase<Acceleration3D>, IAccelerometer
    {
        /// <summary>
        /// Raised when new acceleration data is processed
        /// </summary>
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated = delegate { };

        /// <summary>
        /// Current Acceleration3d value
        /// </summary>
        public Acceleration3D? Acceleration3D => Conditions;

        /// <summary>
        /// Get the current direction value
        /// </summary>
        public DirectionType Direction { get; set; } = DirectionType.Unknown;

        /// <summary>
        /// Get the current orientation
        /// </summary>
        public OrientationType Orientation { get; set; } = OrientationType.Unknown;

        /// <summary>
        /// Create a new instance of the Mma7660fc communicating over the I2C interface.
        /// </summary>
        /// <param name="address">Address of the I2C sensor</param>
        /// <param name="i2cBus">I2C bus</param>
        public Mma7660fc(II2cBus i2cBus, Addresses address = Addresses.Default)
            : this(i2cBus, (byte)address)
        {
        }

        /// <summary>
        /// Create a new instance of the Mma7660fc communicating over the I2C interface.
        /// </summary>
        /// <param name="address">Address of the I2C sensor</param>
        /// <param name="i2cBus">I2C bus</param>
        public Mma7660fc(II2cBus i2cBus, byte address)
            : base(i2cBus, address)
        {
            Initialize();
        }

        void Initialize()
        {
            SetMode(SensorPowerMode.Standby);
            SetSampleRate(SampleRate._32);
            SetMode(SensorPowerMode.Active);
        }

        void SetMode(SensorPowerMode mode)
        {
            Peripheral.WriteRegister((byte)Registers.Mode, (byte)mode);
        }

        /// <summary>
        /// Set sample rate in samples per second
        /// </summary>
        /// <param name="rate">sample rate</param>
        public void SetSampleRate(SampleRate rate)
        {
            Peripheral.WriteRegister((byte)Registers.SleepRate, (byte)rate);
        }

        /// <summary>
        /// Read sensor data from registers
        /// </summary>
        /// <returns></returns>
        protected override Task<Acceleration3D> ReadSensor()
        {
            return Task.Run(() =>
            {
                Direction = (DirectionType)(Peripheral.ReadRegister((byte)Registers.TILT) & 0x1C);

                Orientation = (OrientationType)(Peripheral.ReadRegister((byte)Registers.TILT) & 0x03);

                int xAccel, yAccel, zAccel;
                byte x, y, z;

                //Signed byte 6-bit 2’s complement data with allowable range of +31 to -32
                //[5] is 0 if the g direction is positive, 1 if the g direction is negative.
                //ensure bit 6 isn't set - if so, it means there was a read/write collision ... try again
                do
                {
                    x = Peripheral.ReadRegister((byte)Registers.XOUT);
                } while (x >= 64);

                //check bit 5 and flip to negative
                if ((x & (1 << 5)) != 0) xAccel = x - 64;
                else xAccel = x;

                do
                {
                    y = Peripheral.ReadRegister((byte)Registers.YOUT);
                } while (y >= 64); //ensure bit 6 isn't set - if so, it means there was a read/write collision ... try again

                if ((y & (1 << 5)) != 0) yAccel = y - 64;
                else yAccel = y;

                //ensure bit 6 isn't set - if so, it means there was a read/write collision ... try again
                do
                {
                    z = Peripheral.ReadRegister((byte)Registers.ZOUT);
                } while (y >= 64);

                if ((z & (1 << 5)) != 0) zAccel = z - 64;
                else zAccel = z;

                return new Acceleration3D(
                    new Acceleration(xAccel * 3.0 / 64.0, Acceleration.UnitType.Gravity),
                    new Acceleration(yAccel * 3.0 / 64.0, Acceleration.UnitType.Gravity),
                    new Acceleration(zAccel * 3.0 / 64.0, Acceleration.UnitType.Gravity));
            });
        }

        /// <summary>
        /// Raise event and notify subscribers
        /// </summary>
        /// <param name="changeResult">Acceleration3d data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<Acceleration3D> changeResult)
        {
            Acceleration3DUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}
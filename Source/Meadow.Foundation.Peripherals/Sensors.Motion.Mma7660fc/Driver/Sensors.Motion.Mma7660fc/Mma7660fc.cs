using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Represents Mma7660fc 3-axis acclerometer
    /// </summary>
    public partial class Mma7660fc : ByteCommsSensorBase<Acceleration3D>, IAccelerometer
    {
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated = delegate { };

        public Acceleration3D? Acceleration3D => Conditions;

        /// <summary>
        ///     Create a new instance of the Mma7660fc communicating over the I2C interface.
        /// </summary>
        /// <param name="address">Address of the I2C sensor</param>
        /// <param name="i2cBus">I2C bus</param>
        public Mma7660fc(II2cBus i2cBus, Addresses address = Addresses.Default)
            : this(i2cBus, (byte)address)
        {
            
        }

        /// <summary>
        ///     Create a new instance of the Mma7660fc communicating over the I2C interface.
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
            SetMode(SensorMode.Standby);
            SetSampleRate(SampleRate._32);
            SetMode(SensorMode.Active);
        }
      
        void SetMode(SensorMode mode)
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

        protected override Task<Acceleration3D> ReadSensor()
        {
            return Task.Run(() =>
            {
                int xAccel, yAccel, zAccel;

                //Signed byte 6-bit 2’s complement data with allowable range of +31 to -32
                //[5] is 0 if the g direction is positive, 1 if the g direction is negative.
                var x = Peripheral.ReadRegister((byte)Registers.XOUT);

                if ((x & (1 << 5)) != 0) xAccel = x - 64;
                else xAccel = x;

                var y = Peripheral.ReadRegister((byte)Registers.YOUT);

                if ((y & (1 << 5)) != 0) yAccel = y - 64;
                else yAccel = y;

                var z = Peripheral.ReadRegister((byte)Registers.ZOUT);
                if ((z & (1 << 5)) != 0) zAccel = z - 64;
                else zAccel = z;

                return new Acceleration3D(
                    new Acceleration(0, Acceleration.UnitType.Gravity),
                    new Acceleration(0, Acceleration.UnitType.Gravity),
                    new Acceleration(0, Acceleration.UnitType.Gravity)
                    );

            });
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Acceleration3D> changeResult)
        {
            Acceleration3DUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }


    }
}
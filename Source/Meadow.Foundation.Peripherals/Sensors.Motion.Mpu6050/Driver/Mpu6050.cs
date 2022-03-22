using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using TU = Meadow.Units.Temperature.UnitType;

namespace Meadow.Foundation.Sensors.Motion
{
    // Sample reading:
    // Accel: [X:0.24,Y:-0.74,Z:10.49 (m/s^2)]
    // Angular Velocity: [X:-0.90, Y:-1.24, Z:-0.52 (dps)]
    // Temp: 33.33C

    // TODO: this sensor has software controlled sensitivity ranges. we should
    // expose them. note the `AccelScaleBase` will need to change. Right now it's
    // hard coded to +-2G

    public partial class Mpu6050 :
        ByteCommsSensorBase<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Units.Temperature? Temperature)>,
        IAccelerometer, IGyroscope, ITemperatureSensor
    {
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated = delegate { };
        public event EventHandler<IChangeResult<AngularVelocity3D>> AngularVelocity3DUpdated = delegate { };
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        private const float GyroScaleBase = 131f;
        private const float AccelScaleBase = 16384f; // pg.13, scale for sensitivity scale AFS_SEL=0 (+- 2G)
        private int GyroScale { get; set; }
        private int AccelerometerScale { get; set; }

        /// <summary>
        /// Acceleration 3D
        /// </summary>
        public Acceleration3D? Acceleration3D => Conditions.Acceleration3D;
        /// <summary>
        /// Angualr acceleration 3D
        /// </summary>
        public AngularVelocity3D? AngularVelocity3D => Conditions.AngularVelocity3D;
        /// <summary>
        /// Temperature
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        public Mpu6050(II2cBus i2cBus, Addresses address = Addresses.Default)
            : this(i2cBus, (byte)address)
        {
        }

        public Mpu6050(II2cBus i2cBus, byte address)
            : base(i2cBus, address, readBufferSize: 14)
        {
            Initialize(address);
        }

        protected void Initialize(byte address)
        {
            switch (address)
            {
                case 0x68:
                case 0x69:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("MPU6050 device address must be either 0x68 or 0x69");
            }
            Wake();
        }

        public void Wake()
        {
            WriteBuffer.Span[0] = Registers.POWER_MANAGEMENT;
            WriteBuffer.Span[1] = 0x00;
            Peripheral.Write(WriteBuffer.Span[0..2]);

            LoadConfiguration();
        }

        protected void LoadConfiguration()
        {
            // read all 3 config bytes
            Peripheral.ReadRegister(Registers.CONFIG, ReadBuffer.Span[0..3]);

            GyroScale = (ReadBuffer.Span[1] & 0b00011000) >> 3;
            AccelerometerScale = (ReadBuffer.Span[2] & 0b00011000) >> 3;
        }

        protected override void RaiseEventsAndNotify(IChangeResult<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Units.Temperature? Temperature)> changeResult)
        {
            if (changeResult.New.AngularVelocity3D is { } angular)
            {
                AngularVelocity3DUpdated?.Invoke(this, new ChangeResult<AngularVelocity3D>(angular, changeResult.Old?.AngularVelocity3D));
            }
            if (changeResult.New.Acceleration3D is { } accel)
            {
                Acceleration3DUpdated?.Invoke(this, new ChangeResult<Acceleration3D>(accel, changeResult.Old?.Acceleration3D));
            }
            if (changeResult.New.Temperature is { } temp)
            {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        protected override Task<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Units.Temperature? Temperature)> ReadSensor()
        {
            return Task.Run(() =>
            {
                (Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Units.Temperature? Temperature) conditions;

                // we'll just read 14 bytes (7 registers), starting at 0x3b
                Peripheral.ReadRegister(Registers.ACCELEROMETER_X, ReadBuffer.Span);

                //---- acceleration
                // get the acceleration 3d
                var a_scale = (1 << AccelerometerScale) / AccelScaleBase;
                var g_scale = (1 << GyroScale) / GyroScaleBase;
                // note that this comes back as mg (1/10 of m/s^2) which is 0.01m/s^2, so we have to multiply by 10
                Acceleration3D newAccel = new Acceleration3D(
                    new Acceleration(ScaleAndOffset(ReadBuffer.Span, 0, a_scale) * 10, Acceleration.UnitType.MetersPerSecondSquared),
                    new Acceleration(ScaleAndOffset(ReadBuffer.Span, 2, a_scale) * 10, Acceleration.UnitType.MetersPerSecondSquared),
                    new Acceleration(ScaleAndOffset(ReadBuffer.Span, 4, a_scale) * 10, Acceleration.UnitType.MetersPerSecondSquared)
                    );
                conditions.Acceleration3D = newAccel;

                //---- temp
                conditions.Temperature = new Units.Temperature(ScaleAndOffset(ReadBuffer.Span, 6, 1 / 340f, 36.53f), TU.Celsius);

                //---- angular acceleration
                AngularVelocity3D angularVelocity = new AngularVelocity3D(
                    new AngularVelocity(ScaleAndOffset(ReadBuffer.Span, 8, g_scale), AngularVelocity.UnitType.DegreesPerSecond),
                    new AngularVelocity(ScaleAndOffset(ReadBuffer.Span, 10, g_scale), AngularVelocity.UnitType.DegreesPerSecond),
                    new AngularVelocity(ScaleAndOffset(ReadBuffer.Span, 12, g_scale), AngularVelocity.UnitType.DegreesPerSecond)
                    );
                conditions.AngularVelocity3D = angularVelocity;

                //ushort rawTemp = Peripheral.ReadRegisterAsUShort(Registers.TEMPERATURE, ByteOrder.BigEndian);
                //return new Units.Temperature(rawTemp * (1 << GyroScale) / GyroScaleBase, TU.Celsius);

                return conditions;
            });
        }

        private float ScaleAndOffset(Span<byte> data, int index, float scale, float offset = 0)
        {
            // convert to a signed number
            unchecked
            {
                var s = (short)(data[index] << 8 | data[index + 1]);
                return (s * scale) + offset;
            }
        }
    }
}

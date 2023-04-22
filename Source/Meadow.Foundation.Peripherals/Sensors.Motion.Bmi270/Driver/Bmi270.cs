using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Accelerometers
{
    /// <summary>
    /// Represents a BMI270 interial measurement unit (IMU) 
    /// </summary>
    public partial class Bmi270 :
        PollingSensorBase<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Units.Temperature? Temperature)>
    {
        /// <summary>
        /// Event raised when linear acceleration changes
        /// </summary>
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated = delegate { };

        /// <summary>
        /// Event raised when angular velocity (gyro) changes
        /// </summary>
        public event EventHandler<IChangeResult<AngularVelocity3D>> AngularVelocity3DUpdated = delegate { };

        /// <summary>
        /// Event raised when temperature changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        /// <summary>
        /// Current Acceleration 3D
        /// </summary>
        public Acceleration3D? Acceleration3D => Conditions.Acceleration3D;

        /// <summary>
        /// Current Angular Velocity (Gyro) 3D
        /// </summary>
        public AngularVelocity3D? AngularVelocity3D => Conditions.AngularVelocity3D;

        /// <summary>
        /// Current Temperature
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The range of values that can be read for acceleration on each axis
        /// </summary>
        public AccelerationRange CurrentAccelerationRange { get; private set; }

        /// <summary>
        /// The range of values that can be read for angular acceleration (gyro) on each axis
        /// </summary>
        public AngularVelocityRange CurrentAngularVelocityRange { get; private set; }

        readonly II2cPeripheral i2cPeripheral;

        /// <summary>
        /// Create a new Bmi270 instance
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the sensor</param>
        /// <param name="address">The I2C address</param>
        public Bmi270(II2cBus i2cBus, byte address = (byte)Addresses.Address_0x68)
        {
            //Read buffer: 16 (needs at least 13)
            //Write buffer: 256 bytes for the config data + 1 for the address
            i2cPeripheral = new I2cPeripheral(i2cBus, address, 16, 256 + 1);

            var id = i2cPeripheral.ReadRegister(CHIP_ID);

            if (id != 36)
            {
                throw new Exception("Could not detect BMI270");
            }

            Initialize();
            SetPowerMode(PowerMode.Normal);
        }

        void Initialize()
        {   //disable advanced power save mode
            i2cPeripheral.WriteRegister(PWR_CONF, 0xB0);

            SetAccelerationRange(AccelerationRange._16g);

            //wait 450us
            Thread.Sleep(1);

            //Write INIT_CTRL 0x00 to prepare config load
            i2cPeripheral.WriteRegister(INIT_CTRL, 0);

            //upload a configuration file to register INIT_DATA
            ushort index = 0;
            ushort length = 128;
            byte[] dmaLocation = new byte[2];

            while (index < bmi270_config_file.Length) //8096
            {   /* Store 0 to 3 bits of address in first byte */
                dmaLocation[0] = (byte)((index / 2) & 0x0F);

                /* Store 4 to 11 bits of address in the second byte */
                dmaLocation[1] = (byte)((index / 2) >> 4);

                Thread.Sleep(1); //probably not needed ... data sheet wants a 2us delay

                i2cPeripheral.WriteRegister(INIT_0, dmaLocation);

                i2cPeripheral.WriteRegister(INIT_DATA, bmi270_config_file.Skip(index).Take(length).ToArray());

                index += length;
            }

            //Write INIT_CTRL 0x01 to complete config load
            i2cPeripheral.WriteRegister(INIT_CTRL, 1);

            //wait until register INTERNAL_STATUS contains 0b0001 (~20 ms)
            while (true)
            {
                Thread.Sleep(10);
                byte status = i2cPeripheral.ReadRegister(INTERNAL_STATUS);

                if (status == 0x01) { break; }
            }
            //Afer initialization - power mode is set to "configuration mode"
            //Need to change power modes before you can sample data
        }

        /// <summary>
        /// Set the range of values the sensor can read for acceleration
        /// </summary>
        /// <param name="accelRange">AccelerationRange</param>
        public void SetAccelerationRange(AccelerationRange accelRange)
        {
            i2cPeripheral.WriteRegister(ACC_RANGE, (byte)accelRange);
            CurrentAccelerationRange = accelRange;
        }

        /// <summary>
        /// Set the range of values the sensor can read for angular velocity (gyro)
        /// </summary>
        /// <param name="angRange">AngularAccelerationRange</param>
        public void SetAngularVelocityRange(AngularVelocityRange angRange)
        {   //This register also sets the OIS range but it's not implemented so we can ignore it 
            i2cPeripheral.WriteRegister(GYR_RANGE, (byte)angRange);
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
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

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Units.Temperature? Temperature)> ReadSensor()
        {
            return Task.Run(() =>
            {
                (Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Units.Temperature? Temperature) conditions;

                //12 bytes - includes accel and gyro
                var data = ReadAccelerationData();

                //likely +/- 2g by default
                var accelX = (short)(data[1] << 8 | data[0]);
                var accelY = (short)(data[3] << 8 | data[2]);
                var accelZ = (short)(data[5] << 8 | data[4]);

                double divisor = CurrentAccelerationRange switch
                {
                    AccelerationRange._2g => 16384,
                    AccelerationRange._4g => 8192,
                    AccelerationRange._8g => 4096,
                    AccelerationRange._16g => 2048,
                    _ => throw new ArgumentException("CurrentAccelerationRange is out of range")
                };

                var gX = accelX / divisor;
                var gY = accelY / divisor;
                var gZ = accelZ / divisor;

                divisor = CurrentAngularVelocityRange switch
                {
                    AngularVelocityRange._2000dps => 16.4,
                    AngularVelocityRange._1000dps => 32.8,
                    AngularVelocityRange._500dps => 65.5,
                    AngularVelocityRange._250dps => 131,
                    AngularVelocityRange._125dps => 262,

                    _ => throw new ArgumentException("CurrentAngularAccelerationRange is out of range")
                };

                var gyroX = (short)(data[7] << 8 | data[6]);
                var gyroY = (short)(data[9] << 8 | data[8]);
                var gyroZ = (short)(data[11] << 8 | data[10]);

                var dpsX = gyroX / divisor;
                var dpsY = gyroY / divisor;
                var dpsZ = gyroZ / divisor;

                conditions.Acceleration3D = new Acceleration3D(
                    new Acceleration(gX, Acceleration.UnitType.Gravity),
                    new Acceleration(gY, Acceleration.UnitType.Gravity),
                    new Acceleration(gZ, Acceleration.UnitType.Gravity));

                conditions.AngularVelocity3D = new AngularVelocity3D(
                    new AngularVelocity(dpsX, AngularVelocity.UnitType.DegreesPerSecond),
                    new AngularVelocity(dpsY, AngularVelocity.UnitType.DegreesPerSecond),
                    new AngularVelocity(dpsZ, AngularVelocity.UnitType.DegreesPerSecond));

                //Get the temperature
                ushort tempRaw = (ushort)(i2cPeripheral.ReadRegister(TEMPERATURE_1) << 8 | i2cPeripheral.ReadRegister(TEMPERATURE_0));
                double tempC;

                double degreePerByte = 0.001953125; //in celcius

                if (tempRaw < 0x8000)
                {
                    tempC = 23 + tempRaw * degreePerByte;
                }
                else
                {
                    tempC = -41 + (tempRaw - 0x8000) * degreePerByte;
                }

                if (tempRaw == 0x8000)
                {   //means we have an invalid temperature reading
                    conditions.Temperature = null;
                }
                else
                {
                    conditions.Temperature = new Units.Temperature(tempC, Units.Temperature.UnitType.Celsius);
                }
                return conditions;
            });
        }

        /// <summary>
        /// Set the device power mode
        /// </summary>
        /// <param name="powerMode">The power mode</param>
        public void SetPowerMode(PowerMode powerMode)
        {
            switch (powerMode)
            {
                case PowerMode.Suspend:
                    i2cPeripheral.WriteRegister(PWR_CTRL, 0x00);
                    i2cPeripheral.WriteRegister(PWR_CONF, 0x00);
                    break;
                case PowerMode.Configuration:
                    i2cPeripheral.WriteRegister(PWR_CTRL, 0x00);
                    i2cPeripheral.WriteRegister(PWR_CONF, 0x00);
                    break;
                case PowerMode.LowPower:
                    i2cPeripheral.WriteRegister(PWR_CTRL, 0x04);
                    i2cPeripheral.WriteRegister(ACC_CONF, 0x17);
                    i2cPeripheral.WriteRegister(GYR_CONF, 0xA9);
                    i2cPeripheral.WriteRegister(PWR_CONF, 0x03);
                    break;
                case PowerMode.Normal:
                    i2cPeripheral.WriteRegister(PWR_CTRL, 0x0E);
                    i2cPeripheral.WriteRegister(ACC_CONF, 0xA8);
                    i2cPeripheral.WriteRegister(GYR_CONF, 0xA9);
                    i2cPeripheral.WriteRegister(PWR_CONF, 0x02);
                    break;
                case PowerMode.Performance:
                    i2cPeripheral.WriteRegister(PWR_CTRL, 0x0E);
                    i2cPeripheral.WriteRegister(ACC_CONF, 0xA8);
                    i2cPeripheral.WriteRegister(GYR_CONF, 0xE9);
                    i2cPeripheral.WriteRegister(PWR_CONF, 0x02);
                    break;
            }
        }

        byte[] ReadAccelerationData()
        {
            var readBuffer = new byte[12];
            i2cPeripheral.ReadRegister(0x0C, readBuffer);
            return readBuffer;
        }
    }
}
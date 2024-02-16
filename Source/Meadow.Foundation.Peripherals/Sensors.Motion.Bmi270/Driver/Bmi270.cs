using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Accelerometers
{
    /// <summary>
    /// Represents a BMI270 inertial measurement unit (IMU) 
    /// </summary>
    public partial class Bmi270 :
        PollingSensorBase<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Units.Temperature? Temperature)>,
        II2cPeripheral, IGyroscope, IAccelerometer, ITemperatureSensor, ISleepAwarePeripheral
    {
        private event EventHandler<IChangeResult<AngularVelocity3D>> _angularVelocityHandlers = default!;
        private event EventHandler<IChangeResult<Acceleration3D>> _accelerationHandlers = default!;
        private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers = default!;

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

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        private byte[] readBuffer;

        /// <summary>
        /// Create a new Bmi270 instance
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the sensor</param>
        /// <param name="address">The I2C address</param>
        public Bmi270(II2cBus i2cBus, byte address = (byte)Addresses.Address_0x68)
        {
            //Write buffer: 256 bytes for the config data + 1 for the address
            i2cComms = new I2cCommunications(i2cBus, address, 256 + 1);

            readBuffer = new byte[12];

            var id = i2cComms.ReadRegister(CHIP_ID);

            if (id != 36)
            {
                throw new Exception("Could not detect BMI270");
            }

            Initialize();
            SetPowerMode(PowerMode.Normal);
        }

        event EventHandler<IChangeResult<AngularVelocity3D>> ISamplingSensor<AngularVelocity3D>.Updated
        {
            add => _angularVelocityHandlers += value;
            remove => _angularVelocityHandlers -= value;
        }

        event EventHandler<IChangeResult<Acceleration3D>> ISamplingSensor<Acceleration3D>.Updated
        {
            add => _accelerationHandlers += value;
            remove => _accelerationHandlers -= value;
        }

        event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
        {
            add => _temperatureHandlers += value;
            remove => _temperatureHandlers -= value;
        }

        private void Initialize()
        {   //disable advanced power save mode
            i2cComms.WriteRegister(PWR_CONF, 0xB0);

            SetAccelerationRange(AccelerationRange._16g);

            //wait 450us
            Thread.Sleep(1);

            //Write INIT_CTRL 0x00 to prepare config load
            i2cComms.WriteRegister(INIT_CTRL, 0);

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

                i2cComms.WriteRegister(INIT_0, dmaLocation);

                i2cComms.WriteRegister(INIT_DATA, bmi270_config_file.Skip(index).Take(length).ToArray());

                index += length;
            }

            //Write INIT_CTRL 0x01 to complete config load
            i2cComms.WriteRegister(INIT_CTRL, 1);

            //wait until register INTERNAL_STATUS contains 0b0001 (~20 ms)
            while (true)
            {
                Thread.Sleep(10);
                byte status = i2cComms.ReadRegister(INTERNAL_STATUS);

                if (status == 0x01) { break; }
            }
            //After initialization - power mode is set to "configuration mode"
            //Need to change power modes before you can sample data
        }

        /// <summary>
        /// Set the range of values the sensor can read for acceleration
        /// </summary>
        /// <param name="accelRange">AccelerationRange</param>
        public void SetAccelerationRange(AccelerationRange accelRange)
        {
            i2cComms.WriteRegister(ACC_RANGE, (byte)accelRange);
            CurrentAccelerationRange = accelRange;
        }

        /// <summary>
        /// Set the range of values the sensor can read for angular velocity (gyro)
        /// </summary>
        /// <param name="angRange">AngularAccelerationRange</param>
        public void SetAngularVelocityRange(AngularVelocityRange angRange)
        {   //This register also sets the OIS range but it's not implemented so we can ignore it 
            i2cComms.WriteRegister(GYR_RANGE, (byte)angRange);
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Units.Temperature? Temperature)> changeResult)
        {
            if (changeResult.New.AngularVelocity3D is { } angular)
            {
                _angularVelocityHandlers?.Invoke(this, new ChangeResult<AngularVelocity3D>(angular, changeResult.Old?.AngularVelocity3D));
            }
            if (changeResult.New.Acceleration3D is { } accel)
            {
                _accelerationHandlers?.Invoke(this, new ChangeResult<Acceleration3D>(accel, changeResult.Old?.Acceleration3D));
            }
            if (changeResult.New.Temperature is { } temp)
            {
                _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D, Units.Temperature? Temperature)> ReadSensor()
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
            ushort tempRaw = (ushort)(i2cComms.ReadRegister(TEMPERATURE_1) << 8 | i2cComms.ReadRegister(TEMPERATURE_0));
            double tempC;

            double degreePerByte = 0.001953125; //in celsius

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
            return Task.FromResult(conditions);
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
                    i2cComms.WriteRegister(PWR_CTRL, 0x00);
                    i2cComms.WriteRegister(PWR_CONF, 0x00);
                    break;
                case PowerMode.Configuration:
                    i2cComms.WriteRegister(PWR_CTRL, 0x00);
                    i2cComms.WriteRegister(PWR_CONF, 0x00);
                    break;
                case PowerMode.LowPower:
                    i2cComms.WriteRegister(PWR_CTRL, 0x04);
                    i2cComms.WriteRegister(ACC_CONF, 0x17);
                    i2cComms.WriteRegister(GYR_CONF, 0xA9);
                    i2cComms.WriteRegister(PWR_CONF, 0x03);
                    break;
                case PowerMode.Normal:
                    i2cComms.WriteRegister(PWR_CTRL, 0x0E);
                    i2cComms.WriteRegister(ACC_CONF, 0xA8);
                    i2cComms.WriteRegister(GYR_CONF, 0xA9);
                    i2cComms.WriteRegister(PWR_CONF, 0x02);
                    break;
                case PowerMode.Performance:
                    i2cComms.WriteRegister(PWR_CTRL, 0x0E);
                    i2cComms.WriteRegister(ACC_CONF, 0xA8);
                    i2cComms.WriteRegister(GYR_CONF, 0xE9);
                    i2cComms.WriteRegister(PWR_CONF, 0x02);
                    break;
            }
        }

        private byte[] ReadAccelerationData()
        {
            i2cComms.ReadRegister(0x0C, readBuffer);
            return readBuffer;
        }

        async Task<AngularVelocity3D> ISensor<AngularVelocity3D>.Read()
            => (await Read()).AngularVelocity3D!.Value;

        async Task<Acceleration3D> ISensor<Acceleration3D>.Read()
            => (await Read()).Acceleration3D!.Value;

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature!.Value;

        /// <inheritdoc/>
        public Task BeforeSleep(CancellationToken cancellationToken)
        {
            SetPowerMode(PowerMode.Suspend);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task AfterWake(CancellationToken cancellationToken)
        {
            SetPowerMode(PowerMode.Normal);
            return Task.CompletedTask;
        }
    }
}
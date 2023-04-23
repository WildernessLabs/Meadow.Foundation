using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Represents an Ms5611 pressure and temperature sensor
    /// </summary>
    public partial class Ms5611 :
        ByteCommsSensorBase<(Units.Temperature? Temperature, Pressure? Pressure)>,
        ITemperatureSensor, IBarometricPressureSensor
    {
        /// <summary>
        /// Temperature changed event
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        /// <summary>
        /// Pressure changed event
        /// </summary>
        public event EventHandler<IChangeResult<Pressure>> PressureUpdated = delegate { };

        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The pressure, in hectopascals (hPa), from the last reading. 1 hPa
        /// is equal to one millibar, or 1/10th of a kilopascal (kPa)/centibar
        /// </summary>
        public Pressure? Pressure => Conditions.Pressure;

        private Ms5611Base ms5611;

        /// <summary>
        /// Connect to the Ms5611 using I2C
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the device</param>
        /// <param name="address">I2c address - default is 0x5c</param>
        /// <param name="resolution"></param>
        public Ms5611(II2cBus i2cBus, byte address = (byte)Addresses.Default, Resolution resolution = Resolution.OSR_1024)
        {
            ms5611 = new Ms5611I2c(i2cBus, address, resolution);
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, Pressure? Pressure)> changeResult)
        {
            if (changeResult.New.Temperature is { } temp)
            {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Pressure is { } pressure)
            {
                PressureUpdated?.Invoke(this, new ChangeResult<Pressure>(pressure, changeResult.Old?.Pressure));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(Units.Temperature? Temperature, Pressure? Pressure)> ReadSensor()
        {
            (Units.Temperature? Temperature, Pressure? Pressure) conditions;

            conditions.Temperature = new Units.Temperature(ReadTemperature(), Units.Temperature.UnitType.Celsius);
            conditions.Pressure = new Pressure(ReadPressure(), Units.Pressure.UnitType.Millibar);

            return Task.FromResult(conditions);
        }

        /// <summary>
        /// Reset the MS5611
        /// </summary>
        public void Reset()
        {
            ms5611.Reset();
        }

        private void BeginTempConversion()
        {
            ms5611.BeginTempConversion();
        }

        private void BeginPressureConversion()
        {
            ms5611.BeginPressureConversion();
        }

        private byte[] ReadData()
        {
            return ms5611.ReadData();
        }

        int ReadTemperature()
        {
            ms5611.BeginTempConversion();
            Thread.Sleep(10); // 1 + 2 * Resolution

            // we get back 24 bits (3 bytes), regardless of the resolution we're asking for
            var data = ms5611.ReadData();

            var result = data[2] | data[1] << 8 | data[0] << 16;

            return result;
        }

        int ReadPressure()
        {
            ms5611.BeginPressureConversion();

            Thread.Sleep(10); // 1 + 2 * Resolution

            // we get back 24 bits (3 bytes), regardless of the resolution we're asking for
            var data = ms5611.ReadData();

            var result = data[2] | data[1] << 8 | data[0] << 16;

            return result;
        }

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature.Value;

        async Task<Pressure> ISensor<Pressure>.Read()
            => (await Read()).Pressure.Value;
    }
}
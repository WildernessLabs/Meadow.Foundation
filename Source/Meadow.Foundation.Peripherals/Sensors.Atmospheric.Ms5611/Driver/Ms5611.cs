using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
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
        ITemperatureSensor, IBarometricPressureSensor, II2cPeripheral
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
        /// The current temperature
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The current pressure
        /// </summary>
        public Pressure? Pressure => Conditions.Pressure;

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        readonly Resolution resolution;

        /// <summary>
        /// Connect to the Ms5611 using I2C
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the device</param>
        /// <param name="address">I2c address - default is 0x5c</param>
        /// <param name="resolution"></param>
        public Ms5611(II2cBus i2cBus, byte address = (byte)Addresses.Default, Resolution resolution = Resolution.OSR_1024)
        {
            i2cComms = new I2cCommunications(i2cBus, address);
            this.resolution = resolution;
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
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
            var cmd = (byte)Commands.Reset;

            i2cComms.Write(cmd);
        }

        private void BeginTempConversion()
        {
            var cmd = (byte)((byte)Commands.ConvertD2 + 2 * (byte)resolution);
            i2cComms.Write(cmd);
        }

        private void BeginPressureConversion()
        {
            var cmd = (byte)((byte)Commands.ConvertD1 + 2 * (byte)resolution);
            i2cComms.Write(cmd);
        }

        private byte[] ReadData()
        {
            var data = new byte[3];
            i2cComms.ReadRegister((byte)Commands.ReadADC, data);
            return data;
        }

        int ReadTemperature()
        {
            BeginTempConversion();
            Thread.Sleep(10); // 1 + 2 * Resolution

            // we get back 24 bits (3 bytes), regardless of the resolution we're asking for
            var data = ReadData();

            var result = data[2] | data[1] << 8 | data[0] << 16;

            return result;
        }

        int ReadPressure()
        {
            BeginPressureConversion();

            Thread.Sleep(10); // 1 + 2 * Resolution

            // we get back 24 bits (3 bytes), regardless of the resolution we're asking for
            var data = ReadData();

            var result = data[2] | data[1] << 8 | data[0] << 16;

            return result;
        }

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature!.Value;

        async Task<Pressure> ISensor<Pressure>.Read()
            => (await Read()).Pressure!.Value;
    }
}
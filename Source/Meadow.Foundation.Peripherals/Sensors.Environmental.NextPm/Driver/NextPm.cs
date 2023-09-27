using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents a TERA Sensor NextPM serial particulate matter sensor
    /// </summary>
    public partial class NextPm :
        PollingSensorBase<(
            ParticulateReading? reading10s,
            ParticulateReading? reading1min,
            ParticulateReading? reading15min,
            Units.Temperature? temperature,
            RelativeHumidity? humidity)>,
        IDisposable, IPowerControllablePeripheral
    {
        /// <summary>
        /// Raised when a new 10-second average reading is taken
        /// </summary>
        public event EventHandler<IChangeResult<ParticulateReading>> Readings10sUpdated = delegate { };
        /// <summary>
        /// Raised when a new 1-minute average reading is taken
        /// </summary>
        public event EventHandler<IChangeResult<ParticulateReading>> Readings1minUpdated = delegate { };
        /// <summary>
        /// Raised when a new 15-minute average reading is taken
        /// </summary>
        public event EventHandler<IChangeResult<ParticulateReading>> Readings15minUpdated = delegate { };
        /// <summary>
        /// Raised when a new temperature reading is taken
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        /// <summary>
        /// Raised when a new humidity reading is taken
        /// </summary>
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        /// <summary>
        /// Returns true if the object is disposed, otherwise false
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a NextPm instance
        /// </summary>
        /// <param name="portName"></param>
        public NextPm(SerialPortName portName)
            : this(portName.CreateSerialPort())
        {
        }

        /// <summary>
        /// Creates a NextPm instance
        /// </summary>
        /// <param name="port"></param>
        public NextPm(ISerialPort port)
        {
            _port = InitializePort(port);

            _port.Open();
        }

        /// <summary>
        /// Gets the sensor's firmware
        /// </summary>
        /// <returns>The sensor's firmware version</returns>
        public async Task<short> GetFirmwareVersion()
        {
            await SendCommand(CommandByte.ReadFirmware);
            return (short)(_readBuffer[3] << 8 | _readBuffer[4]);
        }

        /// <summary>
        /// Checks to see if the sensor is in Sleep mode
        /// </summary>
        /// <returns><b>True</b> if the sensor is in Sleep mode</returns>
        public async Task<bool> IsAsleep()
        {
            await SendCommand(CommandByte.ReadSensorState);
            return ((SensorStatus)_readBuffer[2] & SensorStatus.Sleep) == SensorStatus.Sleep;
        }

        /// <summary>
        /// Puts the device into Sleep mode
        /// </summary>
        public async Task PowerOff()
        {
            await SendCommand(CommandByte.SetPowerMode, 0x01);
        }

        /// <summary>
        /// Wakes the device from Sleep mode
        /// </summary>
        public async Task PowerOn()
        {
            await SendCommand(CommandByte.SetPowerMode, 0x00);
        }

        /// <summary>
        /// Gets the fan speed (in percentage)
        /// </summary>
        public async Task<int> GetFanSpeed()
        {
            await SendCommand(CommandByte.ReadWriteFanSpeed, 0x00, 0x00);

            // dev note:
            // this offset doesn't match the data sheet.  The data sheet says there should be 1 byte of response data, but I'm seeing 2 bytes.
            // data sheet also says the value range should be 30-100, but with 2 bytes I see a value of 290, so totally guessing on this response here
            // the data sheet also says it should be 5 bytes, but the sensor requires 6 or it will give an error
            return _readBuffer[4];
        }

        /// <summary>
        /// Sets the sensor fan speed (in percentage)
        /// </summary>
        /// <param name="speedPercent"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task SetFanSpeed(int speedPercent)
        {
            // dev note:
            // the sensor seems to accept the command, but I can' hear any difference in fan speed from 30 to 100%
            if (speedPercent < 30 || speedPercent > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(speedPercent), "Valid range is 30-100");
            }

            await SendCommand(CommandByte.ReadWriteFanSpeed, 0x00, (byte)speedPercent);
        }

        /// <summary>
        /// Gets the sensor's current Temperature and Humidity readings
        /// </summary>
        public async Task<(Units.Temperature temperature, RelativeHumidity humidity)> GetTemperatureAndHumidity()
        {
            await SendCommand(CommandByte.ReadTempAndHumidity);

            var rawTemp = (_readBuffer[3] << 8 | _readBuffer[4]) / 100d;
            var realTemp = 0.9754 * rawTemp - 4.2488; // math from data sheet
            var rawHumidity = (_readBuffer[5] << 8 | _readBuffer[6]) / 100d;
            var realHumidity = 1.1768 * rawHumidity - 4.727; // math from data sheet

            return (new Units.Temperature(realTemp, Units.Temperature.UnitType.Celsius),
                new RelativeHumidity(realHumidity));
        }

        /// <summary>
        /// Gets the average particulate reading for the past 10 seconds
        /// </summary>
        public async Task<ParticulateReading> Get10SecondAverageReading()
        {
            await SendCommand(CommandByte.Read10sConcentrations);
            return new ParticulateReading(_readBuffer, 3);
        }

        /// <summary>
        /// Gets the average particulate reading for the past 60 seconds
        /// </summary>
        public async Task<ParticulateReading> Get1MinuteAverageReading()
        {
            await SendCommand(CommandByte.Read60sConcentrations);
            return new ParticulateReading(_readBuffer, 3);
        }

        /// <summary>
        /// Gets the average particulate reading for the past 15 minutes
        /// </summary>
        public async Task<ParticulateReading> Get15MinuteAverageReading()
        {
            await SendCommand(CommandByte.Read900sConcentrations);
            return new ParticulateReading(_readBuffer, 3);
        }

        /// <inheritdoc/>
        protected override void RaiseEventsAndNotify(IChangeResult<(
            ParticulateReading? reading10s,
            ParticulateReading? reading1min,
            ParticulateReading? reading15min,
            Units.Temperature? temperature,
            RelativeHumidity? humidity)> changeResult)
        {
            if (changeResult.New.reading10s is { } r10)
            {
                Readings10sUpdated?.Invoke(this, new ChangeResult<ParticulateReading>(r10, changeResult.Old?.reading10s));
            }
            if (changeResult.New.reading1min is { } r1)
            {
                Readings1minUpdated?.Invoke(this, new ChangeResult<ParticulateReading>(r1, changeResult.Old?.reading10s));
            }
            if (changeResult.New.reading15min is { } r15)
            {
                Readings15minUpdated?.Invoke(this, new ChangeResult<ParticulateReading>(r15, changeResult.Old?.reading10s));
            }
            if (changeResult.New.temperature is { } temperature)
            {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temperature, changeResult.Old?.temperature));
            }
            if (changeResult.New.humidity is { } humidity)
            {
                HumidityUpdated?.Invoke(this._port, new ChangeResult<RelativeHumidity>(humidity, changeResult.Old?.humidity));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <inheritdoc/>
        protected override async Task<(
            ParticulateReading? reading10s,
            ParticulateReading? reading1min,
            ParticulateReading? reading15min,
            Units.Temperature? temperature,
            RelativeHumidity? humidity)> ReadSensor()
        {
            (ParticulateReading? reading10s, ParticulateReading? reading1min, ParticulateReading? reading15min, Units.Temperature? temperature, RelativeHumidity? humidity) conditions;

            // data sheet indicates you should always read all 4 bytes, in order, for valid data
            try
            {
                conditions.reading10s = await Get10SecondAverageReading();
            }
            catch (TeraException)
            {
                // data likely not ready, or device is asleep
                conditions.reading10s = null;
            }

            try
            {
                conditions.reading1min = await Get1MinuteAverageReading();
            }
            catch (TeraException)
            {
                // data likely not ready, or device is asleep
                conditions.reading1min = null;
            }

            try
            {
                conditions.reading15min = await Get15MinuteAverageReading();
            }
            catch (TeraException)
            {
                // data likely not ready, or device is asleep
                conditions.reading15min = null;
            }

            try
            {
                var th = await GetTemperatureAndHumidity();
                conditions.temperature = th.temperature;
                conditions.humidity = th.humidity;
            }
            catch (TeraException)
            {
                // data likely not ready, or device is asleep
                conditions.temperature = null;
                conditions.humidity = null;
            }

            return conditions;
        }

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _port?.Dispose();
                }
                IsDisposed = true;
            }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
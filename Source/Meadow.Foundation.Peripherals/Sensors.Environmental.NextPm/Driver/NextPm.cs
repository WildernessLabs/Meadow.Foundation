using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents a TERA Sensor NextPM particulate matter sensor
    /// </summary>
    public partial class NextPm : SamplingSensorBase<int>, IDisposable
    {
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
        /// <returns></returns>
        public async Task<short> GetFirmwareVersion()
        {
            await SendCommand(CommandByte.ReadFirmware);
            return (short)(_readBuffer[3] << 8 | _readBuffer[4]);
        }

        /// <summary>
        /// Checks to see if the sensor is in Sleep mode
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsAsleep()
        {
            await SendCommand(CommandByte.ReadSensorState);
            return ((SensorStatus)_readBuffer[2] & SensorStatus.Sleep) == SensorStatus.Sleep;
        }

        /// <summary>
        /// Puts the device into Sleep mode
        /// </summary>
        /// <returns></returns>
        public async Task Sleep()
        {
            await SendCommand(CommandByte.SetPowerMode, 0x01);
        }

        /// <summary>
        /// Wakes the device from Sleep mode
        /// </summary>
        /// <returns></returns>
        public async Task Wake()
        {
            await SendCommand(CommandByte.SetPowerMode, 0x00);
        }

        /// <summary>
        /// Gets the fan speed (in percentage)
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetFanSpeed()
        {
            await SendCommand(CommandByte.ReadWriteFanSpeed, 0x00, 0x00);

            // dev note:
            // this offset doesn't match the data sheet.  The data sheet says there should be 1 byte of response data, but I'm seeing 2 bytes.
            // data sheet also says the value range should be 30-100, but with 2 bytes I see a value of 290, so totally guessing on this reponse here
            // the data sheet also says it should be 5 bytes, but the sensor requires 6 or it will give an error
            return _readBuffer[4];
        }

        /// <summary>
        /// Sets the sensor fan speed (in percentage)
        /// </summary>
        /// <param name="speedPercent"></param>
        /// <returns></returns>
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
        /// <returns></returns>
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
        /// <returns></returns>
        public async Task<ParticulateReading> Get10SecondAverageReading()
        {
            await SendCommand(CommandByte.Read10sConcentrations);
            return new ParticulateReading(_readBuffer, 3);
        }

        /// <summary>
        /// Gets the average particulate reading for the past 60 seconds
        /// </summary>
        /// <returns></returns>
        public async Task<ParticulateReading> Get1MinuteAverageReading()
        {
            await SendCommand(CommandByte.Read60sConcentrations);
            return new ParticulateReading(_readBuffer, 3);
        }

        /// <summary>
        /// Gets the average particulate reading for the past 15 minutes
        /// </summary>
        /// <returns></returns>
        public async Task<ParticulateReading> Get15MinueAverageReading()
        {
            await SendCommand(CommandByte.Read900sConcentrations);
            return new ParticulateReading(_readBuffer, 3);
        }

        /// <inheritdoc/>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void StopUpdating()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override Task<int> ReadSensor()
        {
            throw new NotImplementedException();
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
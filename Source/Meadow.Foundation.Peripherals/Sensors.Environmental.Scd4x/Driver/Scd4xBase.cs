using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Base class for SCD4x series of C02 sensors
    /// </summary>
    public abstract partial class Scd4xBase
        : PollingSensorBase<(Concentration? Concentration, Units.Temperature? Temperature, RelativeHumidity? Humidity)>,
            ITemperatureSensor, IHumiditySensor, ICO2ConcentrationSensor, II2cPeripheral
    {
        private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers = default!;
        private event EventHandler<IChangeResult<RelativeHumidity>> _humidityHandlers = default!;
        private event EventHandler<IChangeResult<Concentration>> _concentrationHandlers = default!;

        event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
        {
            add => _temperatureHandlers += value;
            remove => _temperatureHandlers -= value;
        }

        event EventHandler<IChangeResult<RelativeHumidity>> ISamplingSensor<RelativeHumidity>.Updated
        {
            add => _humidityHandlers += value;
            remove => _humidityHandlers -= value;
        }

        event EventHandler<IChangeResult<Concentration>> ISamplingSensor<Concentration>.Updated
        {
            add => _concentrationHandlers += value;
            remove => _concentrationHandlers -= value;
        }

        /// <summary>
        /// The current C02 concentration value
        /// </summary>
        public Concentration? CO2Concentration => Conditions.Concentration;

        /// <summary>
        /// The current temperature
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The current humidity
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

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
        /// Create a new Scd4xBase object
        /// </summary>
        /// <remarks>
        /// The constructor sends the stop periodic updates method otherwise 
        /// the sensor may not respond to new commands
        /// </remarks>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Scd4xBase(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            readBuffer = new byte[9];

            i2cComms = new I2cCommunications(i2cBus, address, 9);
            StopPeriodicUpdates().Wait();
        }

        /// <summary>
        /// Re-initialize the sensor
        /// </summary>
        public Task ReInitialize()
        {
            SendCommand(Commands.ReInit);
            return Task.Delay(1000);
        }

        /// <summary>
        /// Forced recalibration allows recalibration using an external CO2 reference
        /// </summary>
        public Task PerformForcedRecalibration()
        {
            SendCommand(Commands.PerformForcedCalibration);
            return Task.Delay(400);
        }

        /// <summary>
        /// Persist settings to EEPROM
        /// </summary>
        public void PersistSettings()
        {
            SendCommand(Commands.PersistSettings);
        }

        /// <summary>
        /// Device factory reset and clear all saved settings
        /// </summary>
        public void PerformFactoryReset()
        {
            SendCommand(Commands.PerformFactoryReset);
        }

        /// <summary>
        /// Get Serial Number from the device
        /// </summary>
        /// <returns>a 48bit (6 byte) serial number as a byte array</returns>
        public byte[] GetSerialNumber()
        {
            SendCommand(Commands.GetSerialNumber);
            Thread.Sleep(1);

            var data = new byte[9];
            i2cComms.Read(data);

            var ret = new byte[6];

            ret[0] = data[0];
            ret[1] = data[1];
            ret[2] = data[3];
            ret[3] = data[4];
            ret[4] = data[6];
            ret[5] = data[7];

            return ret;
        }

        /// <summary>
        /// Is there sensor measurement data ready
        /// Sensor returns data ~5 seconds in normal operation and ~30 seconds in low power mode
        /// </summary>
        /// <returns>True if ready</returns>
        protected bool IsDataReady()
        {
            SendCommand(Commands.GetDataReadyStatus);
            Thread.Sleep(1);
            var data = new byte[3];
            i2cComms.Read(data);

            if (data[1] == 0 && (data[0] & 0x07) == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Stop the sensor from sampling
        /// The sensor will not respond to commands for 500ms
        /// </summary>
        private Task StopPeriodicUpdates()
        {
            SendCommand(Commands.StopPeriodicMeasurement);
            return Task.Delay(500);
        }

        /// <summary>
        /// Starts updating the sensor on the updateInterval frequency specified
        /// The sensor updates every 5 seconds, its recommended to choose an interval of 5s or longer
        /// If the update interval is 30 seconds or longer, the sensor will run in low power mode
        /// </summary>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            if (IsSampling)
            {
                return;
            }

            if (updateInterval != null && updateInterval.Value.TotalSeconds >= 30)
            {
                SendCommand(Commands.StartLowPowerPeriodicMeasurement);
            }
            else
            {
                SendCommand(Commands.StartPeriodicMeasurement);
            }
            base.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stop updating the sensor
        /// The sensor will not respond to commands for 500ms 
        /// The call will delay the calling thread for 500ms
        /// </summary>
        public override void StopUpdating()
        {
            _ = StopPeriodicUpdates();
            base.StopUpdating();
        }

        private void SendCommand(Commands command)
        {
            var data = new byte[2];
            data[0] = (byte)((ushort)command >> 8);
            data[1] = (byte)(ushort)command;

            i2cComms.Write(data);
        }

        /// <summary>
        /// Get Scdx40 C02 Gas Concentration and
        /// Update the Concentration property
        /// </summary>
        protected override async Task<(Concentration? Concentration, Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
        {
            while (IsDataReady() == false)
            {
                await Task.Delay(500);
            }

            (Concentration Concentration, Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

            SendCommand(Commands.ReadMeasurement);
            Thread.Sleep(1);
            i2cComms.Read(readBuffer);

            int value = readBuffer[0] << 8 | readBuffer[1];
            conditions.Concentration = new Concentration(value, Concentration.UnitType.PartsPerMillion);

            conditions.Temperature = CalcTemperature(readBuffer[3], readBuffer[4]);

            value = readBuffer[6] << 8 | readBuffer[8];
            double humidiy = 100 * value / 65536.0;
            conditions.Humidity = new RelativeHumidity(humidiy, RelativeHumidity.UnitType.Percent);

            return conditions;
        }

        private Units.Temperature CalcTemperature(byte valueHigh, byte valueLow)
        {
            int value = valueHigh << 8 | valueLow;
            double temperature = -45 + value * 175 / 65536.0;
            return new Units.Temperature(temperature, Units.Temperature.UnitType.Celsius);
        }

        /// <summary>
        /// Raise change events for subscribers
        /// </summary>
        /// <param name="changeResult">The change result with the current sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Concentration? Concentration, Units.Temperature? Temperature, RelativeHumidity? Humidity)> changeResult)
        {
            if (changeResult.New.Temperature is { } temperature)
            {
                _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temperature, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity)
            {
                _humidityHandlers?.Invoke(this, new ChangeResult<RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            if (changeResult.New.Concentration is { } concentration)
            {
                _concentrationHandlers?.Invoke(this, new ChangeResult<Concentration>(concentration, changeResult.Old?.Concentration));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature!.Value;

        async Task<RelativeHumidity> ISensor<RelativeHumidity>.Read()
            => (await Read()).Humidity!.Value;

        async Task<Concentration> ISensor<Concentration>.Read()
            => (await Read()).Concentration!.Value;
    }
}
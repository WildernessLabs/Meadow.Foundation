using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide a mechanism for reading the temperature and humidity from
    /// a SHT31D temperature / humidity sensor.
    /// </summary>
    /// <remarks>
    /// Readings from the sensor are made in Single-shot mode.
    /// </remarks>
    public class Sht31d :
        SensorBase<(Units.Temperature?, RelativeHumidity?)>,
        ITemperatureSensor, IHumiditySensor
    {
        public event EventHandler<IChangeResult<(Units.Temperature?, RelativeHumidity?)>> Updated;
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated;
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated;

        /// <summary>
        ///     SH31D sensor communicates using I2C.
        /// </summary>
        private readonly II2cPeripheral sht31d;

        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        /// The AtmosphericConditions from the last reading.
        /// </summary>
        public (Units.Temperature? Temperature, RelativeHumidity? Humidity) Conditions;

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        ///     Create a new SHT31D object.
        /// </summary>
        /// <param name="address">Sensor address (should be 0x44 or 0x45).</param>
        /// <param name="i2cBus">I2cBus (0-1000 KHz).</param>
        public Sht31d(II2cBus i2cBus, byte address = 0x44)
        {
            sht31d = new I2cPeripheral(i2cBus, address);
        }

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> Read()
        {
            Update();

            return Task.FromResult(Conditions);
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) { return; }

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                (Units.Temperature?, RelativeHumidity?) oldConditions;
                ChangeResult<(Units.Temperature?, RelativeHumidity?)> result;

                Task.Factory.StartNew(async () => {
                    while (true) {
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Conditions;

                        // read
                        Update();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<(Units.Temperature?, RelativeHumidity?)>(Conditions, oldConditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            if (changeResult.New.Temperature is { } temp) {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity) {
                HumidityUpdated?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock) {
                if (!IsSampling) { return; }

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        ///     Get a reading from the sensor and set the Temperature and Humidity properties.
        /// </summary>
        public void Update()
        {
            var data = sht31d.WriteRead(new byte[] { 0x2c, 0x06 }, 6);
            var humidity = (100 * (float)((data[3] << 8) + data[4])) / 65535;
            var tempC = ((175 * (float)((data[0] << 8) + data[1])) / 65535) - 45;

            Conditions.Humidity = new RelativeHumidity(humidity);
            Conditions.Temperature = new Units.Temperature(tempC, Units.Temperature.UnitType.Celsius);
        }

        /// <summary>
        /// Creates a `FilterableChangeObserver` that has a handler and a filter.
        /// </summary>
        /// <param name="handler">The action that is invoked when the filter is satisifed.</param>
        /// <param name="filter">An optional filter that determines whether or not the
        /// consumer should be notified.</param>
        /// <returns></returns>
        /// <returns></returns>
        // Implementor Notes:
        //  This is a convenience method that provides named tuple elements. It's not strictly
        //  necessary, as the `FilterableChangeObservableBase` class provides a default implementation,
        //  but if you use it, then the parameters are named `Item1`, `Item2`, etc. instead of
        //  `Temperature`, `Pressure`, etc.
        public static new
            FilterableChangeObserver<(Units.Temperature?, RelativeHumidity?)>
            CreateObserver(
                Action<IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>> handler,
                Predicate<IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>>? filter = null
            )
        {
            return new FilterableChangeObserver<(Units.Temperature?, RelativeHumidity?)>(
                handler: handler, filter: filter
                );
        }
    }
}
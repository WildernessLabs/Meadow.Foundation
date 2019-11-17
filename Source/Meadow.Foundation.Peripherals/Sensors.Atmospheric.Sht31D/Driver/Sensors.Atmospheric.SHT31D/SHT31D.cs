using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide a mechanism for reading the temperature and humidity from
    /// a SHT31D temperature / humidity sensor.
    /// </summary>
    /// <remarks>
    /// Readings from the sensor are made in Single-shot mode.
    /// </remarks>
    public class SHT31D : FilterableObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        IAtmosphericSensor, ITemperatureSensor, IHumiditySensor
    {
        #region Constants

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        #endregion Constants

        #region Member variables / fields

        /// <summary>
        ///     SH31D sensor communicates using I2C.
        /// </summary>
        private readonly II2cPeripheral sht31d;

        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort _updateInterval = 100;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        /// The temperature, in degrees celsius (ºC), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public float Humidity => Conditions.Humidity;


        private float _lastNotifiedHumidity = 0.0F;
        private float _lastNotifiedTemperature = 0.0F;

        /// <summary>
        ///     Any changes in the temperature that are greater than the temperature
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public float TemperatureChangeNotificationThreshold { get; set; } = 0.001F;

        /// <summary>
        ///     Any changes in the humidity that are greater than the humidity
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public float HumidityChangeNotificationThreshold { get; set; } = 0.001F;

        /// <summary>
        /// The AtmosphericConditions from the last reading.
        /// </summary>
        public AtmosphericConditions Conditions { get; protected set; } = new AtmosphericConditions();

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        #endregion Properties

        #region Events and delegates

        public event EventHandler<AtmosphericConditionChangeResult> Updated;

        #endregion Events and delegates

        #region Constructors

        /// <summary>
        ///     Default constructor (made private to prevent it being called).
        /// </summary>
        private SHT31D()
        {
        }

        /// <summary>
        ///     Create a new SHT31D object.
        /// </summary>
        /// <param name="address">Sensor address (should be 0x44 or 0x45).</param>
        /// <param name="i2cBus">I2cBus (0-1000 KHz).</param>
        public SHT31D(II2cBus i2cBus, byte address = 0x44)
        {
            if ((address != 0x44) && (address != 0x45))
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address should be 0x44 or 0x45");
            }

            sht31d = new I2cPeripheral(i2cBus, address);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public async Task<AtmosphericConditions> Read()
        {
            Conditions = await Read();

            return Conditions;
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock)
            {
                if (IsSampling) return;

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                AtmosphericConditions oldConditions;
                AtmosphericConditionChangeResult result;
                Task.Factory.StartNew(async () => {
                    while (true)
                    {
                        // TODO: someone please review; is this the correct
                        // place to do this?
                        // check for cancel (doing this here instead of 
                        // while(!ct.IsCancellationRequested), so we can perform 
                        // cleanup
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Conditions;

                        // read
                        Update();

                        // build a new result with the old and new conditions
                        result = new AtmosphericConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(AtmosphericConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock)
            {
                if (!IsSampling) return;

                if (SamplingTokenSource != null)
                {
                    SamplingTokenSource.Cancel();
                }

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
            Conditions.Humidity = (100 * (float) ((data[3] << 8) + data[4])) / 65535;
            Conditions.Temperature = ((175 * (float) ((data[0] << 8) + data[1])) / 65535) - 45;
        }

        #endregion
    }
}
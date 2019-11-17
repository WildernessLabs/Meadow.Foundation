using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide a mechanism for reading the Temperature and Humidity from
    /// a HIH6130 temperature and Humidity sensor.
    /// </summary>
    public class HIH6130 : FilterableObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        IAtmosphericSensor, ITemperatureSensor, IHumiditySensor
    {
        #region Member variables / fields

        /// <summary>
        ///     HIH6130 sensor object.
        /// </summary>
        private readonly II2cPeripheral hih6130;

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
        ///     Default constructor is private to prevent the developer from calling it.
        /// </summary>
        private HIH6130()
        {
        }

        /// <summary>
        ///     Create a new HIH6130 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the HIH6130 (default = 0x27).</param>
        /// <param name="i2cBus">I2C bus (default = 100 KHz).</param>
        public HIH6130(II2cBus i2cBus, byte address = 0x27)
        {
            hih6130 = new I2cPeripheral(i2cBus, address);
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
                        await Update();

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

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        ///     Force the sensor to make a reading and update the relevant properties.
        /// </summary>
        public async Task Update()
        {
            hih6130.WriteByte(0);
            //
            //  Sensor takes 35ms to make a valid reading.
            //
            await Task.Delay(40);

            var data = hih6130.ReadBytes(4);
            //
            //  Data format:
            //
            //  Byte 0: S1  S0  H13 H12 H11 H10 H9 H8
            //  Byte 1: H7  H6  H5  H4  H3  H2  H1 H0
            //  Byte 2: T13 T12 T11 T10 T9  T8  T7 T6
            //  Byte 4: T5  T4  T3  T2  T1  T0  XX XX
            //
            if ((data[0] & 0xc0) != 0)
            {
                throw new Exception("Status indicates readings are invalid.");
            }
            var reading = ((data[0] << 8) | data[1]) & 0x3fff;
            Conditions.Humidity = ((float) reading / 16383) * 100;
            reading = ((data[2] << 8) | data[3]) >> 2;
            Conditions.Temperature = (((float) reading / 16383) * 165) - 40;
        }

        #endregion Methods
    }
}
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
    public abstract class DhtBase : FilterableChangeObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        IAtmosphericSensor, ITemperatureSensor, IHumiditySensor
    {
        

        /// <summary>
        ///     DHT12 sensor object.
        /// </summary>
        protected readonly II2cPeripheral _sensor;

        /// <summary>
        /// Read buffer
        /// </summary>
        protected byte[] _readBuffer = new byte[5];

        private readonly BusType _protocol;

        private int _lastMeasurement = 0;

        

        

        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature.Value;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public float Humidity => Conditions.Humidity.Value;

        /// <summary>
        /// The AtmosphericConditions from the last reading.
        /// </summary>
        public AtmosphericConditions Conditions { get; protected set; } = new AtmosphericConditions();

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the sensor. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling, otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// How last read went, <c>true</c> for success, <c>false</c> for failure
        /// </summary>
        public bool WasLastReadSuccessful { get; internal set; }

        
        private enum BusType
        {
            I2C,
            OneWire,
        }

        

        

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        

        

        

        public event EventHandler<AtmosphericConditionChangeResult> Updated;

        

        

        /// <summary>
        /// Create a DHT sensor through I2C (Only DHT12)
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public DhtBase(II2cBus i2cBus, byte address = 0x5C)
        {
            _sensor = new I2cPeripheral(i2cBus, address);
            _protocol = BusType.I2C;

            //give the device time to initialize
            Thread.Sleep(1000);
        }

        

        

        /// <summary>
        /// Start a reading
        /// </summary>
        internal virtual void ReadData()
        {
            // Dht device reads should be at least 1s apart, use the previous measurement if less than 1000ms
            if (Environment.TickCount - _lastMeasurement < 1000) {
                return;
            }

            if (_protocol == BusType.OneWire) {
                ReadDataOneWire();
            } else {
                ReadDataI2c();
            }
        }

        /// <summary>
        /// Read through One-Wire
        /// </summary>
        internal virtual void ReadDataOneWire()
        {
            throw new NotImplementedException("ReadDataOneWire()");
        }

        /// <summary>
        /// Read data via I2C (DHT12, etc.)
        /// </summary>
        internal virtual void ReadDataI2c()
        {
            _sensor.WriteByte(0x00);
            _readBuffer = _sensor.ReadBytes(5);

            _lastMeasurement = Environment.TickCount;

            if ((_readBuffer[4] == ((_readBuffer[0] + _readBuffer[1] + _readBuffer[2] + _readBuffer[3]) & 0xFF))) {
                WasLastReadSuccessful = (_readBuffer[0] != 0) || (_readBuffer[2] != 0);
            } else {
                WasLastReadSuccessful = false;
            }
        }

        /// <summary>
        /// Converting data to humidity
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Humidity</returns>
        internal abstract float GetHumidity(byte[] data);

        /// <summary>
        /// Converting data to Temperature
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Temperature</returns>
        internal abstract float GetTemperature(byte[] data);

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public async Task<AtmosphericConditions> Read()
        {
            await Update();

            return Conditions;
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) return;

                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                AtmosphericConditions oldConditions;
                AtmosphericConditionChangeResult result;

                Task.Factory.StartNew(async () => {
                    while (true) {
                        // cleanup
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = AtmosphericConditions.From(Conditions);

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
            lock (_lock) {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        ///     Force the sensor to make a reading and update the relevant properties.
        /// </summary>
        public Task Update()
        {
            if (_protocol == BusType.I2C) {
                ReadDataI2c();
            } else {
                ReadDataOneWire();
            }

            Conditions.Humidity = GetHumidity(_readBuffer);
            Conditions.Temperature = GetTemperature(_readBuffer);

            return Task.CompletedTask;
        }
        
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Atmospheric.Dhtxx
{
    /// <summary>
    /// Provide a mechanism for reading the Temperature and Humidity from
    /// a DHT temperature and Humidity sensor.
    /// </summary>
    public abstract class DhtBase : 
        FilterableChangeObservable<CompositeChangeResult<Units.Temperature, RelativeHumidity>, Units.Temperature, RelativeHumidity>,
        ITemperatureSensor, IHumiditySensor
    {
        /// <summary>
        ///     DHT12 sensor object.
        /// </summary>
        protected readonly II2cPeripheral sensor;

        /// <summary>
        /// Read buffer
        /// </summary>
        protected byte[] readBuffer = new byte[5];

        private readonly BusType protocol;

        private int lastMeasurement = 0;

        /// <summary>
        /// The temperature
        /// </summary>
        public Units.Temperature Temperature => Conditions.Temperature;

        /// <summary>
        /// The relative humidity
        /// </summary>
        public RelativeHumidity Humidity => Conditions.Humidity;

        public (Units.Temperature Temperature, RelativeHumidity Humidity) Conditions;
     
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

        public event EventHandler<CompositeChangeResult<Units.Temperature, RelativeHumidity>> Updated;
        public event EventHandler<CompositeChangeResult<Units.Temperature>> TemperatureUpdated;
        public event EventHandler<CompositeChangeResult<RelativeHumidity>> HumidityUpdated;

        /// <summary>
        /// Create a DHT sensor through I2C (Only DHT12)
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public DhtBase(II2cBus i2cBus, byte address = 0x5C)
        {
            sensor = new I2cPeripheral(i2cBus, address);
            protocol = BusType.I2C;

            //give the device time to initialize
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Start a reading
        /// </summary>
        internal virtual void ReadData()
        {
            // Dht device reads should be at least 1s apart, use the previous measurement if less than 1000ms
            if (Environment.TickCount - lastMeasurement < 1000) 
            {
                return;
            }

            if (protocol == BusType.OneWire) 
            {
                ReadDataOneWire();
            } 
            else 
            {
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
            sensor.WriteByte(0x00);
            readBuffer = sensor.ReadBytes(5);

            lastMeasurement = Environment.TickCount;

            if ((readBuffer[4] == ((readBuffer[0] + readBuffer[1] + readBuffer[2] + readBuffer[3]) & 0xFF))) 
            {
                WasLastReadSuccessful = (readBuffer[0] != 0) || (readBuffer[2] != 0);
            } 
            else 
            {
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
        public async Task<(Units.Temperature Temperature, RelativeHumidity Humidity)> Read()
        {
            await Update();

            return Conditions;
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock) 
            {
                if (IsSampling) return;

                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                (Units.Temperature, RelativeHumidity) oldConditions;
                CompositeChangeResult<Units.Temperature, RelativeHumidity> result;

                Task.Factory.StartNew(async () => {
                    while (true) {
                        // cleanup
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Conditions;

                        // read
                        await Update();

                        // build a new result with the old and new conditions
                        result = new CompositeChangeResult<Units.Temperature, RelativeHumidity>(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(CompositeChangeResult<Units.Temperature, RelativeHumidity> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            HumidityUpdated?.Invoke(this, new CompositeChangeResult<RelativeHumidity>(changeResult.New.Value.Unit2, changeResult.Old.Value.Unit2));
            TemperatureUpdated?.Invoke(this, new CompositeChangeResult<Units.Temperature>(changeResult.New.Value.Unit1, changeResult.Old.Value.Unit1));
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
        public Task Update()
        {
            if (protocol == BusType.I2C) 
            {
                ReadDataI2c();
            } 
            else 
            {
                ReadDataOneWire();
            }

            Conditions.Humidity = new RelativeHumidity(GetHumidity(readBuffer), RelativeHumidity.UnitType.Percent);
            Conditions.Temperature = new Units.Temperature(GetTemperature(readBuffer), Units.Temperature.UnitType.Celsius);

            return Task.CompletedTask;
        }
    }
}
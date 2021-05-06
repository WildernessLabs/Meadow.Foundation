using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide a mechanism for reading the Temperature and Humidity from
    /// a HIH6130 temperature and Humidity sensor.
    /// </summary>
    public class Hih6130 :
        FilterableChangeObservableBase<ChangeResult<(Units.Temperature, RelativeHumidity)>, (Units.Temperature, RelativeHumidity)>,
        ITemperatureSensor, IHumiditySensor
    {
        /// <summary>
        /// </summary>
        public event EventHandler<ChangeResult<(Units.Temperature, RelativeHumidity)>> Updated = delegate { };
        public event EventHandler<ChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<ChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        /// <summary>
        ///     HIH6130 sensor object.
        /// </summary>
        private readonly II2cPeripheral hih6130;

        /// <summary>
        /// The temperature, from the last reading.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        /// The last read conditions.
        /// </summary>
        public (Units.Temperature Temperature, RelativeHumidity Humidity) Conditions;

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
        ///     Create a new HIH6130 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the HIH6130 (default = 0x27).</param>
        /// <param name="i2cBus">I2C bus (default = 100 KHz).</param>
        public Hih6130(II2cBus i2cBus, byte address = 0x27)
        {
            hih6130 = new I2cPeripheral(i2cBus, address);
        }

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public async Task<(Units.Temperature Temperature, RelativeHumidity Humidity)> Read()
        {
            // update confiruation for a one-off read
            this.Conditions = await ReadSensor();
            return Conditions;
        }

        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) return;

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                (Units.Temperature Temperature, RelativeHumidity Humidity) oldConditions;
                ChangeResult<(Units.Temperature, RelativeHumidity)> result;

                Task.Factory.StartNew(async () => {
                    while (true) {
                        // cleanup
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = (Conditions.Temperature, Conditions.Humidity);

                        // read
                        Conditions = await Read();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<(Units.Temperature, RelativeHumidity)>(Conditions, oldConditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Inheritance-safe way to raise events and notify observers.
        /// </summary>
        /// <param name="changeResult"></param>
        protected void RaiseChangedAndNotify(ChangeResult<(Units.Temperature Temperature, RelativeHumidity Humidity)> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(changeResult.New.Temperature, changeResult.Old?.Temperature));
            HumidityUpdated?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(changeResult.New.Humidity, changeResult.Old?.Humidity));
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
        protected async Task<(Units.Temperature Temperature, RelativeHumidity Humidity)> ReadSensor()
        {
            (Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

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
            if ((data[0] & 0xc0) != 0) {
                throw new Exception("Status indicates readings are invalid.");
            }
            var reading = ((data[0] << 8) | data[1]) & 0x3fff;
            conditions.Humidity = new RelativeHumidity(((float)reading / 16383) * 100);
            reading = ((data[2] << 8) | data[3]) >> 2;
            conditions.Temperature = new Units.Temperature((((float)reading / 16383) * 165) - 40, Units.Temperature.UnitType.Celsius);

            return conditions;
        }

        
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Units;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Grove TH02 temperature and humidity sensor.
    /// </summary>
    public class GroveTh02 :
        FilterableChangeObservableBase<(Units.Temperature, RelativeHumidity)>,
        ITemperatureSensor, IHumiditySensor
    {
        /// <summary>
        /// </summary>
        public event EventHandler<IChangeResult<(Units.Temperature, RelativeHumidity)>> Updated = delegate { };
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        /// <summary>
        ///     Start measurement bit in the configuration register.
        /// </summary>
        private const byte StartMeasurement = 0x01;

        /// <summary>
        ///     Measure temperature bit in the configuration register.
        /// </summary>
        private const byte MeasureTemperature = 0x10;

        /// <summary>
        ///     Heater control bit in the configuration register.
        /// </summary>
        private const byte HeaterOnBit = 0x02;

        /// <summary>
        ///     Mask used to turn the heater off.
        /// </summary>
        private const byte HeaterMask = 0xfd;

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 200;

        /// <summary>
        ///     Register addresses in the Grove TH02 sensor.
        /// </summary>
        private class Registers
        {
            /// <summary>
            ///     Status register.
            /// </summary>
            public const byte Status = 0x00;

            /// <summary>
            ///     High byte of the data register.
            /// </summary>
            public const byte DataHigh = 0x01;

            /// <summary>
            ///     Low byte of the data register.
            /// </summary>
            public const byte DataLow = 0x02;

            /// <summary>
            ///     Addess of the configuration register.
            /// </summary>
            public const byte Config = 0x04;

            /// <summary>
            ///     Address of the ID register.
            /// </summary>
            public const byte ID = 0x11;
        }

        /// <summary>
        ///     GroveTH02 object.
        /// </summary>
        private readonly II2cPeripheral groveTH02;

        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort _updateInterval = 100;

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

        /// <summary>
        ///     Get / set the heater status.
        /// </summary>
        public bool HeaterOn {
            get {
                return ((groveTH02.ReadRegister(Registers.Config) & HeaterOnBit) > 0);
            }
            set {
                byte config = groveTH02.ReadRegister(Registers.Config);
                if (value) {
                    config |= HeaterOnBit;
                } else {
                    config &= HeaterMask;
                }
                groveTH02.WriteRegister(Registers.Config, config);
            }
        }

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
        ///     Create a new GroveTH02 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the Grove TH02 (default = 0x4-).</param>
        /// <param name="i2cBus">I2C bus (default = 100 KHz).</param>
        public GroveTh02(II2cBus i2cBus, byte address = 0x40)
        {
            groveTH02 = new I2cPeripheral(i2cBus, address);
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
        protected void RaiseChangedAndNotify(IChangeResult<(Units.Temperature Temperature, RelativeHumidity Humidity)> changeResult)
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

            int temp = 0;
            //
            //  Get the humidity first.
            //
            groveTH02.WriteRegister(Registers.Config, StartMeasurement);
            //
            //  Maximum conversion time should be 40ms but loop just in case 
            //  it takes longer.
            //

            await Task.Delay(40);

            while ((groveTH02.ReadRegister(Registers.Status) & 0x01) > 0) ;
            byte[] data = groveTH02.ReadRegisters(Registers.DataHigh, 2);
            temp = data[0] << 8;
            temp |= data[1];
            temp >>= 4;
            conditions.Humidity = new RelativeHumidity( (((float)temp) / 16) - 24);
            //
            //  Now get the temperature.
            //
            groveTH02.WriteRegister(Registers.Config, StartMeasurement | MeasureTemperature);
            //
            //  Maximum conversion time should be 40ms but loop just in case 
            //  it takes longer.
            //
            await Task.Delay(40);

            while ((groveTH02.ReadRegister(Registers.Status) & 0x01) > 0) ;
            data = groveTH02.ReadRegisters(Registers.DataHigh, 2);
            temp = data[0] << 8;
            temp |= data[1];
            temp >>= 2;
            conditions.Temperature = new Units.Temperature( (((float)temp) / 32) - 50);

            return conditions;
        }
    }
}

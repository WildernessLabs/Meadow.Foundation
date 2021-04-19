using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// TMP102 Temperature sensor object.
    /// </summary>    
    public class Lm75 :
        FilterableChangeObservable<CompositeChangeResult<Units.Temperature>, Units.Temperature?>,
        ITemperatureSensor
    {
        /// <summary>
        /// LM75 Registers
        /// </summary>
        enum Register : byte
        {
            LM_TEMP = 0x00,
            LM_CONFIG = 0x01,
            LM_THYST = 0x02,
            LM_TOS = 0x03
        }

        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        private readonly II2cPeripheral lm75;

        public byte DEFAULT_ADDRESS => 0x48;

        /// <summary>
        /// The Temperature value from the last reading.
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        
        

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<CompositeChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        /// <summary>
        ///     Create a new TMP102 object using the default configuration for the sensor.
        /// </summary>
        /// <param name="address">I2C address of the sensor.</param>
        public Lm75(II2cBus i2cBus, byte address = 0x48)
        {
            lm75 = new I2cPeripheral(i2cBus, address);
        }

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        // TODO: Make this async?
        public Units.Temperature Read()
        {
            Update();
            return Temperature;
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `TemperatureUpdated` events and IObservable
        /// subscribers getting notified.
        /// </summary>
        /// <param name="standbyDuration">The time, in milliseconds, to wait
        /// between sets of sample readings. This value determines how often
        /// `TemperatureUpdated` events are raised and `IObservable` consumers are notified.</param>
        public void StartUpdating(int standbyDuration = 1000)
        {
            lock (_lock) 
            {
                if (IsSampling) { return; }
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                Units.Temperature oldtemperature;
                CompositeChangeResult<Units.Temperature> result;

                Task.Factory.StartNew(async () => 
                {
                    while (true) 
                    {
                        if (ct.IsCancellationRequested) 
                        {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldtemperature = Temperature;

                        // read
                        Update(); //synchronous for this driver 

                        // build a new result with the old and new conditions
                        result = new CompositeChangeResult<Units.Temperature>(oldtemperature, Temperature);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
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
        /// Update the Temperature property.
        /// </summary>
        protected void Update()
        {
            lm75.WriteByte((byte)Register.LM_TEMP);

            var data = lm75.ReadRegisters((byte)Register.LM_TEMP, 2);

            // Details in Datasheet P10
            double temp = 0;
            ushort raw = (ushort)((data[0] << 3) | (data[1] >> 5));
            if ((data[0] & 0x80) == 0) {
                // temperature >= 0
                temp = raw * 0.125;
            } else {
                raw |= 0xF800;
                raw = (ushort)(~raw + 1);

                temp = raw * (-1) * 0.125;
            }

            //only accurate to +/- 0.1 degrees
            Temperature = new Units.Temperature((float)Math.Round(temp, 1), Units.Temperature.UnitType.Celsius);
        }

        protected void RaiseChangedAndNotify(CompositeChangeResult<Units.Temperature> changeResult)
        {
            TemperatureUpdated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }
    }
}
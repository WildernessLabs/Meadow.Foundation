using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// TMP102 Temperature sensor object.
    /// </summary>    
    public class Lm75 :
        FilterableChangeObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        IAtmosphericSensor, ITemperatureSensor
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

        /// <summary>
        ///     LM75 sensor.
        /// </summary>
        private readonly II2cPeripheral lm75;


        public byte DEFAULT_ADDRESS => 0x48;

        /// <summary>
        /// The AtmosphericConditions from the last reading.
        /// </summary>
        public AtmosphericConditions Conditions { get; protected set; } = new AtmosphericConditions();

        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature.Value;

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        

        

        public event EventHandler<AtmosphericConditionChangeResult> Updated;

        

        

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
        public async Task<AtmosphericConditions> Read()
        {
            Conditions = await Read();

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

                AtmosphericConditions oldConditions;
                AtmosphericConditionChangeResult result;
                Task.Factory.StartNew(async () => {
                    while (true) {
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = AtmosphericConditions.From(Conditions);

                        // read
                        Update(); //syncrhnous for this driver 

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
        ///     Update the Temperature property.
        /// </summary>
        public void Update()
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
            Conditions.Temperature = (float)Math.Round(temp, 1);
        }

        
    }
}
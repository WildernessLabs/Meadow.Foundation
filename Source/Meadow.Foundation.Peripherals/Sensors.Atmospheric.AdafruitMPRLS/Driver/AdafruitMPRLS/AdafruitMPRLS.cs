using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Atmospheric;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Device driver for the Adafruit MPRLS Ported Pressure Sensor Breakout
    /// https://www.adafruit.com/product/3965
    /// Device datasheets also available here: https://sensing.honeywell.com/micropressure-mpr-series
    /// </summary>
    public class AdafruitMPRLSSensor : FilterableChangeObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>, IAtmosphericSensor
    {
        private readonly II2cPeripheral i2cPeripheral;

        //Defined in section 6.6.1 of the datasheet.
        private readonly byte[] mprlsMeasurementCommand = { 0xAA, 0x00, 0x00 };
        private object _lock = new object();
        private CancellationTokenSource samplingTokenSource;

        public AtmosphericConditions Conditions { get; protected set; } = new AtmosphericConditions();

        /// <summary>
        /// Indicates that the sensor is in use.
        /// </summary>
        public bool IsSampling { get; set; } = false;

        /// <summary>
        /// Set by the sensor, to tell us it has power.
        /// </summary>
        public bool IsDevicePowered { get; set; }

        /// <summary>
        /// Set by the sensor, to tell us it's busy.
        /// </summary>
        public bool IsDeviceBusy { get; set; }

        /// <summary>
        /// Set by the sensor, to tell us whether or not there's an issue with its own memory.
        /// </summary>
        public bool HasMemoryIntegrityFailed { get; set; }

        /// <summary>
        /// Convienence property to get the raw measurement from the sensor.
        /// </summary>
        public float RawPSIMeasurement { get; set; }

        /// <summary>
        /// Convienence property to get the calculated PSI measurement from the sensor.
        /// </summary>
        public float CalculatedPSIMeasurement { get; set; }

        /// <summary>
        /// Convienence property to get the hPa PSI measurement from the sensor.
        /// </summary>
        public double CalculatedhPAMeasurement { get; set; }
        public float hPaMeasurement { get; set; }

        //Tells us that the sensor has reached its pressure limit.
        public bool InternalMathSaturated { get; set; }

        private int psiMin { get; set; } = 0;
        private int psiMax { get; set; } = 25;

        private AtmosphericConditions oldConditions { get; set; }

        //This value is set by the manufacturer and can't be changed.
        public const byte Address = 0x18;

        public float Pressure => Conditions.Pressure.Value;

        public event EventHandler<AtmosphericConditionChangeResult> Updated;

        protected void RaiseChangedAndNotify(AtmosphericConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        public AdafruitMPRLSSensor(II2cBus i2cbus, int psiMin = 0, int psiMax = 25)
        {
            i2cPeripheral = new I2cPeripheral(i2cbus, Address);
        }

        public async Task<AtmosphericConditions> Read()
        {
            await Update();

            return Conditions;
        }

        public void StartUpdating(int readFrequencyMs = 1000)
        {
            lock (_lock)
            {
                if (this.IsSampling)
                {
                    return;
                }

                IsSampling = true;

                samplingTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = samplingTokenSource.Token;

                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }

                        oldConditions = AtmosphericConditions.From(Conditions);

                        await Update();

                        AtmosphericConditionChangeResult changeResult = new AtmosphericConditionChangeResult(Conditions, oldConditions);

                        RaiseChangedAndNotify(changeResult);

                        await Task.Delay(readFrequencyMs);
                    }
                });
            }
        }

        public void StopUpdating()
        {
            lock (_lock)
            {
                if (!IsSampling) return;

                samplingTokenSource?.Cancel();

                IsSampling = false;
            }
        }

        private async Task Update()
        {
            byte[] readBuffer;
            BitArray bufferBits;

            //Send the command to the sensor to tell it to do the thing.
            i2cPeripheral.WriteBytes(mprlsMeasurementCommand);

            //Datasheet says wait 5 ms.
            await Task.Delay(5);

            while (true)
            {
                readBuffer = i2cPeripheral.ReadBytes(1);
                bufferBits = new BitArray(readBuffer);

                //From section 6.5 of the datasheet.
                this.IsDevicePowered = bufferBits[6];
                this.IsDeviceBusy = bufferBits[5];
                this.HasMemoryIntegrityFailed = bufferBits[2];
                this.InternalMathSaturated = bufferBits[0];

                if (this.InternalMathSaturated)
                {
                    throw new InvalidOperationException("Sensor pressure has exceeded max value!");
                }

                if (this.HasMemoryIntegrityFailed)
                {
                    throw new InvalidOperationException("Sensor internal memory integrity check failed!");
                }

                if (!(this.IsDeviceBusy))
                {
                    break;
                }
            }

            readBuffer = i2cPeripheral.ReadBytes(4);

            RawPSIMeasurement = (readBuffer[1] << 16) | (readBuffer[2] << 8) | readBuffer[3];
            //Console.WriteLine(RawPSIMeasurement);

            //From Section 8.0 of the datasheet.
            CalculatedPSIMeasurement = (RawPSIMeasurement - 1677722) * (psiMax - psiMin);
            //Console.WriteLine(CalculatedPSIMeasurement);

            CalculatedPSIMeasurement /= 15099494 - 1677722;
            //Console.WriteLine(CalculatedPSIMeasurement);

            CalculatedPSIMeasurement += psiMin;
            //Console.WriteLine(CalculatedPSIMeasurement);

            //https://www.justintools.com/unit-conversion/pressure.php?k1=psi&k2=millibars
            CalculatedhPAMeasurement = CalculatedPSIMeasurement * 68.947572932;
            //Console.WriteLine(CalculatedhPAMeasurement);

            Conditions.Pressure = CalculatedPSIMeasurement;
        }
    }
}
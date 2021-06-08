using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Device driver for the Adafruit MPRLS Ported Pressure Sensor Breakout
    /// https://www.adafruit.com/product/3965
    /// Device datasheets also available here: https://sensing.honeywell.com/micropressure-mpr-series
    /// </summary>
    public class AdafruitMPRLS :
        SensorBase<Pressure>,
        IBarometricPressureSensor
    {
        //==== events
        public event EventHandler<IChangeResult<Pressure>> Updated = delegate { };
        public event EventHandler<IChangeResult<Pressure>> PressureUpdated = delegate { };

        //==== internals
        private readonly II2cPeripheral i2cPeripheral;

        //Defined in section 6.6.1 of the datasheet.
        private readonly byte[] mprlsMeasurementCommand = { 0xAA, 0x00, 0x00 };
        private object _lock = new object();
        private CancellationTokenSource samplingTokenSource;

        private int psiMin => 0;
        private int psiMax => 25;

        private Pressure? oldConditions { get; set; }

        //This value is set by the manufacturer and can't be changed.
        public const byte Address = 0x18;

        //==== properties
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
        public Pressure? RawPSIMeasurement { get; set; }

        //Tells us that the sensor has reached its pressure limit.
        public bool InternalMathSaturated { get; set; }

        public Pressure? Pressure { get; set; } = new Pressure(0);


        protected void RaiseChangedAndNotify(IChangeResult<Pressure> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            PressureUpdated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        public AdafruitMPRLS(II2cBus i2cbus, int psiMin = 0, int psiMax = 25)
        {
            i2cPeripheral = new I2cPeripheral(i2cbus, Address);
        }

        public async Task<Pressure> Read()
        {
            await Update();

            return Pressure.GetValueOrDefault();
        }

        public void StartUpdating(int readFrequencyMs = 1000)
        {
            lock (_lock)
            {
                if (IsSampling) { return; }

                IsSampling = true;

                samplingTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = samplingTokenSource.Token;

                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // save state
                        oldConditions = Pressure;
                        // update
                        await Update();

                        var changeResult = new ChangeResult<Pressure>(Pressure.GetValueOrDefault(), oldConditions);

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
                IsDevicePowered = bufferBits[6];
                IsDeviceBusy = bufferBits[5];
                HasMemoryIntegrityFailed = bufferBits[2];
                InternalMathSaturated = bufferBits[0];

                if (InternalMathSaturated)
                {
                    throw new InvalidOperationException("Sensor pressure has exceeded max value!");
                }

                if (HasMemoryIntegrityFailed)
                {
                    throw new InvalidOperationException("Sensor internal memory integrity check failed!");
                }

                if (!(IsDeviceBusy))
                {
                    break;
                }
            }

            readBuffer = i2cPeripheral.ReadBytes(4);

            var rawPSIMeasurement = (readBuffer[1] << 16) | (readBuffer[2] << 8) | readBuffer[3];
            //Console.WriteLine(RawPSIMeasurement);

            //From Section 8.0 of the datasheet.
            var calculatedPSIMeasurement = (rawPSIMeasurement - 1677722) * (psiMax - psiMin);
            //Console.WriteLine(CalculatedPSIMeasurement);

            calculatedPSIMeasurement /= 15099494 - 1677722;
            //Console.WriteLine(CalculatedPSIMeasurement);

            calculatedPSIMeasurement += psiMin;
            //Console.WriteLine(CalculatedPSIMeasurement);

            RawPSIMeasurement = new Pressure(rawPSIMeasurement, Units.Pressure.UnitType.Psi);
            Pressure = new Pressure(calculatedPSIMeasurement, Units.Pressure.UnitType.Psi);
        }
    }
}
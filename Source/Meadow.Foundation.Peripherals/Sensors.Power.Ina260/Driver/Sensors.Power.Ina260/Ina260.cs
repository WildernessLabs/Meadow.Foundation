using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Power
{
    public partial class Ina260
        : FilterableChangeObservableI2CPeripheral<(Units.Power?, Units.Voltage?, Units.Current?)>
    {
        public delegate void ValueChangedHandler(float previousValue, float newValue);


        public event EventHandler<IChangeResult<(Units.Power?, Units.Voltage?, Units.Current?)>> Updated = delegate { };
        public event EventHandler<IChangeResult<Units.Power>> PowerUpdated = delegate { };
        public event EventHandler<IChangeResult<Units.Voltage>> VoltageUpdated = delegate { };
        public event EventHandler<IChangeResult<Units.Current>> CurrentUpdated = delegate { };

        private const float MeasurementScale = 0.00125f;
        private TimeSpan _samplePeriod;

        private Register RegisterPointer { get; set; }
        private object SyncRoot { get; } = new object();
        private CancellationTokenSource SamplingTokenSource { get; set; }

        public bool IsSampling { get; private set; }

        /// <summary>
        /// The value of the current (in Amps) flowing through the shunt resistor from the last reading
        /// </summary>
        public Units.Current? Current => Conditions.Current;

        /// <summary>
        /// The voltage from the last reading..
        /// </summary>
        public Units.Voltage? Voltage => Conditions.Voltage;

        /// <summary>
        /// The power from the last reading..
        /// </summary>
        public Units.Power? Power => Conditions.Power;

        /// <summary>
        /// The last read conditions.
        /// </summary>
        public (Units.Power? Power, Units.Voltage? Voltage, Units.Current? Current) Conditions;

        public Ina260(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        {
            switch (address)
            {
                case 0x40:
                case 0x41:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("INA260 device address must be either 0x40 or 0x41");
            }
        }

        public Ina260(II2cBus i2cBus, Addresses address)
            : this(i2cBus, (byte)address)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                StopSampling();
            }
        }

        public void StartUpdating(TimeSpan samplePeriod)
        {
            lock (SyncRoot)
            {
                // allow subsequent calls to StartSampling to just change the sample period
                _samplePeriod = samplePeriod;

                if (IsSampling)
                {
                    return;
                }

                SamplingTokenSource = new CancellationTokenSource();
                var ct = SamplingTokenSource.Token;

                Task.Factory.StartNew(async () =>
                {
                    IsSampling = true;

                    (Units.Power? Power, Units.Voltage? Voltage, Units.Current? Current) oldConditions;
                    ChangeResult<(Units.Power? Power, Units.Voltage? Voltage, Units.Current? Current)> result;

                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            IsSampling = false;
                            break;
                        }

                        // capture history
                        oldConditions = (Conditions.Power, Conditions.Voltage, Conditions.Current);

                        // read
                        Conditions = await Update();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<(Units.Power?, Units.Voltage?, Units.Current?)>(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        await Task.Delay(_samplePeriod);
                    }

                    IsSampling = false;
                });
            }
        }


        protected async Task<(Units.Power?, Units.Voltage?, Units.Current?)> Update()
        {
            return await Task.Run(() =>
            {
                (Units.Power? Power, Units.Voltage? Voltage, Units.Current? Current) conditions;
                //conditions.Voltage = new Units.Voltage(Bus.ReadRegisterShort((byte)Register.Voltage) * MeasurementScale, Units.Voltage.UnitType.Volts);
                conditions.Voltage = new Units.Voltage(I2cPeripheral.ReadRegister((byte)Register.Voltage) * MeasurementScale, Units.Voltage.UnitType.Volts);
                //conditions.Current = new Units.Current(Bus.ReadRegisterShort((byte)Register.Current) * MeasurementScale, Units.Current.UnitType.Amps);
                conditions.Current = new Units.Current(I2cPeripheral.ReadRegister((byte)Register.Current) * MeasurementScale, Units.Current.UnitType.Amps);
                //conditions.Power = new Units.Power(Bus.ReadRegisterShort((byte)Register.Power) * 0.01f, Units.Power.UnitType.Watts);
                conditions.Power = new Units.Power(I2cPeripheral.ReadRegister((byte)Register.Power) * 0.01f, Units.Power.UnitType.Watts);

                return conditions;
            });
        }

        public void StopSampling()
        {
            lock (SyncRoot)
            {
                if (!IsSampling) return;
                SamplingTokenSource.Cancel();
            }
        }

        protected void RaiseChangedAndNotify(IChangeResult<(Units.Power? Power, Units.Voltage? Voltage, Units.Current? Current)> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            if (changeResult.New.Power is { } power)
            {
                PowerUpdated?.Invoke(this, new ChangeResult<Units.Power>(power, changeResult.Old?.Power));
            }
            if (changeResult.New.Voltage is { } volts)
            {
                VoltageUpdated?.Invoke(this, new ChangeResult<Units.Voltage>(volts, changeResult.Old?.Voltage));
            }
            if (changeResult.New.Current is { } amps)
            {
                CurrentUpdated?.Invoke(this, new ChangeResult<Units.Current>(amps, changeResult.Old?.Current));
            }
            base.NotifyObservers(changeResult);
        }


        /// <summary>
        /// Reads the unique manufacturer identification number
        /// </summary>
        public int ManufacturerID
        {
            //get => Bus.ReadRegisterShort((byte)Register.ManufacturerID);
            get => I2cPeripheral.ReadRegister((byte)Register.ManufacturerID);
        }

        /// <summary>
        /// Reads the unique die identification number
        /// </summary>
        public int DieID
        {
            //get => Bus.ReadRegisterShort((byte)Register.ManufacturerID);
            get => I2cPeripheral.ReadRegister((byte)Register.ManufacturerID);
        }
    }
}

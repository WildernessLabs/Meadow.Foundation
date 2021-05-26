using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Provide access to the CCS811 C02 and VOC Air Quality Sensor
    /// </summary>
    public partial class Ccs811 :
        FilterableChangeObservableI2CPeripheral<(Concentration?, Concentration?)>,
        ICO2Sensor, IVocSensor
    {
        // internal thread lock
        private object _lock = new object();
        private byte[] _readingBuffer = new byte[8];
        private CancellationTokenSource? SamplingTokenSource { get; set; }

        public event EventHandler<IChangeResult<(Concentration?, Concentration?)>> Updated = delegate { };
        public event EventHandler<ChangeResult<Concentration>> CO2Updated = delegate { };
        public event EventHandler<ChangeResult<Concentration>> VOCUpdated = delegate { };

        /// <summary>
        /// Gets a value indicating whether the sensor is currently in a sampling
        /// loop. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// The last read conditions.
        /// </summary>
        public (Concentration CO2, Concentration VOC) Conditions { get; private set; }

        /// <summary>
        /// The measured CO2 concentration
        /// </summary>
        /// 
        public Concentration? CO2 { get => Conditions.CO2; }

        /// <summary>
        /// The measured VOC concentration
        /// </summary>
        public Concentration? VOC { get => Conditions.VOC; }


        public Ccs811(II2cBus i2cBus, byte address)
            : base(i2cBus, address, 10, 8)
        {
            switch (address)
            {
                case 0x5a:
                case 0x5b:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("CCS811 device address must be either 0x5a or 0x5b");
            }

            Init();
        }

        public Ccs811(II2cBus i2cBus, Addresses address = Addresses.Default)
            : this(i2cBus, (byte)address)
        {
        }

        protected void Init()
        {
            // reset
            Reset();

            // wait for the chip to do its thing
            Thread.Sleep(100);

            // read chip ID to make sure it's a CCS
            var id = Bus.ReadRegisterByte((byte)Register.HW_ID);
            if(id != 0x81)
            {
                throw new Exception("Hardware is not identifying as a CCS811");
            }

            // start the firmware app
            Bus.WriteBytes((byte)BootloaderCommand.APP_START);

            // change mode
            SetMeasurementMode(MeasurementMode.ConstantPower1s);
            var mode = Bus.ReadRegisterByte((byte)Register.MEAS_MODE);
        }

        private void ShowDebugInfo()
        {
            var ver = Bus.ReadRegisterByte((byte)Register.HW_VERSION);
            Console.WriteLine($"hardware version A = 0x{ver:x2}");

            var fwb = Bus.ReadRegisterShort((byte)Register.FW_BOOT_VERSION);
            Console.WriteLine($"FWB version = 0x{fwb:x4}");

            var fwa = Bus.ReadRegisterShort((byte)Register.FW_APP_VERSION);
            Console.WriteLine($"FWA version = 0x{fwa:x4}");

            // read status
            var status = Bus.ReadRegisterByte((byte)Register.STATUS);
            Console.WriteLine($"status = 0x{status:x2}");
        }

        public ushort GetBaseline()
        {
            return Bus.ReadRegisterShort((byte)Register.BASELINE);

        }

        public void SetBaseline(ushort value)
        {
            Bus.WriteRegister((byte)Register.BASELINE, value);
        }

        public MeasurementMode GetMeasurementMode()
        {
            return (MeasurementMode)Bus.ReadRegisterByte((byte)Register.MEAS_MODE);
        }

        public void SetMeasurementMode(MeasurementMode mode)
        {
            // TODO: interrupts, etc would be here
            var m = (byte)mode;
            Bus.WriteRegister((byte)Register.MEAS_MODE, m);
        }

        private void Reset()
        {
            Bus.WriteBytes((byte)Register.SW_RESET, 0x11, 0xE5, 0x72, 0x8A);
        }

        public async Task<(Concentration, Concentration)> Read()
        {
            var state = await Update();

            return state;
        }

        public void StartUpdating()
        {
            // thread safety
            lock (_lock)
            {
                if (IsSampling) return;

                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                (Concentration CO2, Concentration VOC) oldConditions;
                ChangeResult<(Concentration?, Concentration?)> result;

                Task.Factory.StartNew(async () => {
                    while (true)
                    {
                        // cleanup
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            IsSampling = false;
                            break;
                        }
                        // capture history
                        oldConditions = (Conditions.CO2, Conditions.VOC);

                        // read
                        Conditions = await Read();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<(Concentration?, Concentration?)>(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(1100);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected async Task<(Concentration, Concentration)> Update()
        {
            return await Task.Run(() =>
            {
                _readingBuffer = new byte[8];

                // data is really in just the first 4, but this gets us status and raw data as well
                Bus.ReadRegisterBytes((byte)Register.ALG_RESULT_DATA, _readingBuffer);

                (Concentration co2, Concentration voc) state;
                state.co2 = new Concentration(_readingBuffer[0] << 8 | _readingBuffer[1], Concentration.UnitType.PartsPerMillion);
                state.voc = new Concentration(_readingBuffer[2] << 8 | _readingBuffer[3], Concentration.UnitType.PartsPerBillion);

                return state;
            });
        }

        protected void RaiseChangedAndNotify(IChangeResult<(Concentration? CO2, Concentration? VOC)> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            if (changeResult.New.CO2 is { } co2)
            {
                CO2Updated?.Invoke(this, new ChangeResult<Concentration>(co2, changeResult.Old?.CO2));
            }
            if (changeResult.New.VOC is { } voc)
            {
                VOCUpdated?.Invoke(this, new ChangeResult<Concentration>(voc, changeResult.Old?.VOC));
            }
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Creates a `FilterableChangeObserver` that has a handler and a filter.
        /// </summary>
        /// <param name="handler">The action that is invoked when the filter is satisifed.</param>
        /// <param name="filter">An optional filter that determines whether or not the
        /// consumer should be notified.</param>
        /// <returns></returns>
        /// <returns></returns>
        // Implementor Notes:
        //  This is a convenience method that provides named tuple elements. It's not strictly
        //  necessary, as the `FilterableChangeObservableBase` class provides a default implementation,
        //  but if you use it, then the parameters are named `Item1`, `Item2`, etc. instead of
        //  `Concentration`, etc.
        public static new
            FilterableChangeObserver<(Units.Concentration?, Concentration?)>
            CreateObserver(
                Action<IChangeResult<(Units.Concentration? Temperature, Concentration? Humidity)>> handler,
                Predicate<IChangeResult<(Units.Concentration? Temperature, Concentration? Humidity)>>? filter = null
            )
        {
            return new FilterableChangeObserver<(Units.Concentration?, Concentration?)>(
                handler: handler, filter: filter
                );
        }
    }
}
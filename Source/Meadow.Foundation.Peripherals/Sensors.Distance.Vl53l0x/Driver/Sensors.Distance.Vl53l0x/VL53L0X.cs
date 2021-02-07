using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Distance;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Distance
{
    /// <summary>
    /// Represents the Vl53l0x distance sensor
    /// </summary>
    public class Vl53l0x : FilterableChangeObservableBase<DistanceConditionChangeResult, DistanceConditions>, IRangeFinder
    {
        protected const byte RangeStart = 0x00;
        protected const byte SystemThreahHigh = 0x0C;
        protected const byte SystemThreshLow = 0x0E;
        protected const byte SystemSequenceConfig = 0x01;
        protected const byte SystemRangeConfig = 0x09;
        protected const byte SystemIntermeasurementPeriod = 0x04;
        protected const byte SystemInterruptConfigGpio = 0x0A;
        protected const byte GpioHvMuxActiveHigh = 0x84;
        protected const byte SystemInterruptClear = 0x0B;
        protected const byte ResultInterruptStatus = 0x13;
        protected const byte ResultRangeStatus = 0x14;
        protected const byte ResultCoreAmbientWindowEventsRtn = 0xBC;
        protected const byte ResultCoreRangingTotalEventsRtn = 0xC0;
        protected const byte ResultCoreAmbientWindowEventsRef = 0xD0;
        protected const byte ResultCoreRangingTotalEventsRef = 0xD4;
        protected const byte ResultPeakSignalRateRef = 0xB6;
        protected const byte AlgoPartToPartRangeOffsetMm = 0x28;
        protected const byte I2CSlaveDeviceAddress = 0x8A;
        protected const byte MsrcConfigControl = 0x60;
        protected const byte PreRangeConfigMinSnr = 0x27;
        protected const byte PreRangeConfigValidPhaseLow = 0x56;
        protected const byte PreRangeConfigValidPhaseHigh = 0x57;
        protected const byte PreRangeMinCountRateRtnLimit = 0x64;
        protected const byte FinalRangeConfigMinSnr = 0x67;
        protected const byte FinalRangeConfigValidPhaseLow = 0x47;
        protected const byte FinalRangeConfigValidPhaseHigh = 0x48;
        protected const byte FinalRangeConfigMinCountRateRtnLimit = 0x44;
        protected const byte PreRangeConfigSigmaThreshHi = 0x61;
        protected const byte PreRangeConfigSigmaThreshLo = 0x62;
        protected const byte PreRangeConfigVcselPeriod = 0x50;
        protected const byte PreRangeConfigTimeoutMacropHi = 0x51;
        protected const byte PreRangeConfigTimeoutMacropLo = 0x52;
        protected const byte SystemHistogramBin = 0x81;
        protected const byte HistogramConfigInitialPhaseSelect = 0x33;
        protected const byte HistogramConfigReadoutCtrl = 0x55;
        protected const byte FinalRangeConfigVcselPeriod = 0x70;
        protected const byte FinalRangeConfigTimeoutMacropHi = 0x71;
        protected const byte FinalRangeConfigTimeoutMacropLo = 0x72;
        protected const byte CrosstalkCompensationPeakRateMcps = 0x20;
        protected const byte MsrcConfigTimeoutMacrop = 0x46;
        protected const byte SoftResetGo2SoftResetN = 0xBF;
        protected const byte IdentificationModelId = 0xC0;
        protected const byte IdentificationRevisionId = 0xC2;
        protected const byte OscCalibrateVal = 0xF8;
        protected const byte GlobalConfigVcselWidth = 0x32;
        protected const byte GlobalConfigSpadEnablesRef0 = 0xB0;
        protected const byte GlobalConfigSpadEnablesRef1 = 0xB1;
        protected const byte GlobalConfigSpadEnablesRef2 = 0xB2;
        protected const byte GlobalConfigSpadEnablesRef3 = 0xB3;
        protected const byte GlobalConfigSpadEnablesRef4 = 0xB4;
        protected const byte GlobalConfigSpadEnablesRef5 = 0xB5;
        protected const byte GlobalConfigRefEnStartSelect = 0xB6;
        protected const byte DynamicSpadNumRequestedRefSpad = 0x4E;
        protected const byte DynamicSpadRefEnStartOffset = 0x4F;
        protected const byte PowerManagementGo1PowerForce = 0x80;
        protected const byte VhvConfigPadSclSdaExtsupHv = 0x89;
        protected const byte AlgoPhasecalLim = 0x30;
        protected const byte AlgoPhasecalConfigTimeout = 0x30;
        protected const int VcselPeriodPreRange = 0;
        protected const int VcselPeriodFinalRange = 1;

        

        public enum UnitType
        {
            mm,
            cm,
            inches
        }

        public const byte DefaultI2cAddress = 0x29;

        public UnitType Units { get; set; }
        public bool IsShutdown
        {
            get
            {
                if (shutdownPort != null)
                {
                    return !shutdownPort.State;
                }
                else
                {
                    return false;
                }
            }
        }

        public float CurrentDistance => Conditions.Distance.Value;

        /// <summary>
        /// Minimum valid distance in mm.
        /// </summary>
        public float MinimumDistance => 30;

        /// <summary>
        /// Maximum valid distance in mm (CurrentDistance returns -1 if above).
        /// </summary>
        public float MaximumDistance => 2000;

        public DistanceConditions Conditions { get; protected set; } = new DistanceConditions();

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        /// <summary>
        /// Is the sensor sampling 
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        readonly II2cPeripheral i2cPeripheral;
        readonly IDigitalOutputPort shutdownPort;
       
        byte stopVariable;

        public event EventHandler<DistanceConditionChangeResult> Updated;

        public Vl53l0x(IIODevice device, II2cBus i2cBus, byte address = DefaultI2cAddress, UnitType units = UnitType.mm) : 
            this (device, i2cBus, null, address, units)
        {
        }

        /// <param name="i2cBus">I2C bus</param>
        /// <param name="address">VL53L0X address</param>
        /// <param name="units">Unit of measure</param>
        public Vl53l0x(IIODevice device, II2cBus i2cBus, IPin shutdownPin, byte address = DefaultI2cAddress,  UnitType units = UnitType.mm)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            if(shutdownPin != null)
            {
                device.CreateDigitalOutputPort(shutdownPin, true);
            }
            Units = units;

            Initialize();
        }

        ///// <summary>
        ///// Convenience method to get the current distance. For frequent reads, use
        ///// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        ///// </summary>
        public async Task<DistanceConditions> Read()
        {
            await Update();

            return Conditions;
        }

        ///// <summary>
        ///// Starts continuously sampling the sensor.
        /////
        ///// This method also starts raising `Changed` events and IObservable
        ///// subscribers getting notified.
        ///// </summary>
        public void StartUpdating(int standbyDuration = 1000)
        {
            // thread safety
            lock (_lock)
            {
                if (IsSampling) { return; }

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                DistanceConditions oldConditions;
                DistanceConditionChangeResult result;
                Task.Factory.StartNew(async () => {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = DistanceConditions.From(Conditions);

                        // read
                        await Update();

                        // build a new result with the old and new conditions
                        result = new DistanceConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(DistanceConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        ///// <summary>
        ///// Stops sampling the temperature.
        ///// </summary>
        public void StopUpdating()
        {
            lock (_lock)
            {
                if (!IsSampling) { return; }

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }

        /// <summary>
        ///     Read the sensor output and convert the sensor readings into acceleration values.
        /// </summary>
        public async Task Update()
        {
            Conditions.Distance = await GetRange();
        }

        /// <summary>
        /// Initializes the VL53L0X
        /// </summary>
        protected async Task Initialize()
        {
            if (IsShutdown)
            {
                await ShutDown(false);
            }

            if (Read(0xC0) != 0xEE || Read(0xC1) != 0xAA || Read(0xC2) != 0x10)
            {
                throw new Exception("Failed to find expected ID register values");
            }

            i2cPeripheral.WriteRegister(0x88, 0x00);
            i2cPeripheral.WriteRegister(0x80, 0x01);
            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(0x00, 0x00);

            stopVariable = Read(0x91);

            i2cPeripheral.WriteRegister(0x00, 0x01);
            i2cPeripheral.WriteRegister(0xFF, 0x00);
            i2cPeripheral.WriteRegister(0x80, 0x00);

            var configControl = ((byte)(Read(MsrcConfigControl) | 0x12));
            var signalRateLimit = 0.25f;

            i2cPeripheral.WriteRegister(SystemSequenceConfig, 0xFF);
            var spadInfo = GetSpadInfo();
            int spadCount = spadInfo.Item1;
            bool spad_is_aperture = spadInfo.Item2;

            byte[] ref_spad_map = new byte[7];
            ref_spad_map[0] = GlobalConfigSpadEnablesRef0;

            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(DynamicSpadRefEnStartOffset, 0x00);
            i2cPeripheral.WriteRegister(DynamicSpadNumRequestedRefSpad, 0x2C);
            i2cPeripheral.WriteRegister(0xFF, 0x00);
            i2cPeripheral.WriteRegister(GlobalConfigRefEnStartSelect, 0xB4);

            var first_spad_to_enable = (spad_is_aperture) ? 12 : 0;
            var spads_enabled = 0;
            
            for (int i = 0; i < 48; i++)
            {
                if (i < first_spad_to_enable || spads_enabled == spadCount)
                {
                    ref_spad_map[1 + (i / 8)] &= (byte)~(1 << (i % 8));
                }
                else if ((ref_spad_map[1 + (i / 8)] >> (byte)((i % 8)) & 0x1) > 0)
                {
                    spads_enabled += 1;
                }
            }

            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(0x00, 0x00);
            i2cPeripheral.WriteRegister(0xFF, 0x00);
            i2cPeripheral.WriteRegister(0x09, 0x00);
            i2cPeripheral.WriteRegister(0x10, 0x00);
            i2cPeripheral.WriteRegister(0x11, 0x00);
            i2cPeripheral.WriteRegister(0x24, 0x01);
            i2cPeripheral.WriteRegister(0x25, 0xFF);
            i2cPeripheral.WriteRegister(0x75, 0x00);
            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(0x4E, 0x2C);
            i2cPeripheral.WriteRegister(0x48, 0x00);
            i2cPeripheral.WriteRegister(0x30, 0x20);
            i2cPeripheral.WriteRegister(0xFF, 0x00);
            i2cPeripheral.WriteRegister(0x30, 0x09);
            i2cPeripheral.WriteRegister(0x54, 0x00);
            i2cPeripheral.WriteRegister(0x31, 0x04);
            i2cPeripheral.WriteRegister(0x32, 0x03);
            i2cPeripheral.WriteRegister(0x40, 0x83);
            i2cPeripheral.WriteRegister(0x46, 0x25);
            i2cPeripheral.WriteRegister(0x60, 0x00);
            i2cPeripheral.WriteRegister(0x27, 0x00);
            i2cPeripheral.WriteRegister(0x50, 0x06);
            i2cPeripheral.WriteRegister(0x51, 0x00);
            i2cPeripheral.WriteRegister(0x52, 0x96);
            i2cPeripheral.WriteRegister(0x56, 0x08);
            i2cPeripheral.WriteRegister(0x57, 0x30);
            i2cPeripheral.WriteRegister(0x61, 0x00);
            i2cPeripheral.WriteRegister(0x62, 0x00);
            i2cPeripheral.WriteRegister(0x64, 0x00);
            i2cPeripheral.WriteRegister(0x65, 0x00);
            i2cPeripheral.WriteRegister(0x66, 0xA0);
            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(0x22, 0x32);
            i2cPeripheral.WriteRegister(0x47, 0x14);
            i2cPeripheral.WriteRegister(0x49, 0xFF);
            i2cPeripheral.WriteRegister(0x4A, 0x00);
            i2cPeripheral.WriteRegister(0xFF, 0x00);
            i2cPeripheral.WriteRegister(0x7A, 0x0A);
            i2cPeripheral.WriteRegister(0x7B, 0x00);
            i2cPeripheral.WriteRegister(0x78, 0x21);
            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(0x23, 0x34);
            i2cPeripheral.WriteRegister(0x42, 0x00);
            i2cPeripheral.WriteRegister(0x44, 0xFF);
            i2cPeripheral.WriteRegister(0x45, 0x26);
            i2cPeripheral.WriteRegister(0x46, 0x05);
            i2cPeripheral.WriteRegister(0x40, 0x40);
            i2cPeripheral.WriteRegister(0x0E, 0x06);
            i2cPeripheral.WriteRegister(0x20, 0x1A);
            i2cPeripheral.WriteRegister(0x43, 0x40);
            i2cPeripheral.WriteRegister(0xFF, 0x00);
            i2cPeripheral.WriteRegister(0x34, 0x03);
            i2cPeripheral.WriteRegister(0x35, 0x44);
            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(0x31, 0x04);
            i2cPeripheral.WriteRegister(0x4B, 0x09);
            i2cPeripheral.WriteRegister(0x4C, 0x05);
            i2cPeripheral.WriteRegister(0x4D, 0x04);
            i2cPeripheral.WriteRegister(0xFF, 0x00);
            i2cPeripheral.WriteRegister(0x44, 0x00);
            i2cPeripheral.WriteRegister(0x45, 0x20);
            i2cPeripheral.WriteRegister(0x47, 0x08);
            i2cPeripheral.WriteRegister(0x48, 0x28);
            i2cPeripheral.WriteRegister(0x67, 0x00);
            i2cPeripheral.WriteRegister(0x70, 0x04);
            i2cPeripheral.WriteRegister(0x71, 0x01);
            i2cPeripheral.WriteRegister(0x72, 0xFE);
            i2cPeripheral.WriteRegister(0x76, 0x00);
            i2cPeripheral.WriteRegister(0x77, 0x00);
            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(0x0D, 0x01);
            i2cPeripheral.WriteRegister(0xFF, 0x00);
            i2cPeripheral.WriteRegister(0x80, 0x01);
            i2cPeripheral.WriteRegister(0x01, 0xF8);
            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(0x8E, 0x01);
            i2cPeripheral.WriteRegister(0x00, 0x01);
            i2cPeripheral.WriteRegister(0xFF, 0x00);
            i2cPeripheral.WriteRegister(0x80, 0x00);

            i2cPeripheral.WriteRegister(SystemInterruptConfigGpio, 0x04);
            var gpio_hv_mux_active_high = Read(GpioHvMuxActiveHigh);
            i2cPeripheral.WriteRegister(GpioHvMuxActiveHigh, (byte)(gpio_hv_mux_active_high & ~0x10));

            i2cPeripheral.WriteRegister(GpioHvMuxActiveHigh, 0x01);
            i2cPeripheral.WriteRegister(SystemSequenceConfig, 0xE8);

            i2cPeripheral.WriteRegister(SystemSequenceConfig, 0x01);
            PerformSingleRefCalibration(0x40);
            i2cPeripheral.WriteRegister(SystemSequenceConfig, 0x02);
            PerformSingleRefCalibration(0x00);

            i2cPeripheral.WriteRegister(SystemSequenceConfig, 0xE8);
        }

        /// <summary>
        /// Returns the current distance/range
        /// </summary>
        /// <returns>The distance in the specified Units. Default mm. Returns -1 if the shutdown pin is used and is off</returns>
        protected async Task<float> GetRange()
        {
            if (IsShutdown)
            {
                return -1f;
            }

            float dist = await GetRawRangeData();

            if (dist > MaximumDistance)
            {
                dist = -1;
            }
            else if (Units == UnitType.inches)
            {
                dist = dist * 0.0393701f;
            }
            else if (Units == UnitType.cm)
            {
                dist = dist / 10;
            }

            return dist;
        }

        /// <summary>
        /// Set a new I2C address
        /// </summary>
        /// <param name="newAddress"></param>
        public void SetAddress(byte newAddress)
        {
            if (IsShutdown)
                return;

            byte address = (byte)(newAddress & 0x7F);
            i2cPeripheral.WriteRegister(I2CSlaveDeviceAddress, address);
        }

        /// <summary>
        /// Set the Shutdown state of the device
        /// </summary>
        /// <param name="state">true = off/shutdown. false = on</param>
        public async Task ShutDown(bool state)
        {
            if (shutdownPort == null)
            {
                return;
            }

            shutdownPort.State = !state;
            await Task.Delay(2).ConfigureAwait(false);

            if (state == false)
            {
                Initialize();
                await Task.Delay(2).ConfigureAwait(false);
            }
        }

        protected Tuple<int, bool> GetSpadInfo()
        {
            i2cPeripheral.WriteRegister(0x80, 0x01);
            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(0x00, 0x00);
            i2cPeripheral.WriteRegister(0xFF, 0x06);

            var result = (byte)(Read(0x83) | 0x04);
            i2cPeripheral.WriteRegister(0x83, result);

            i2cPeripheral.WriteRegister(0xFF, 0x07);
            i2cPeripheral.WriteRegister(0x81, 0x01);
            i2cPeripheral.WriteRegister(0x94, 0x6B);

            i2cPeripheral.WriteRegister(0x83, 0x00);

            int tCount = 0;
            while (Read(0x83) == 0x00)
            {
                Thread.Sleep(5);
                tCount++;
                if (tCount > 100)
                {
                    throw new Exception("Timeout");
                }
            }

            i2cPeripheral.WriteRegister(0x83, 0x01);
            var tmp = Read(0x92);
            var count = tmp & 0x7F;
            var is_aperture = ((tmp >> 7) & 0x01) == 1;

            i2cPeripheral.WriteRegister(0x81, 0x00);
            i2cPeripheral.WriteRegister(0xFF, 0x06);

            var t = (byte)(Read(0x83) & ~0x04);
            i2cPeripheral.WriteRegister(0xFF, t);

            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(0x00, 0x01);
            i2cPeripheral.WriteRegister(0xFF, 0x00);
            i2cPeripheral.WriteRegister(0x80, 0x00);

            return new Tuple<int, bool>(count, is_aperture);
        }

        protected void PerformSingleRefCalibration(byte vhvInitByte)
        {
            i2cPeripheral.WriteRegister(RangeStart, (byte)(0x01 | vhvInitByte & 0xFF));

            int tCount = 0;

            while ((byte)(Read(ResultInterruptStatus) & 0x07) == 0)
            {
                Thread.Sleep(5);
                tCount++;
                if (tCount > 100)
                {
                    throw new Exception("Interrupt Status Timeout");
                }
            }

            i2cPeripheral.WriteRegister(GpioHvMuxActiveHigh, 0x01);
            i2cPeripheral.WriteRegister(RangeStart, 0x00);
        }

        protected byte Read(byte address)
        {
            var result = i2cPeripheral.ReadRegister(address);
            return result;
        }

        protected int Read16(byte address)
        {
            var result = i2cPeripheral.ReadRegisters(address, 2);

            return (result[0] << 8) | result[1];
        }

        protected async Task<int> GetRawRangeData()
        {
            i2cPeripheral.WriteRegister(0x80, 0x01);
            i2cPeripheral.WriteRegister(0xFF, 0x01);
            i2cPeripheral.WriteRegister(0x00, 0x00);
            i2cPeripheral.WriteRegister(0x91, stopVariable);
            i2cPeripheral.WriteRegister(0x00, 0x01);
            i2cPeripheral.WriteRegister(0xFF, 0x00);
            i2cPeripheral.WriteRegister(0x80, 0x00);
            i2cPeripheral.WriteRegister(RangeStart, 0x01);

            int tCount = 0;
            while ((byte)(Read(RangeStart) & 0x01) > 0)
            {
                await Task.Delay(5).ConfigureAwait(false);

                tCount++;
                if (tCount > 100)
                {
                    throw new Exception("VL53L0X Range Start Timeout");
                }
            }

            tCount = 0;
            while ((byte)(Read(ResultInterruptStatus) & 0x07) == 0)
            {
                await Task.Delay(5).ConfigureAwait(false);

                tCount++;
                if (tCount > 100)
                {
                    throw new Exception("VL53L0X Interrupt Status Timeout");
                }
            }

            var range_mm = Read16(ResultRangeStatus + 10);
            i2cPeripheral.WriteRegister(GpioHvMuxActiveHigh, 0x01);

            return range_mm;
        }

        public IDisposable Subscribe(IObserver<DistanceConditionChangeResult> observer)
        {
            throw new NotImplementedException();
        }
    }
}
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// Represents a BH1745 Luminance and Color Sensor
    /// </summary>
    public partial class Bh1745
        : ByteCommsSensorBase<(Illuminance? AmbientLight, Color? Color, bool Valid)>,
        ILightSensor, II2cPeripheral
    {
        private event EventHandler<IChangeResult<Illuminance>> _lightHandlers;

        event EventHandler<IChangeResult<Illuminance>> ISamplingSensor<Illuminance>.Updated
        {
            add => _lightHandlers += value;
            remove => _lightHandlers -= value;
        }

        /// <summary>
        /// The current Illuminance value
        /// </summary>
        public Illuminance? Illuminance => Conditions.AmbientLight;

        /// <summary>
        /// Interrupt reset status
        /// </summary>
        public InterruptStatus InterruptReset
        {
            get
            {
                var intReset = BusComms.ReadRegister(Registers.SYSTEM_CONTROL);
                intReset = (byte)((intReset & MaskValues.INT_RESET) >> 6);
                return (InterruptStatus)intReset;
            }
            set
            {
                if (!Enum.IsDefined(typeof(InterruptStatus), value))
                {
                    throw new ArgumentOutOfRangeException();
                }
                var intReset = BusComms.ReadRegister(Registers.SYSTEM_CONTROL);
                intReset = (byte)((intReset & ~MaskValues.INT_RESET) | (byte)value << 6);
                BusComms?.WriteRegister(Registers.SYSTEM_CONTROL, intReset);
            }
        }

        /// <summary>
        /// Gets or sets the currently set measurement time
        /// </summary>
        public MeasurementTimeType MeasurementTime
        {
            get => measurementTime;
            set
            {
                if (!Enum.IsDefined(typeof(MeasurementTimeType), value))
                {
                    throw new ArgumentOutOfRangeException();
                }
                var time = BusComms.ReadRegister(Registers.MODE_CONTROL1);
                time = (byte)((time & ~MaskValues.MEASUREMENT_TIME) | (byte)value);
                BusComms?.WriteRegister(Registers.MODE_CONTROL1, time);
                measurementTime = value;
            }
        }

        private MeasurementTimeType measurementTime;

        /// <summary>
        /// Is the sensor actively measuring
        /// </summary>
        public bool IsMeasurementActive
        {
            get => isMeasurementActive;
            set
            {
                var active = BusComms.ReadRegister(Registers.MODE_CONTROL2);
                active = (byte)((active & ~MaskValues.RGBC_EN) | Convert.ToByte(value) << 4);
                BusComms?.WriteRegister(Registers.MODE_CONTROL2, active);
                isMeasurementActive = value;
            }
        }

        private bool isMeasurementActive;

        /// <summary>
        /// Gets or sets the ADC gain of the sensor
        /// </summary>
        public AdcGainTypes AdcGain
        {
            get => adcGain;
            set
            {
                if (!Enum.IsDefined(typeof(AdcGainTypes), value))
                {
                    throw new ArgumentOutOfRangeException();
                }
                var adcGain = BusComms.ReadRegister(Registers.MODE_CONTROL2);
                adcGain = (byte)((adcGain & ~MaskValues.ADC_GAIN) | (byte)value);
                BusComms?.WriteRegister(Registers.MODE_CONTROL2, adcGain);
                this.adcGain = value;
            }
        }

        private AdcGainTypes adcGain;

        /// <summary>
        /// Is the interrupt active
        /// </summary>
        public bool InterruptSignalIsActive
        {
            get
            {
                var intStatus = BusComms.ReadRegister(Registers.INTERRUPT);
                intStatus = (byte)((intStatus & MaskValues.INT_STATUS) >> 7);
                return Convert.ToBoolean(intStatus);
            }
        }

        /// <summary>
        /// Gets or sets how the interrupt pin latches
        /// </summary>
        public LatchBehaviorTypes LatchBehavior
        {
            get => latchBehavior;
            set
            {
                if (!Enum.IsDefined(typeof(LatchBehaviorTypes), value))
                {
                    throw new ArgumentOutOfRangeException();
                }
                var intLatch = BusComms.ReadRegister(Registers.INTERRUPT);
                intLatch = (byte)((intLatch & ~MaskValues.INT_LATCH) | (byte)value << 4);
                BusComms?.WriteRegister(Registers.INTERRUPT, intLatch);
                latchBehavior = value;
            }
        }

        private LatchBehaviorTypes latchBehavior;

        /// <summary>
        /// Gets or sets the source channel that triggers the interrupt
        /// </summary>
        public InterruptChannels InterruptSource
        {
            get => interruptSource;
            set
            {
                if (!Enum.IsDefined(typeof(InterruptChannels), value)) { throw new ArgumentOutOfRangeException(); }
                var intSource = BusComms.ReadRegister(Registers.INTERRUPT);
                intSource = (byte)((intSource & ~MaskValues.INT_SOURCE) | (byte)value << 2);
                BusComms?.WriteRegister(Registers.INTERRUPT, intSource);
                interruptSource = value;
            }
        }

        private InterruptChannels interruptSource;

        /// <summary>
        /// Gets or sets whether the interrupt pin is enabled
        /// </summary>
        public bool InterruptIsEnabled
        {
            get => isInterruptEnabled;
            set
            {
                var intPin = BusComms.ReadRegister(Registers.INTERRUPT);
                intPin = (byte)((intPin & ~MaskValues.INT_ENABLE) | Convert.ToByte(value));
                BusComms?.WriteRegister(Registers.INTERRUPT, intPin);
                isInterruptEnabled = value;
            }
        }

        private bool isInterruptEnabled;

        /// <summary>
        /// Gets or sets the persistence function of the interrupt
        /// </summary>
        public InterruptTypes InterruptPersistence
        {
            get => interruptPersistence;
            set
            {
                if (!Enum.IsDefined(typeof(InterruptTypes), value))
                {
                    throw new ArgumentOutOfRangeException();
                }
                var intPersistence = BusComms.ReadRegister(Registers.PERSISTENCE);
                intPersistence = (byte)((intPersistence & ~MaskValues.PERSISTENCE_MASK) | (byte)value);
                BusComms?.WriteRegister(Registers.PERSISTENCE, intPersistence);
                interruptPersistence = value;
            }
        }

        private InterruptTypes interruptPersistence;

        /// <summary>
        /// Gets or sets the lower interrupt threshold
        /// </summary>
        public ushort LowerInterruptThreshold
        {
            get => lowerInterruptThreshold;
            set
            {
                BusComms.WriteRegister(Registers.TL, value);
                lowerInterruptThreshold = value;
            }
        }

        private ushort lowerInterruptThreshold;

        /// <summary>
        /// Gets or sets the upper interrupt threshold
        /// </summary>
        public ushort UpperInterruptThreshold
        {
            get => upperInterruptThreshold;
            set
            {
                BusComms.WriteRegister(Registers.TH, value);
                upperInterruptThreshold = value;
            }
        }

        private ushort upperInterruptThreshold;

        /// <summary>
        /// Gets or sets the channel compensation multipliers which are used to scale the channel measurements
        /// </summary>
        public ChannelMultipliers CompensationMultipliers { get; set; }

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Create a new BH17545 color sensor object
        /// </summary>
        public Bh1745(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        {
            CompensationMultipliers = new ChannelMultipliers
            {
                Red = 1.0, //2.2,
                Green = 1.0,
                Blue = 1.0, //1.8,
                Clear = 10.0
            };

            Reset();
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(Illuminance? AmbientLight, Color? Color, bool Valid)> ReadSensor()
        {
            (Illuminance? AmbientLight, Color? Color, bool Valid) conditions;

            // get the ambient light
            var clearData = ReadClearDataRegister();

            if (clearData == 0) { conditions.Color = Color.Black; }

            // apply channel multipliers and normalize
            double compensatedRed = ReadRedDataRegister() * CompensationMultipliers.Red / (int)MeasurementTime * 360;
            double compensatedGreen = ReadGreenDataRegister() * CompensationMultipliers.Green / (int)MeasurementTime * 360;
            double compensatedBlue = ReadBlueDataRegister() * CompensationMultipliers.Blue / (int)MeasurementTime * 360;
            double compensatedClear = clearData * CompensationMultipliers.Clear / (int)MeasurementTime * 360;

            // scale relative to clear
            int red = (int)Math.Min(255, compensatedRed / compensatedClear * 255);
            int green = (int)Math.Min(255, compensatedGreen / compensatedClear * 255);
            int blue = (int)Math.Min(255, compensatedBlue / compensatedClear * 255);

            conditions.Color = Color.FromRgb(red, green, blue);

            conditions.AmbientLight = new Illuminance(compensatedClear, Units.Illuminance.UnitType.Lux);

            conditions.Valid = ReadMeasurementIsValid();

            return Task.FromResult(conditions);
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Illuminance? AmbientLight, Color? Color, bool Valid)> changeResult)
        {
            if (changeResult.New.AmbientLight is { } ambient)
            {
                _lightHandlers?.Invoke(this, new ChangeResult<Illuminance>(ambient, changeResult.Old?.AmbientLight));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Resets the device to the default configuration
        /// On reset the sensor goes to power down mode
        /// </summary>
        protected void Reset()
        {
            Resolver.Log.Info("Reset");

            var status = BusComms.ReadRegister(Registers.SYSTEM_CONTROL);
            status = (byte)((status & ~MaskValues.SW_RESET) | 0x01 << 7);

            BusComms.WriteRegister(Registers.SYSTEM_CONTROL, status);

            // set default measurement configuration
            MeasurementTime = MeasurementTimeType.Ms160;
            AdcGain = AdcGainTypes.X1;
            IsMeasurementActive = true;
            InterruptIsEnabled = true;

            // set fields to reset state
            interruptPersistence = InterruptTypes.UpdateMeasurementEnd;
            latchBehavior = LatchBehaviorTypes.LatchUntilReadOrInitialized;
            interruptSource = InterruptChannels.Blue;
            isInterruptEnabled = false;
            lowerInterruptThreshold = 0x0000;
            upperInterruptThreshold = 0xFFFF;

            // write default value to Mode_Control3
            BusComms.WriteRegister(Registers.MODE_CONTROL3, 0x02);
        }

        /// <summary>
        /// Reads whether the last measurement is valid
        /// </summary>
        protected bool ReadMeasurementIsValid()
        {
            var valid = BusComms.ReadRegister(Registers.MODE_CONTROL2);
            valid = (byte)(valid & MaskValues.VALID);
            return Convert.ToBoolean(valid);
        }

        /// <summary>
        /// Reads the red data register of the sensor
        /// </summary>
        /// <returns></returns>
        protected ushort ReadRedDataRegister() => BusComms.ReadRegisterAsUShort(Registers.RED_DATA);

        /// <summary>
        /// Reads the green data register of the sensor
        /// </summary>
        /// <returns></returns>
        protected ushort ReadGreenDataRegister() => BusComms.ReadRegisterAsUShort(Registers.GREEN_DATA);

        /// <summary>
        /// Reads the blue data register of the sensor
        /// </summary>
        /// <returns></returns>
        protected ushort ReadBlueDataRegister() => BusComms.ReadRegisterAsUShort(Registers.BLUE_DATA);

        /// <summary>
        /// Reads the clear data register of the sensor
        /// </summary>
        /// <returns></returns>
        protected ushort ReadClearDataRegister() => BusComms.ReadRegisterAsUShort(Registers.CLEAR_DATA);

        async Task<Illuminance> ISensor<Illuminance>.Read()
            => (await Read()).AmbientLight!.Value;
    }
}
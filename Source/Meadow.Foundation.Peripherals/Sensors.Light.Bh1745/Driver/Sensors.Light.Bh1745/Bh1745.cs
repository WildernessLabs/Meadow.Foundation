using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Light
{
    // TODO: The chip can drive LEDs which will help to identify colors more
    // accurately by lighting them up. I found this documentation from the
    // pimoroni site (https://shop.pimoroni.com/products/bh1745-luminance-and-colour-sensor-breakout)
    // which makes a breakout that has LEDs:
    //
    // The LEDs are connected to the BH1745 Interrupt line. This is activated by
    // a threshold mechanism that is fully documented in the BH1745 chip manual.
    // You can select which of the four light sensor channels to use and you can
    // set a high and a low threshold. The Interrupt is enabled when the light
    // is above the high level or below the low level. Write 0x1D to register
    // 0x60 to enable Interrupts and select the unfiltered light sensor. Write
    // 0xFF to the four registers starting at 0x62 to force the LEDs on. With
    // the default settings in these registers, the LEDS will be off.

    public partial class Bh1745
        : ByteCommsSensorBase<(Illuminance? AmbientLight, Color? Color, bool Valid)>, ILightSensor
    {
        //==== events
        public event EventHandler<IChangeResult<Illuminance>> LuminosityUpdated = delegate { };

        //==== properties
        /// <summary>
        /// 
        /// </summary>
        public Illuminance? Illuminance => Conditions.AmbientLight;

        public InterruptStatus InterruptReset {
            get {
                var intReset = Peripheral.ReadRegister(Registers.SYSTEM_CONTROL);
                intReset = (byte)((intReset & MaskValues.INT_RESET) >> 6);
                return (InterruptStatus)intReset;
            }
            set {
                if (!Enum.IsDefined(typeof(InterruptStatus), value)) {
                    throw new ArgumentOutOfRangeException();
                }
                var intReset = Peripheral.ReadRegister(Registers.SYSTEM_CONTROL);
                intReset = (byte)((intReset & ~MaskValues.INT_RESET) | (byte)value << 6);
                Peripheral?.WriteRegister(Registers.SYSTEM_CONTROL, intReset);
            }
        }

        /// <summary>
        /// Gets or sets the currently set measurement time.
        /// </summary>
        public MeasurementTimeType MeasurementTime {
            get => measurementTime;
            set {
                if (!Enum.IsDefined(typeof(MeasurementTimeType), value)) {
                    throw new ArgumentOutOfRangeException();
                }
                var time = Peripheral.ReadRegister(Registers.MODE_CONTROL1);
                time = (byte)((time & ~MaskValues.MEASUREMENT_TIME) | (byte)value);
                Peripheral?.WriteRegister(Registers.MODE_CONTROL1, time);
                measurementTime = value;
            }
        } protected MeasurementTimeType measurementTime;

        /// <summary>
        /// Is the sensor actively measuring
        /// </summary>
        public bool IsMeasurementActive {
            get => isMeasurementActive;
            set {
                var active = Peripheral.ReadRegister(Registers.MODE_CONTROL2);
                active = (byte)((active & ~MaskValues.RGBC_EN) | Convert.ToByte(value) << 4);
                Peripheral?.WriteRegister(Registers.MODE_CONTROL2, active);
                isMeasurementActive = value;
            }
        } protected bool isMeasurementActive;

        /// <summary>
        /// Gets or sets the ADC gain of the sensor
        /// </summary>
        public AdcGainTypes AdcGain {
            get => adcGain;
            set {
                if (!Enum.IsDefined(typeof(AdcGainTypes), value)) {
                    throw new ArgumentOutOfRangeException();
                }
                var adcGain = Peripheral.ReadRegister(Registers.MODE_CONTROL2);
                adcGain = (byte)((adcGain & ~MaskValues.ADC_GAIN) | (byte)value);
                Peripheral?.WriteRegister(Registers.MODE_CONTROL2, adcGain);
                this.adcGain = value;
            }
        } protected AdcGainTypes adcGain;

        /// <summary>
        /// Is the interrupt active
        /// </summary>
        public bool InterruptSignalIsActive {
            get {
                var intStatus = Peripheral.ReadRegister(Registers.INTERRUPT);
                intStatus = (byte)((intStatus & MaskValues.INT_STATUS) >> 7);
                return Convert.ToBoolean(intStatus);
            }
        }

        /// <summary>
        /// Gets or sets how the interrupt pin latches
        /// </summary>
        public LatchBehaviorTypes LatchBehavior {
            get => latchBehavior;
            set {
                if (!Enum.IsDefined(typeof(LatchBehaviorTypes), value)) {
                    throw new ArgumentOutOfRangeException();
                }
                var intLatch = Peripheral.ReadRegister(Registers.INTERRUPT);
                intLatch = (byte)((intLatch & ~MaskValues.INT_LATCH) | (byte)value << 4);
                Peripheral?.WriteRegister(Registers.INTERRUPT, intLatch);
                latchBehavior = value;
            }
        } protected LatchBehaviorTypes latchBehavior;

        /// <summary>
        /// Gets or sets the source channel that triggers the interrupt
        /// </summary>
        public InterruptChannels InterruptSource {
            get => interruptSource;
            set {
                if (!Enum.IsDefined(typeof(InterruptChannels), value)) { throw new ArgumentOutOfRangeException(); }
                var intSource = Peripheral.ReadRegister(Registers.INTERRUPT);
                intSource = (byte)((intSource & ~MaskValues.INT_SOURCE) | (byte)value << 2);
                Peripheral?.WriteRegister(Registers.INTERRUPT, intSource);
                interruptSource = value;
            }
        } protected InterruptChannels interruptSource;

        /// <summary>
        /// Gets or sets whether the interrupt pin is enabled
        /// </summary>
        public bool InterruptIsEnabled {
            get => isInterruptEnabled;
            set {
                var intPin = Peripheral.ReadRegister(Registers.INTERRUPT);
                intPin = (byte)((intPin & ~MaskValues.INT_ENABLE) | Convert.ToByte(value));
                Peripheral?.WriteRegister(Registers.INTERRUPT, intPin);
                isInterruptEnabled = value;
            }
        } protected bool isInterruptEnabled;

        /// <summary>
        /// Gets or sets the persistence function of the interrupt
        /// </summary>
        public InterruptTypes InterruptPersistence {
            get => interruptPersistence;
            set {
                if (!Enum.IsDefined(typeof(InterruptTypes), value)) {
                    throw new ArgumentOutOfRangeException();
                }
                var intPersistence = Peripheral.ReadRegister(Registers.PERSISTENCE);
                intPersistence = (byte)((intPersistence & ~MaskValues.PERSISTENCE_MASK) | (byte)value);
                Peripheral?.WriteRegister(Registers.PERSISTENCE, intPersistence);
                interruptPersistence = value;
            }
        } protected InterruptTypes interruptPersistence;

        /// <summary>
        /// Gets or sets the lower interrupt threshold
        /// </summary>
        public ushort LowerInterruptThreshold {
            get => lowerInterruptThreshold;
            set {
                Peripheral.WriteRegister(Registers.TL, value);
                lowerInterruptThreshold = value;
            }
        } protected ushort lowerInterruptThreshold;

        /// <summary>
        /// Gets or sets the upper interrupt threshold
        /// </summary>
        public ushort UpperInterruptThreshold {
            get => upperInterruptThreshold;
            set {
                Peripheral.WriteRegister(Registers.TH, value);
                upperInterruptThreshold = value;
            }
        } protected ushort upperInterruptThreshold;

        /// <summary>
        /// Gets or sets the channel compensation multipliers which are used to scale the channel measurements
        /// </summary>
        public ChannelMultipliers CompensationMultipliers { get; set; }

        //==== ctors

        /// <summary>
        ///     Create a new BH17545 color sensor object
        /// </summary>
        public Bh1745(II2cBus i2cBus, byte address = Addresses.Low)
            : base(i2cBus, address)
        {
            CompensationMultipliers = new ChannelMultipliers {
                Red = 1.0, //2.2,
                Green = 1.0,
                Blue = 1.0, //1.8,
                Clear = 10.0
            };

            Reset();
        }

        //==== internal methods

        protected override Task<(Illuminance? AmbientLight, Color? Color, bool Valid)> ReadSensor()
        {
            return Task.Run(() => {
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

                // TODO: honestly, no idea what this comes back as
                // need to test when i get a sensor and compare to other light
                // sensors
                conditions.AmbientLight = new Illuminance(compensatedClear, Units.Illuminance.UnitType.Lux);

                // WTH
                conditions.Valid = ReadMeasurementIsValid();

                return conditions;
            });
        }

        protected override void RaiseEventsAndNotify(IChangeResult<(Illuminance? AmbientLight, Color? Color, bool Valid)> changeResult)
        {
            if (changeResult.New.AmbientLight is { } ambient) {
                LuminosityUpdated?.Invoke(this, new ChangeResult<Illuminance>(ambient, changeResult.Old?.AmbientLight));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Resets the device to the default configuration
        /// On reset the sensor goes to power down mode
        /// </summary>
        protected void Reset()
        {
            Console.WriteLine("Reset");

            var status = Peripheral.ReadRegister(Registers.SYSTEM_CONTROL);
            status = (byte)((status & ~MaskValues.SW_RESET) | 0x01 << 7);

            Peripheral.WriteRegister(Registers.SYSTEM_CONTROL, status);

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
            Peripheral.WriteRegister(Registers.MODE_CONTROL3, 0x02);
        }

        /// <summary>
        /// Reads whether the last measurement is valid
        /// </summary>
        protected bool ReadMeasurementIsValid()
        {
            var valid = Peripheral.ReadRegister(Registers.MODE_CONTROL2);
            valid = (byte)(valid & MaskValues.VALID);
            return Convert.ToBoolean(valid);
        }

        /// <summary>
        /// Reads the red data register of the sensor
        /// </summary>
        /// <returns></returns>
        protected ushort ReadRedDataRegister() => Peripheral.ReadRegisterAsUShort(Registers.RED_DATA);

        /// <summary>
        /// Reads the green data register of the sensor
        /// </summary>
        /// <returns></returns>
        protected ushort ReadGreenDataRegister() => Peripheral.ReadRegisterAsUShort(Registers.GREEN_DATA);

        /// <summary>
        /// Reads the blue data register of the sensor
        /// </summary>
        /// <returns></returns>
        protected ushort ReadBlueDataRegister() => Peripheral.ReadRegisterAsUShort(Registers.BLUE_DATA);

        /// <summary>
        /// Reads the clear data register of the sensor
        /// </summary>
        /// <returns></returns>
        protected ushort ReadClearDataRegister() => Peripheral.ReadRegisterAsUShort(Registers.CLEAR_DATA);

    }
}
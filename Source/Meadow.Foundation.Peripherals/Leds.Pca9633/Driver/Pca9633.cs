using Meadow.Hardware;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents a Pca9633 led driver
    /// </summary>
    public partial class Pca9633 : II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        /// <summary>
        /// Red LED location - used for RGB control
        /// </summary>
        public LedPosition LedRed { get; set; } = LedPosition.Led2;
        /// <summary>
        /// Red LED location - used for RGB control
        /// </summary>
        public LedPosition LedGreen { get; set; } = LedPosition.Led1;
        /// <summary>
        /// Red LED location - used for RGB control
        /// </summary>
        public LedPosition LedBlue { get; set; } = LedPosition.Led0;

        /// <summary>
        /// Turn led controller output on or off
        /// </summary>
        public bool IsOn
        {
            get => isOn;
            set => i2cComms.WriteRegister((byte)Registers.LEDOUT, (byte)(value == true ? 1 : 0));
        }
        bool isOn = true;

        /// <summary>
        /// Create a new Pca9633 led controller object
        /// </summary>
        /// <param name="i2cBus">i2c bus</param>
        /// <param name="address">i2c address</param>
        public Pca9633(II2cBus i2cBus, Addresses address = Addresses.Default)
            : this(i2cBus, (byte)address)
        {
        }

        /// <summary>
        /// Create a new Pca9633 led controller object
        /// </summary>
        /// <param name="i2cBus">i2c bus</param>
        /// <param name="address">i2c address</param>
        public Pca9633(II2cBus i2cBus, byte address)
        {
            i2cComms = new I2cCommunications(i2cBus, address);

            Initialize();
        }

        void Initialize()
        {
            // backlight init
            i2cComms.WriteRegister((byte)Registers.MODE1, 0);
            // set LEDs controllable by both PWM and GRPPWM registers
            i2cComms.WriteRegister(8, 0xFF);
            i2cComms.WriteRegister((byte)Registers.MODE2, 20);
        }

        /// <summary>
        /// Put device into active state
        /// </summary>
        public void Wake()
        {
            var value = i2cComms.ReadRegister((byte)Registers.MODE1);
            value = (byte)(value & ~(1 << BIT_SLEEP));
            i2cComms.WriteRegister((byte)Registers.MODE1, value);
        }

        /// <summary>
        /// Put device into sleep state
        /// </summary>
        public void Sleep()
        {
            var value = i2cComms.ReadRegister((byte)Registers.MODE1);
            value = (byte)(value | (1 << BIT_SLEEP));
            i2cComms.WriteRegister((byte)Registers.MODE1, value);
        }

        /// <summary>
        /// Helper method to set RGB led positions for color control
        /// </summary>
        /// <param name="redLed">red led position</param>
        /// <param name="greenLed">green led position</param>
        /// <param name="blueLed">blue led position</param>
        public void SetRgbLedPositions(LedPosition redLed, LedPosition greenLed, LedPosition blueLed)
        {
            LedRed = redLed;
            LedGreen = greenLed;
            LedBlue = blueLed;
        }

        /// <summary>
        /// Set RGB LED color if red, green and blue LEDs are set
        /// </summary>
        /// <param name="color">Color</param>
        public void SetColor(Color color)
        {
            if (LedRed != LedPosition.Undefined)
            {
                i2cComms.WriteRegister((byte)LedRed, (byte)(255 - color.R));
            }
            if (LedGreen != LedPosition.Undefined)
            {
                i2cComms.WriteRegister((byte)LedGreen, (byte)(255 - color.G));
            }
            if (LedBlue != LedPosition.Undefined)
            {
                i2cComms.WriteRegister((byte)LedBlue, (byte)(255 - color.B));
            }
        }

        /// <summary>
        /// Set brightness of an individual led
        /// </summary>
        /// <param name="led">led position</param>
        /// <param name="brightness">brightness (0-255)</param>
        public void SetLedBrightness(LedPosition led, byte brightness)
        {
            i2cComms.WriteRegister((byte)led, (byte)(255 - brightness));
        }

        /// <summary>
        /// Set brightness of all leds
        /// </summary>
        /// <param name="brightness">brightness (0-255)</param>
        public void SetGroupBrightess(byte brightness)
        {
            i2cComms.WriteRegister((byte)Registers.GRPPWM, (byte)(255 - brightness));
        }

        /// <summary>
        /// Set the drive mode 
        /// Open drain or totem pole
        /// </summary>
        /// <param name="drive"></param>
        public void SetDriveMode(DriveType drive)
        {
            var value = i2cComms.ReadRegister((byte)Registers.MODE2);
            value = (byte)(value & ~(1 << BIT_OUTDRV));
            value |= (byte)((byte)drive << BIT_OUTDRV);
            i2cComms.WriteRegister((byte)Registers.MODE2, value);
        }

        /// <summary>
        /// Set auto increment mode of control registers
        /// </summary>
        /// <param name="mode"></param>
        public void SetAutoIncrementMode(AutoIncrement mode)
        {
            var value = mode switch
            {
                AutoIncrement.AllRegisters => 1 << 7,
                AutoIncrement.IndividualBrightnessRegisters => 1 << 7 | 1 << 6,
                AutoIncrement.GlobalControlRegisters => 1 << 7 | 1 << 5,
                AutoIncrement.IndividualAndGlobalRegisters => 1 << 7 | 1 << 6 | 1 << 5,
                _ => 0,
            };
            i2cComms.WriteRegister((byte)Registers.MODE1, (byte)value);

        }

        //helper values for bit manipulation
        readonly byte BIT_OUTDRV = 2;
        readonly byte BIT_SLEEP = 4;
    }
}
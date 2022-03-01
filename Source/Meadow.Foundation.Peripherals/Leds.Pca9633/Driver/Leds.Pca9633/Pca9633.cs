using Meadow.Hardware;

namespace Meadow.Foundation.Leds
{
    /// <summary>
    /// Represents Pca9633
    /// </summary>
    public partial class Pca9633
    {
        II2cPeripheral i2CPeripheral;

        static byte BIT_SLEEP = 4;

        public Pca9633(II2cBus i2cBus, Addresses address)
            : this(i2cBus, (byte)address)
        {
        }

        public Pca9633(II2cBus i2cBus, byte address)
        {
            i2CPeripheral = new I2cPeripheral(i2cBus, address);

            Initialize();
        }

        void Initialize()
        {
            // backlight init
            i2CPeripheral.WriteRegister((byte)Registers.MODE1, 0);
            i2CPeripheral.WriteRegister((byte)Registers.MODE2, 0);
        }

        /// <summary>
        /// Put device into active stte
        /// </summary>
        public void Wake()
        {
            var value = i2CPeripheral.ReadRegister((byte)Registers.MODE1);
            value = (byte)(value & ~(1 << BIT_SLEEP));
            i2CPeripheral.WriteRegister((byte)Registers.MODE1, value);
        }
        
        /// <summary>
        /// Put device into sleep state
        /// </summary>
        public void Sleep()
        {
            var value = i2CPeripheral.ReadRegister((byte)Registers.MODE1);
            value = (byte)(value | (1 << BIT_SLEEP));
            i2CPeripheral.WriteRegister((byte)Registers.MODE1, value);
        }

        /// <summary>
        /// Turn LEDs on
        /// </summary>
        public void SetOn()
        {
            i2CPeripheral.WriteRegister((byte)Registers.LEDOUT, 1);
        }

        /// <summary>
        /// Turn LEDs off
        /// </summary>
        public void SetOff()
        {
            i2CPeripheral.WriteRegister((byte)Registers.LEDOUT, 0);
        }

        /// <summary>
        /// Set RGB LED color
        /// </summary>
        /// <param name="color"></param>
        public void SetColor(Color color)
        {
            i2CPeripheral.WriteRegister(0x04, color.R);
            i2CPeripheral.WriteRegister(0x03, color.G);
            i2CPeripheral.WriteRegister(0x02, color.B);
        }

        /// <summary>
        /// Set brightness of group
        /// </summary>
        /// <param name="brightness">brightness (0-255)</param>
        public void SetGroupPwm(byte brightness)
        {
            i2CPeripheral.WriteRegister((byte)Registers.GRPPWM, brightness);
        }

        /// <summary>
        /// Set auto increment mode of control registers
        /// </summary>
        /// <param name="mode"></param>
        public void SetAutoIncrementMode(AutoIncrement mode)
        {
            byte value;

            switch(mode)
            {
                case AutoIncrement.AllRegisters:
                    value = 1 << 7;
                    break;
                case AutoIncrement.IndividualBrightnessRegisters:
                    value = 1 << 7 | 1 << 6;
                    break;
                case AutoIncrement.GlobaControlRegisters:
                    value = 1 << 7 | 1 << 5;
                    break;
                case AutoIncrement.IndividualAndGlobalRegisters:
                    value = 1 << 7 | 1 << 6 | 1 << 5;
                    break;
                default:
                case AutoIncrement.None:
                    value = 0;
                    break;
            }

            i2CPeripheral.WriteRegister((byte)Registers.MODE1, value);

        }
    }
}
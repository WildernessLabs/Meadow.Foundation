using Meadow.Hardware;
using System;
using System.Threading;

namespace FeatherWings.MiniTft
{
    public class SeeSaw
    {
        #region Member variables / fields

        private I2cPeripheral _i2cPeripheral;
        private byte _i2caddr;
        int _flow;

        #endregion

        #region Constants

        private readonly byte SEESAW_STATUS_BASE = 0x00;
        private readonly byte SEESAW_GPIO_BASE = 0x01;
        private readonly byte SEESAW_SERCOM0_BASE = 0x02;

        private readonly byte SEESAW_TIMER_BASE = 0x08;
        private readonly byte SEESAW_ADC_BASE = 0x09;
        private readonly byte SEESAW_DAC_BASE = 0x0A;
        private readonly byte SEESAW_INTERRUPT_BASE = 0x0B;
        private readonly byte SEESAW_DAP_BASE = 0x0C;
        private readonly byte SEESAW_EEPROM_BASE = 0x0D;
        private readonly byte SEESAW_NEOPIXEL_BASE = 0x0E;
        private readonly byte SEESAW_TOUCH_BASE = 0x0F;
        private readonly byte SEESAW_KEYPAD_BASE = 0x10;
        private readonly byte SEESAW_ENCODER_BASE = 0x11;

        /** GPIO module function address registers
         */
        private readonly byte SEESAW_GPIO_DIRSET_BULK = 0x02;
        private readonly byte SEESAW_GPIO_DIRCLR_BULK = 0x03;
        private readonly byte SEESAW_GPIO_BULK = 0x04;
        private readonly byte SEESAW_GPIO_BULK_SET = 0x05;
        private readonly byte SEESAW_GPIO_BULK_CLR = 0x06;
        private readonly byte SEESAW_GPIO_BULK_TOGGLE = 0x07;
        private readonly byte SEESAW_GPIO_INTENSET = 0x08;
        private readonly byte SEESAW_GPIO_INTENCLR = 0x09;
        private readonly byte SEESAW_GPIO_INTFLAG = 0x0A;
        private readonly byte SEESAW_GPIO_PULLENSET = 0x0B;
        private readonly byte SEESAW_GPIO_PULLENCLR = 0x0C;

        /** status module function address registers
         */
        private readonly byte SEESAW_STATUS_HW_ID = 0x01;
        private readonly byte SEESAW_STATUS_VERSION = 0x02;
        private readonly byte SEESAW_STATUS_OPTIONS = 0x03;
        private readonly byte SEESAW_STATUS_TEMP = 0x04;
        private readonly byte SEESAW_STATUS_SWRST = 0x7F;

        /** timer module function address registers
         */
        private readonly byte SEESAW_TIMER_STATUS = 0x00;
        private readonly byte SEESAW_TIMER_PWM = 0x01;
        private readonly byte SEESAW_TIMER_FREQ = 0x02;

        /** ADC module function addres registers
         */
        private readonly byte SEESAW_ADC_STATUS = 0x00;
        private readonly byte SEESAW_ADC_INTEN = 0x02;
        private readonly byte SEESAW_ADC_INTENCLR = 0x03;
        private readonly byte SEESAW_ADC_WINMODE = 0x04;
        private readonly byte SEESAW_ADC_WINTHRESH = 0x05;
        private readonly byte SEESAW_ADC_CHANNEL_OFFSET = 0x07;

        /** Sercom module function addres registers
         */
        private readonly byte SEESAW_SERCOM_STATUS = 0x00;
        private readonly byte SEESAW_SERCOM_INTEN = 0x02;
        private readonly byte SEESAW_SERCOM_INTENCLR = 0x03;
        private readonly byte SEESAW_SERCOM_BAUD = 0x04;
        private readonly byte SEESAW_SERCOM_DATA = 0x05;

        /** neopixel module function addres registers
         */
        private readonly byte SEESAW_NEOPIXEL_STATUS = 0x00;
        private readonly byte SEESAW_NEOPIXEL_PIN = 0x01;
        private readonly byte SEESAW_NEOPIXEL_SPEED = 0x02;
        private readonly byte SEESAW_NEOPIXEL_BUF_LENGTH = 0x03;
        private readonly byte SEESAW_NEOPIXEL_BUF = 0x04;
        private readonly byte SEESAW_NEOPIXEL_SHOW = 0x05;

        /** touch module function addres registers
         */
        private readonly byte SEESAW_TOUCH_CHANNEL_OFFSET = 0x10;

        /** keypad module function addres registers
         */
        private readonly byte SEESAW_KEYPAD_STATUS = 0x00;
        private readonly byte SEESAW_KEYPAD_EVENT = 0x01;
        private readonly byte SEESAW_KEYPAD_INTENSET = 0x02;
        private readonly byte SEESAW_KEYPAD_INTENCLR = 0x03;
        private readonly byte SEESAW_KEYPAD_COUNT = 0x04;
        private readonly byte SEESAW_KEYPAD_FIFO = 0x10;

        /** keypad module edge definitions
         */
        private readonly byte SEESAW_KEYPAD_EDGE_HIGH = 0;
        private readonly byte SEESAW_KEYPAD_EDGE_LOW = 1;
        private readonly byte SEESAW_KEYPAD_EDGE_FALLING = 2;
        private readonly byte SEESAW_KEYPAD_EDGE_RISING = 3;

        /** encoder module edge definitions
         */
        private readonly byte SEESAW_ENCODER_STATUS = 0x00;
        private readonly byte SEESAW_ENCODER_INTENSET = 0x02;
        private readonly byte SEESAW_ENCODER_INTENCLR = 0x03;
        private readonly byte SEESAW_ENCODER_POSITION = 0x04;
        private readonly byte SEESAW_ENCODER_DELTA = 0x05;

        private const byte ADC_INPUT_0_PIN = 2; ///< default ADC input pin
        private const byte ADC_INPUT_1_PIN = 3; ///< default ADC input pin
        private const byte ADC_INPUT_2_PIN = 4; ///< default ADC input pin
        private const byte ADC_INPUT_3_PIN = 5; ///< default ADC input pin

        private const byte PWM_0_PIN = 4; ///< default PWM output pin
        private const byte PWM_1_PIN = 5; ///< default PWM output pin
        private const byte PWM_2_PIN = 6; ///< default PWM output pin
        private const byte PWM_3_PIN = 7; ///< default PWM output pin

        private const byte INPUT = 0;
        private const byte INPUT_PULLUP = 1;
        private const byte INPUT_PULLDOWN = 2;
        private const byte OUTPUT = 3;

        private readonly byte SEESAW_HW_ID_CODE = 0x55;

        #endregion

        private void Delay (int delay)
        {
            Thread.Sleep(delay);
        }


        /*!
         *****************************************************************************************
         *  @brief      Start the seesaw
         *
         *				This should be called when your sketch is
         *connecting to the seesaw
         *
         *  @param      addr the I2C address of the seesaw
         *  @param      flow the flow control pin to use
         *  @param		reset pass true to reset the seesaw on startup. Defaults
         *to true.
         *
         *  @return     true if we could connect to the seesaw, false otherwise
         ****************************************************************************************/
        private bool Begin(byte addr, int flow, bool reset)
        {
            _i2caddr = addr;
            _flow = flow;

            if (_flow != -1)
            {
                PinMode((byte)_flow, INPUT);
            }

          //  _i2c_init();

            if (reset)
            {
                SWReset();
                Delay(500);
            }

            byte c = Read8(SEESAW_STATUS_BASE, SEESAW_STATUS_HW_ID);
            if (c != SEESAW_HW_ID_CODE)
            {
                return false;
            }
            return true;
        }

/*!
 *******************************************************************
 *  @brief      perform a software reset. This resets all seesaw registers to
 *their default values.
 *  			This is called automatically from
 *Adafruit_seesaw.begin()

 ********************************************************************/
void SWReset()
{
    Write8(SEESAW_STATUS_BASE, SEESAW_STATUS_SWRST, 0xFF);
}

/*!
 **************************************************************************
 *  @brief      Returns the available options compiled into the seesaw firmware.
 *  @return     the available options compiled into the seesaw firmware. If the
 *option is included, the corresponding bit is set. For example, if the ADC
 *module is compiled in then (ss.getOptions() & (1UL << SEESAW_ADC_BASE)) > 0
 ***********************************************************************/
uint GetOptions()
{
            var buf = new byte[4];
    Read(SEESAW_STATUS_BASE, SEESAW_STATUS_OPTIONS, buf, 4);
    uint ret = ((uint)buf[0] << 24) | ((uint)buf[1] << 16) |
                   ((uint)buf[2] << 8) | (uint)buf[3];
    return ret;
}

/*!
 *********************************************************************
 *  @brief      Returns the version of the seesaw
 *  @return     The version code. Bits [31:16] will be a date code, [15:0] will
 *be the product id.
 ********************************************************************/
uint GetVersion()
{
    var buf = new byte[4];
    Read(SEESAW_STATUS_BASE, SEESAW_STATUS_VERSION, buf, 4);
    uint ret = ((uint)buf[0] << 24) | ((uint)buf[1] << 16) |
                   ((uint)buf[2] << 8) | (uint)buf[3];
    return ret;
}

/*!
 **************************************************************************
 *  @brief      Set the mode of a GPIO pin.
 *
 *  @param      pin the pin number. On the SAMD09 breakout, this corresponds to
 *the number on the silkscreen.
 *  @param		mode the mode to set the pin. One of INPUT, OUTPUT, or
 *INPUT_PULLUP.

 ************************************************************************/
void PinMode(byte pin, byte mode)
{
            if (pin >= 32)
            {
                pinModeBulk(0, (uint)(1ul << (pin - 32)), mode);
            }
            else
            {
                PinModeBulk((uint)(1ul << pin), mode);
            }
}

/*!
 ***************************************************************************
 *  @brief      Set the output of a GPIO pin
 *
 *  @param      pin the pin number. On the SAMD09 breakout, this corresponds to
 *the number on the silkscreen.
 *	@param		value the value to write to the GPIO pin. This should be
 *HIGH or LOW.
 ***************************************************************************/
void digitalWrite(byte pin, byte value)
{
            if (pin >= 32)
            {
                DigitalWriteBulk(0, (uint)(1ul << (pin - 32)), value);
            }
            else
            {
                DigitalWriteBulk((uint)(1ul << pin), value);
            }
}

/*!
 ****************************************************************************
 *  @brief      Read the current status of a GPIO pin
 *
 *  @param      pin the pin number. On the SAMD09 breakout, this corresponds to
 *the number on the silkscreen.
 *
 *  @return     the status of the pin. HIGH or LOW (1 or 0).
 ***********************************************************************/
private bool DigitalRead(byte pin)
{
            if (pin >= 32)
            {
                return DigitalReadBulkB((uint)(1ul << (pin - 32))) != 0;
            }
            else
            {
                return DigitalReadBulk((uint)(1ul << pin)) != 0;
            }
}

/*!
 ****************************************************************************
 *  @brief      read the status of multiple pins on port A.
 *
 *  @param      pins a bitmask of the pins to write. On the SAMD09 breakout,
 *this corresponds to the number on the silkscreen. For example, passing 0b0110
 *will return the values of pins 2 and 3.
 *
 *  @return     the status of the passed pins. If 0b0110 was passed and pin 2 is
 *high and pin 3 is low, 0b0010 (decimal number 2) will be returned.
 *******************************************************************/
uint DigitalReadBulk(uint pins)
{
    var buf = new byte[4];
    Read(SEESAW_GPIO_BASE, SEESAW_GPIO_BULK, buf, 4);
    uint ret = ((uint)buf[0] << 24) | ((uint)buf[1] << 16) |
                   ((uint)buf[2] << 8) | (uint)buf[3];
    return ret & pins;
}

/*!
 **************************************************************************
 *  @brief      read the status of multiple pins on port B.
 *
 *  @param      pins a bitmask of the pins to write.
 *
 *  @return     the status of the passed pins. If 0b0110 was passed and pin 2 is
 *high and pin 3 is low, 0b0010 (decimal number 2) will be returned.
 ************************************************************************/
        uint DigitalReadBulkB(uint pins)
        {
            var buf = new byte[8];
            Read(SEESAW_GPIO_BASE, SEESAW_GPIO_BULK, buf, 8);
            uint ret = ((uint)buf[4] << 24) | ((uint)buf[5] << 16) |
                           ((uint)buf[6] << 8) | (uint)buf[7];
            return ret & pins;
        }

        /*!
         **********************************************************************
         *  @brief      Enable or disable GPIO interrupts on the passed pins
         *
         *  @param      pins a bitmask of the pins to write. On the SAMD09 breakout,
         *this corresponds to the number on the silkscreen. For example, passing 0b0110
         *will enable or disable interrups on pins 2 and 3.
         *	@param		enabled pass true to enable the interrupts on the passed
         *pins, false to disable the interrupts on the passed pins.
         ***********************************************************************/
        void SetGPIOInterrupts(uint pins, bool enabled)
        {
            var cmd = new byte[] {(byte)(pins >> 24), 
                                (byte)(pins >> 16),
                                (byte)(pins >> 8), 
                                (byte)pins};
            if (enabled)
            {
                Write(SEESAW_GPIO_BASE, SEESAW_GPIO_INTENSET, cmd, 4);
            }
            else
            {
                Write(SEESAW_GPIO_BASE, SEESAW_GPIO_INTENCLR, cmd, 4);
            }
        }

/*!
 ****************************************************************
 *  @brief      read the analog value on an ADC-enabled pin.
 *
 *  @param      pin the number of the pin to read. On the SAMD09 breakout, this
 *corresponds to the number on the silkscreen. On the default seesaw firmware on
 *the SAMD09 breakout, pins 2, 3, and 4 are ADC-enabled.
 *
 *  @return     the analog value. This is an integer between 0 and 1023
 ***********************************************************************/
ushort AnalogRead(byte pin)
{
    var buf = new byte[2];
    byte p;
    switch (pin)
    {
        case ADC_INPUT_0_PIN:
            p = 0;
            break;
        case ADC_INPUT_1_PIN:
            p = 1;
            break;
        case ADC_INPUT_2_PIN:
            p = 2;
            break;
        case ADC_INPUT_3_PIN:
            p = 3;
            break;
        default:
            return 0;
    }

    Read(SEESAW_ADC_BASE, (byte)(SEESAW_ADC_CHANNEL_OFFSET + p), buf, 2, 500);
    ushort ret = (ushort)((buf[0] << 8) | buf[1]);
    Delay(1);
    return ret;
}

/*!
 ******************************************************************************
 *  @brief      read the analog value on an capacitive touch-enabled pin.
 *
 *  @param      pin the number of the pin to read.
 *
 *  @return     the analog value. This is an integer between 0 and 1023
 ****************************************************************************/
ushort TouchRead(byte pin)
{
    var buf = new byte[2];
    byte p = pin;
    ushort ret = 65535;
    do
    {
        Delay(1);
        Read(SEESAW_TOUCH_BASE, (byte)(SEESAW_TOUCH_CHANNEL_OFFSET + p), buf, 2,
                   1000);
        ret = (ushort)(buf[0] << 8 | buf[1]);
    } while (ret == 65535);
    return ret;
}

/*!
 ***************************************************************************
 *  @brief      set the mode of multiple GPIO pins at once.
 *
 *  @param      pins a bitmask of the pins to write. On the SAMD09 breakout,
 *this corresponds to the number on the silkscreen. For example, passing 0b0110
 *will set the mode of pins 2 and 3.
 *	@param		mode the mode to set the pins to. One of INPUT, OUTPUT,
 *or INPUT_PULLUP.
 ************************************************************************/
void PinModeBulk(uint pins, byte mode)
{
    var cmd = new byte[] {(byte)(pins >> 24), (byte)(pins >> 16),
                   (byte)(pins >> 8), (byte)pins};
    switch (mode)
    {
        case OUTPUT:
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_DIRSET_BULK, cmd, 4);
            break;
        case INPUT:
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_DIRCLR_BULK, cmd, 4);
            break;
        case INPUT_PULLUP:
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_DIRCLR_BULK, cmd, 4);
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_PULLENSET, cmd, 4);
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_BULK_SET, cmd, 4);
            break;
        case INPUT_PULLDOWN:
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_DIRCLR_BULK, cmd, 4);
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_PULLENSET, cmd, 4);
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_BULK_CLR, cmd, 4);
            break;
    }
}

/*!
 *****************************************************************************************
 *  @brief      set the mode of multiple GPIO pins at once. This supports both
 *ports A and B.
 *
 *  @param      pinsa a bitmask of the pins to write on port A. On the SAMD09
 *breakout, this corresponds to the number on the silkscreen. For example,
 *passing 0b0110 will set the mode of pins 2 and 3.
 *  @param      pinsb a bitmask of the pins to write on port B.
 *	@param		mode the mode to set the pins to. One of INPUT, OUTPUT,
 *or INPUT_PULLUP.
 ****************************************************************************************/
void pinModeBulk(uint pinsa, uint pinsb,
                                  byte mode)
{
    var cmd = new byte[] {(byte)(pinsa >> 24), (byte)(pinsa >> 16),
                   (byte)(pinsa >> 8),  (byte)pinsa,
                   (byte)(pinsb >> 24), (byte)(pinsb >> 16),
                   (byte)(pinsb >> 8),  (byte)pinsb};
    switch (mode)
    {
        case OUTPUT:
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_DIRSET_BULK, cmd, 8);
            break;
        case INPUT:
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_DIRCLR_BULK, cmd, 8);
            break;
        case INPUT_PULLUP:
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_DIRCLR_BULK, cmd, 8);
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_PULLENSET, cmd, 8);
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_BULK_SET, cmd, 8);
            break;
        case INPUT_PULLDOWN:
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_DIRCLR_BULK, cmd, 8);
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_PULLENSET, cmd, 8);
            Write(SEESAW_GPIO_BASE, SEESAW_GPIO_BULK_CLR, cmd, 8);
            break;
    }
}

/*!
 *****************************************************************************************
 *  @brief      write a value to multiple GPIO pins at once.
 *
 *  @param      pins a bitmask of the pins to write. On the SAMD09 breakout,
 *this corresponds to the number on the silkscreen. For example, passing 0b0110
 *will write the passed value to pins 2 and 3.
 *	@param		value pass HIGH to set the output on the passed pins to
 *HIGH, low to set the output on the passed pins to LOW.
 ****************************************************************************************/
void DigitalWriteBulk(uint pins, byte value)
{
    var cmd = new byte[] {(byte)(pins >> 24), (byte)(pins >> 16),
                   (byte)(pins >> 8), (byte)pins};
            if (value > 0)
            {
                Write(SEESAW_GPIO_BASE, SEESAW_GPIO_BULK_SET, cmd, 4);
            }
            else
            {
                Write(SEESAW_GPIO_BASE, SEESAW_GPIO_BULK_CLR, cmd, 4);
            }
}

/*!
 *****************************************************************************************
 *  @brief      write a value to multiple GPIO pins at once. This supports both
 *ports A and B
 *
 *  @param      pinsa a bitmask of the pins to write on port A. On the SAMD09
 *breakout, this corresponds to the number on the silkscreen. For example,
 *passing 0b0110 will write the passed value to pins 2 and 3.
 *  @param      pinsb a bitmask of the pins to write on port B.
 *	@param		value pass HIGH to set the output on the passed pins to
 *HIGH, low to set the output on the passed pins to LOW.
 ****************************************************************************************/
void DigitalWriteBulk(uint pinsa, uint pinsb,
                                       byte value)
{
    var cmd = new byte[] {(byte)(pinsa >> 24), (byte)(pinsa >> 16),
                   (byte)(pinsa >> 8),  (byte)pinsa,
                   (byte)(pinsb >> 24), (byte)(pinsb >> 16),
                   (byte)(pinsb >> 8),  (byte)pinsb};
            if (value > 0)
            {
                Write(SEESAW_GPIO_BASE, SEESAW_GPIO_BULK_SET, cmd, 8);
            }
            else
            {
                Write(SEESAW_GPIO_BASE, SEESAW_GPIO_BULK_CLR, cmd, 8);
            }
}

/*!
 *****************************************************************************************
 *  @brief      write a PWM value to a PWM-enabled pin
 *
 *  @param      pin the number of the pin to write. On the SAMD09 breakout, this
 *corresponds to the number on the silkscreen. on the default seesaw firmware on
 *the SAMD09 breakout, pins 5, 6, and 7 are PWM enabled.
 *	@param		value the value to write to the pin
 *	@param		width the width of the value to write. Defaults to 8. If
 *16 is passed a 16 bit value will be written.
 ****************************************************************************************/
void AnalogWrite(byte pin, ushort value, byte width)
{
            
    int p = -1;
    switch (pin)
    {
        case PWM_0_PIN:
            p = 0;
            break;
        case PWM_1_PIN:
            p = 1;
            break;
        case PWM_2_PIN:
            p = 2;
            break;
        case PWM_3_PIN:
            p = 3;
            break;
        default:
            break;
    }
    if (p > -1)
    {
        if (width == 16)
        {
            var cmd = new byte[] { (byte)p, (byte)(value >> 8), (byte)value };
            Write(SEESAW_TIMER_BASE, SEESAW_TIMER_PWM, cmd, 3);
        }
        else
        {
                    ushort mappedVal = (ushort)(value * 255 / 65535);//    map(value, 0, 255, 0, 65535);

            var cmd = new byte[] {(byte)p, (byte)(mappedVal >> 8),
                       (byte)mappedVal};
            Write(SEESAW_TIMER_BASE, SEESAW_TIMER_PWM, cmd, 3);
        }
    }
}

/*!
 *  @brief      set the PWM frequency of a PWM-enabled pin. Note that on SAMD09,
 *              SAMD11 boards the frequency will be mapped to closest match
 *		fixed frequencies. Also note that PWM pins 4 and 5 share a
 *timer, and PWM pins 6 and 7 share a timer. Changing the frequency for one pin
 *will change the frequency for the other pin that is on the timer.
 *
 *  @param      pin the number of the pin to change frequency of. On the SAMD09
 *              breakout, this corresponds to the number on the silkscreen.
 *              on the default seesaw firmware on the SAMD09 breakout, pins 5,
 *6, and 7 are PWM enabled.
 *  @param      freq the frequency to set.
 ******************************************************************************/
void setPWMFreq(byte pin, ushort freq)
{
    int p = -1;
    switch (pin)
    {
        case PWM_0_PIN:
            p = 0;
            break;
        case PWM_1_PIN:
            p = 1;
            break;
        case PWM_2_PIN:
            p = 2;
            break;
        case PWM_3_PIN:
            p = 3;
            break;
        default:
            break;
    }
    if (p > -1)
    {
        var cmd = new byte[] { (byte)p, (byte)(freq >> 8), (byte)freq };
        Write(SEESAW_TIMER_BASE, SEESAW_TIMER_FREQ, cmd, 3);
    }
}

/*!
 *****************************************************************************************
 *  @brief      Write a 1 byte to an EEPROM address
 *
 *  @param      addr the address to write to. On the default seesaw firmware on
 *the SAMD09 breakout this is between 0 and 63.
 *	@param		val to write between 0 and 255
 ****************************************************************************************/
void EEPROMWrite8(byte addr, byte val)
{
    EEPROMWrite(addr, new byte[] { val }, 1);
}

/*!
 *****************************************************************************************
 *  @brief      write a string of bytes to EEPROM starting at the passed address
 *
 *  @param      addr the starting address to write the first byte. This will be
 *automatically incremented with each byte written.
 *	@param		buf the buffer of bytes to be written.
 *	@param		size the number of bytes to write. Writing past the end
 *of available EEPROM may result in undefined behavior.
 ****************************************************************************************/
void EEPROMWrite(byte addr, byte[] buf, byte size)
{
    Write(SEESAW_EEPROM_BASE, addr, buf, size);
}

/*!
 *****************************************************************************************
 *  @brief      Read 1 byte from the specified EEPROM address.
 *
 *  @param      addr the address to read from. One the default seesaw firmware
 *on the SAMD09 breakout this is between 0 and 63.
 *
 *  @return     the value between 0 and 255 that was read from the passed
 *address.
 ****************************************************************************************/
byte EEPROMRead8(byte addr)
{
    return Read8(SEESAW_EEPROM_BASE, addr);
}

/*!
 *****************************************************************************************
 *  @brief      Set the baud rate on SERCOM0.
 *
 *  @param      baud the baud rate to set. This is an integer value. Baud rates
 *up to 115200 are supported.
 ****************************************************************************************/
void UARTSetBaud(uint baud)
{
    var cmd = new byte[] {(byte)(baud >> 24), (byte)(baud >> 16),
                   (byte)(baud >> 8), (byte)baud};
    Write(SEESAW_SERCOM0_BASE, SEESAW_SERCOM_BAUD, cmd, 4);
}

/*!
 *****************************************************************************************
 *  @brief      activate or deactivate a key and edge on the keypad module
 *
 *  @param      key the key number to activate
 *  @param		edge the edge to trigger on
 *  @param		enable passing true will enable the passed event,
 *passing false will disable it.
 ****************************************************************************************/
void setKeypadEvent(byte key, byte edge, bool enable)
{
    keyState ks;
    ks.bit.STATE = enable;
    ks.bit.ACTIVE = (1 << edge);
    var cmd = new byte[] { key, ks.reg };
    Write(SEESAW_KEYPAD_BASE, SEESAW_KEYPAD_EVENT, cmd, 2);
}

/**
 *****************************************************************************************
 *  @brief      enable the keypad interrupt that fires when events are in the
 *fifo.
 ****************************************************************************************/
void enableKeypadInterrupt()
{
    Write8(SEESAW_KEYPAD_BASE, SEESAW_KEYPAD_INTENSET, 0x01);
}

/**
 *****************************************************************************************
 *  @brief      disable the keypad interrupt that fires when events are in the
 *fifo.
 ****************************************************************************************/
void DisableKeypadInterrupt()
{
    Write8(SEESAW_KEYPAD_BASE, SEESAW_KEYPAD_INTENCLR, 0x01);
}

/**
 *****************************************************************************************
 *  @brief      Get the number of events currently in the fifo
 *  @return     the number of events in the fifo
 ****************************************************************************************/
byte GetKeypadCount()
{
    return Read8(SEESAW_KEYPAD_BASE, SEESAW_KEYPAD_COUNT, 500);
}

/**
 *****************************************************************************************
 *  @brief      Read all keyEvents into the passed buffer
 *
 *  @param      buf pointer to where the keyEvents should be stored
 *  @param		count the number of events to read
 ****************************************************************************************/
/*void ReadKeypad(keyEventRaw* buf, byte count)
{
    return Read(SEESAW_KEYPAD_BASE, SEESAW_KEYPAD_FIFO, (byte[])buf,
                      count, 1000);
}*/

/**
 *****************************************************************************************
 *  @brief      Read the temperature of the seesaw board in degrees Celsius.
 *NOTE: not all seesaw firmwares have the temperature sensor enabled.
 *  @return     Temperature in degrees Celsius as a floating point value.
 ****************************************************************************************/
float GetTemp()
{
    var buf = new byte[4];
    Read(SEESAW_STATUS_BASE, SEESAW_STATUS_TEMP, buf, 4, 1000);
    int ret = (buf[0] << 24) | (buf[1] << 16) |
                  (buf[2] << 8) | buf[3];
    return (float)((1.0 / (1UL << 16)) * ret);
}

/**
 *****************************************************************************************
 *  @brief      Read the current position of the encoder
 *  @return     The encoder position as a 32 bit signed integer.
 ****************************************************************************************/
int GetEncoderPosition()
{
    var buf = new byte[4];
    Read(SEESAW_ENCODER_BASE, SEESAW_ENCODER_POSITION, buf, 4);
    int ret = ((buf[0] << 24) | (buf[1] << 16) |
                  (buf[2] << 8) | buf[3]);

    return ret;
}

/**
 *****************************************************************************************
 *  @brief      Set the current position of the encoder
 *  @param     pos the position to set the encoder to.
 ****************************************************************************************/
void SetEncoderPosition(int pos)
{
            var buf = new byte[]{(byte)(pos >> 24), (byte)(pos >> 16),
                   (byte)(pos >> 8), (byte)(pos & 0xFF)};
  //  Write(SEESAW_ENCODER_BASE, SEESAW_ENCODER_POSITION, buf, 4);
}

/**
 *****************************************************************************************
 *  @brief      Read the change in encoder position since it was last read.
 *  @return     The encoder change as a 32 bit signed integer.
 ****************************************************************************************/
int GetEncoderDelta()
{
    var buf = new byte[4];
    Read(SEESAW_ENCODER_BASE, SEESAW_ENCODER_DELTA, buf, 4);
    int ret = (buf[0] << 24) | (buf[1] << 16) |
                  (buf[2] << 8) | buf[3];

    return ret;
}

/**
 *****************************************************************************************
 *  @brief      Enable the interrupt to fire when the encoder changes position.
 ****************************************************************************************/
void EnableEncoderInterrupt()
{
    Write8(SEESAW_ENCODER_BASE, SEESAW_ENCODER_INTENSET, 0x01);
}

/**
 *****************************************************************************************
 *  @brief      Disable the interrupt from firing when the encoder changes
 *position.
 ****************************************************************************************/
void DisableEncoderInterrupt()
{
    Write8(SEESAW_ENCODER_BASE, SEESAW_ENCODER_INTENCLR, 0x01);
}

/**
 *****************************************************************************************
 *  @brief      Write 1 byte to the specified seesaw register.
 *
 *  @param      regHigh the module address register (ex. SEESAW_NEOPIXEL_BASE)
 *	@param		regLow the function address register (ex.
 *SEESAW_NEOPIXEL_PIN)
 *	@param		value the value between 0 and 255 to write
 ****************************************************************************************/
void Write8(byte regHigh, byte regLow, byte value)
{
//    Write(regHigh, regLow, new byte[] { value }, 1);
            
}

/**
 *****************************************************************************************
 *  @brief      read 1 byte from the specified seesaw register.
 *
 *  @param      regHigh the module address register (ex. SEESAW_STATUS_BASE)
 *	@param		regLow the function address register (ex.
 *SEESAW_STATUS_VERSION)
 *	@param		delay a number of microseconds to delay before reading
 *out the data. Different delay values may be necessary to ensure the seesaw
 *chip has time to process the requested data. Defaults to 125.
 *
 *  @return     the value between 0 and 255 read from the passed register
 ****************************************************************************************/
        byte Read8(byte regHigh, byte regLow, ushort delay = 125)
        {
            byte ret = 0; //ToDo
            Read(regHigh, regLow, new byte[] { ret }, 1, delay);

            return ret;
        }

/**
 *****************************************************************************************
 *  @brief      Read a specified number of bytes into a buffer from the seesaw.
 *
 *  @param      regHigh the module address register (ex. SEESAW_STATUS_BASE)
 *	@param		regLow the function address register (ex.
 *SEESAW_STATUS_VERSION)
 *	@param		buf the buffer to read the bytes into
 *	@param		num the number of bytes to read.
 *	@param		delay an optional delay in between setting the read
 *register and reading out the data. This is required for some seesaw functions
 *(ex. reading ADC data)
 ****************************************************************************************/
void Read(byte regHigh, byte regLow, byte[] buf,
                           byte num, ushort delay = 125)
{
     /*       

    byte pos = 0;

    // on arduino we need to read in 32 byte chunks
    while (pos < num)
    {
        byte read_now = (byte)Math.Min(32, num - pos);
        _i2cbus->beginTransmission((byte)_i2caddr);
        _i2cbus->Write((byte)regHigh);
        _i2cbus->Write((byte)regLow);
# ifdef SEESAW_I2C_DEBUG
        Serial.print("I2C read $");
        Serial.print((ushort)regHigh << 8 | regLow, HEX);
        Serial.print(" : ");
#endif

        if (_flow != -1)
            while (!::digitalRead(_flow))
                ;
        _i2cbus->endTransmission();

        // TODO: tune this
        delayMicroseconds(delay);

        if (_flow != -1)
            while (!::digitalRead(_flow))
                ;
        _i2cbus->requestFrom((byte)_i2caddr, read_now);

        for (int i = 0; i < read_now; i++)
        {
            buf[pos] = _i2cbus->Read();
# ifdef SEESAW_I2C_DEBUG
            Serial.print("0x");
            Serial.print(buf[pos], HEX);
            Serial.print(",");
#endif
            pos++;
        }
# ifdef SEESAW_I2C_DEBUG
        Serial.println();
#endif
    }
    */
}

/*!
 *****************************************************************************************
 *  @brief      Write a specified number of bytes to the seesaw from the passed
 *buffer.
 *
 *  @param      regHigh the module address register (ex. SEESAW_GPIO_BASE)
 *  @param	regLow the function address register (ex. SEESAW_GPIO_BULK_SET)
 *  @param	buf the buffer the the bytes from
 *  @param	num the number of bytes to write.
 ****************************************************************************************/
 /*
void Write(byte regHigh, byte regLow, byte[] buf, byte num)
{
    _i2cbus->beginTransmission((byte)_i2caddr);
    _i2cbus->Write((byte)regHigh);
    _i2cbus->Write((byte)regLow);
    _i2cbus->Write((byte[])buf, num);
# ifdef SEESAW_I2C_DEBUG
    Serial.print("I2C write $");
    Serial.print((ushort)regHigh << 8 | regLow, HEX);
    Serial.print(" : ");
    for (int i = 0; i < num; i++)
    {
        Serial.print("0x");
        Serial.print(buf[i], HEX);
        Serial.print(",");
    }
    Serial.println();
#endif

    if (_flow != -1)
        while (!::digitalRead(_flow))
            ;
    _i2cbus->endTransmission();
}

        */

    }
}

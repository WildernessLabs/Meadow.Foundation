using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents PCA9685 IC
    /// </summary>
    /// <remarks>All PWM channels run at the same Frequency</remarks>
    public partial class Pca9685
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte I2cDefaultAddress => (byte)Address.Default;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        private readonly Frequency frequency;

        const byte Mode1 = 0x00;
        const byte Mode2 = 0x01;
        const byte SubAdr1 = 0x02;
        const byte SubAdr2 = 0x03;
        const byte SubAdr3 = 0x04;
        const byte PreScale = 0xFE;
        const byte Led0OnL = 0x06;
        const byte Led0OnH = 0x07;
        const byte Led0OffL = 0x08;
        const byte Led0OffH = 0x09;
        const byte AllLedOnL = 0xFA;
        const byte AllLedOnH = 0xFB;
        const byte AllLedOffL = 0xFC;
        const byte AllLedOffH = 0xFD;

        //# Bits
        const byte restart = 0x80;
        const byte sleep = 0x10;
        const byte allCall = 0x01;
        const byte invert = 0x10;
        const byte outDrv = 0x04;
        const byte mode1AI = 0x21;

        /// <summary>
        /// The I2C bus connected to the pca9685
        /// </summary>
        protected II2cBus i2CBus { get; set; }

        readonly byte address;

        /// <summary>
        /// Create a new Pca9685 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the peripheral</param>
        /// <param name="frequency">The frequency</param>
        /// <param name="address">The I2C address</param>
        public Pca9685(II2cBus i2cBus, Frequency frequency, byte address = (byte)Address.Default)
        {
            i2CBus = i2cBus;
            this.address = address;
            i2cComms = new I2cCommunications(i2CBus, address);
            this.frequency = frequency;
        }

        /// <summary>
        /// Create a new Pca9685 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the peripheral</param>
        /// <param name="address">The I2C address</param>
        public Pca9685(II2cBus i2cBus, byte address = (byte)Address.Default)
        : this(i2cBus, new Frequency(IPwmOutputController.DefaultPwmFrequency, Frequency.UnitType.Hertz), address)
        { }

        /// <summary>
        /// Initializes the PCA9685 IC
        /// </summary>
        public virtual void Initialize()
        {
            i2CBus.Write(address, new byte[] { Mode1, 0x00 });
            i2CBus.Write(address, new byte[] { Mode1 });

            Thread.Sleep(5);

            SetFrequency(frequency);

            for (byte i = 0; i < 16; i++)
            {
                SetPwm(i, 0, 0);
            }
        }

        /// <summary>
        /// Create a IPwmPort on the specified pin
        /// </summary>
        /// <param name="portNumber">The port number (0-15)</param>
        /// <param name="dutyCycle">The duty cycle for that port</param>
        /// <returns>IPwmPort</returns>
        public virtual IPwmPort CreatePwmPort(byte portNumber, float dutyCycle = 0.5f)
        {
            if (portNumber < 0 || portNumber > 15)
            {
                throw new ArgumentException("Value must be between 0 and 15", "portNumber");
            }

            var pwmPort = new PwmPort(i2CBus, address, Led0OnL, frequency, portNumber, dutyCycle);

            return pwmPort;
        }

        /// <summary>
        /// Turns the specified pin On or Off
        /// </summary>
        /// <param name="pin">The pin to set</param>
        /// <param name="on">true is on, false if off</param>
        public virtual void SetPin(byte pin, bool on)
        {
            if ((pin < 0) || (pin > 15))
            {
                throw new ArgumentException("PWM pin must be between 0 and 15");
            }

            if (on)
            {
                SetPwm(pin, 4096, 0);
            }
            else
            {
                SetPwm(pin, 0, 0);
            }
        }

        /// <summary>
        /// Set the values for specified output pin.
        /// </summary>
        /// <param name="pin">The pwm Pin</param>
        /// <param name="on">LED{X}_ON_L and LED{X}_ON_H registier value</param>
        /// <param name="off">LED{X}_OFF_L and LED{X}_OFF_H registier value</param>
        /// <remarks>On parameter is an inverted pwm signal</remarks>
        public virtual void SetPwm(byte pin, int on, int off)
        {
            if (pin < 0 || pin > 15)
            {
                throw new ArgumentException("Value has to be between 0 and 15", "port");
            }

            if (on < 0 || on > 4096)
            {
                throw new ArgumentException("Value has to be between 0 and 4096", "on");
            }

            if (off < 0 || off > 4096)
            {
                throw new ArgumentException("Value has to be between 0 and 4096", "off");
            }

            Write((byte)(Led0OnL + (4 * pin)), (byte)(on & 0xFF), (byte)(on >> 8), (byte)(off & 0xFF), (byte)(off >> 8));
        }

        void Write(byte register, byte ledXOnL, byte ledXOnH, byte ledXOffL, byte ledXOffH)
        {
            i2CBus.Write(address, new byte[] { register, ledXOnL, ledXOnH, ledXOffL, ledXOffH });
        }

        void Write(byte register, byte value)
        {
            i2cComms.WriteRegister(register, value);
        }

        void SetFrequency(Frequency frequency)
        {
            double prescaleval = 25000000.0;  //  # 25MHz
            prescaleval /= 4096.0;       //# 12-bit
            prescaleval /= frequency.Hertz;
            prescaleval -= 1.0;

            double prescale = Math.Floor(prescaleval + 0.5);
            byte oldmode = i2cComms.ReadRegister(Mode1);
            byte newmode = (byte)((oldmode & 0x7F) | 0x10);         //   # sleep

            Write(Mode1, newmode);//       # go to sleep
            Write(PreScale, (byte)((int)(Math.Floor(prescale))));
            Write(Mode1, oldmode);

            Thread.Sleep(5);
            Write(Mode1, (byte)(oldmode | 0x80 | mode1AI));
        }
    }
}
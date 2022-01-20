using Meadow.Hardware;
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
        private readonly II2cPeripheral i2cPeripheral;
        private readonly int frequency;

        //# Registers/etc.
        protected const byte Mode1 = 0x00;
        protected const byte Mode2 = 0x01;
        protected const byte SubAdr1 = 0x02;
        protected const byte SubAdr2 = 0x03;
        protected const byte SubAdr3 = 0x04;
        protected const byte PreScale = 0xFE;
        protected const byte Led0OnL = 0x06;
        protected const byte Led0OnH = 0x07;
        protected const byte Led0OffL = 0x08;
        protected const byte Led0OffH = 0x09;
        protected const byte AllLedOnL = 0xFA;
        protected const byte AllLedOnH = 0xFB;
        protected const byte AllLedOffL = 0xFC;
        protected const byte AllLedOffH = 0xFD;

        //# Bits
        protected const byte restart = 0x80;
        protected const byte sleep = 0x10;
        protected const byte allCall = 0x01;
        protected const byte invert = 0x10;
        protected const byte outDrv = 0x04;
        protected const byte mode1AI = 0x21;

        public II2cBus i2CBus { get; protected set; }
        byte address;

        public Pca9685(II2cBus i2cBus, byte address = (byte)Addresses.Default, int frequency = 100)
        {
            i2CBus = i2cBus;
            this.address = address;
            i2cPeripheral = new I2cPeripheral(i2CBus, address);
            this.frequency = frequency;
        }

        /// <summary>
        /// Initializes the PCA9685 IC
        /// </summary>
        public virtual void Initialize()
        {
            i2CBus.WriteData(address, Mode1, 0X00);
            i2CBus.WriteData(address, Mode1);

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
        /// <param name="on">LED{X}_ON_L & LED{X}_ON_H registier value</param>
        /// <param name="off">LED{X}_OFF_L  & LED{X}_OFF_H registier value</param>
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

        protected virtual void Write(byte register, byte ledXOnL, byte ledXOnH, byte ledXOffL, byte ledXOffH)
        {
            i2CBus.WriteData(address, register, ledXOnL, ledXOnH, ledXOffL, ledXOffH);
        }

        protected virtual void Write(byte register, byte value)
        {
            i2cPeripheral.WriteRegister(register, value);
        }

        protected virtual void SetFrequency(int frequency)
        {
            double prescaleval = 25000000.0;  //  # 25MHz
            prescaleval = prescaleval / 4096.0;       //# 12-bit
            prescaleval = prescaleval / frequency;
            prescaleval -= 1.0;

            double prescale = Math.Floor(prescaleval + 0.5);
            byte oldmode = i2cPeripheral.ReadRegister(Mode1);
            byte newmode = (byte)((oldmode & 0x7F) | 0x10);         //   # sleep

            Write(Mode1, newmode);//       # go to sleep
            Write(PreScale, (byte)((int)(Math.Floor(prescale))));
            Write(Mode1, oldmode);

            Thread.Sleep(5);
            Write(Mode1, (byte)(oldmode | 0x80 | mode1AI));
        }
    }
}
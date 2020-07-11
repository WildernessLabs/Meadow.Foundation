using Meadow.Foundation.ICs;
using Meadow.Foundation.ICs.IOExpanders;
using System;


namespace Meadow.Foundation.FeatherWings
{
    public enum Commmand
    {
        FORWARD,
        BACKWARD,
        RELEASE
    }

    public class DCMotor : Motor
    {
        readonly byte _pwmPin;
        readonly byte _in1;
        readonly byte _in2;


        public DCMotor(short num, Pca9685 pca9685) : base(pca9685)
        {

            if (num < 0 || num > 3)
            {
                throw new ArgumentException("Motor must be between 0 and 3");
            }

            switch (num)
            {
                case 0:
                    _pwmPin = 8;
                    _in2 = 9;
                    _in1 = 10;
                    break;
                case 1:
                    _pwmPin = 13;
                    _in2 = 12;
                    _in1 = 11;
                    break;
                case 2:
                    _pwmPin = 2;
                    _in2 = 3;
                    _in1 = 4;
                    break;
                case 3:
                    _pwmPin = 7;
                    _in2 = 6;
                    _in1 = 5;
                    break;

            }

            Run(Commmand.RELEASE);

        }

        /// <summary>
        /// Controls the motor direction/action
        /// </summary>
        /// <param name="command">The action</param>
        public virtual void Run(Commmand command)
        {
            if (command == Commmand.FORWARD)
            {
                _pca9685.SetPin(_in2, false);
                _pca9685.SetPin(_in1, true);
            }

            if (command == Commmand.BACKWARD)
            {
                _pca9685.SetPin(_in2, true);
                _pca9685.SetPin(_in1, false);
            }

            if (command == Commmand.RELEASE)
            {
                _pca9685.SetPin(_in1, false);
                _pca9685.SetPin(_in2, false);
            }
        }

        /// <summary>
        /// Control the DC Motor speed/throttle
        /// </summary>
        /// <param name="speed">The 8-bit PWM value, 0 is off, 255 is on</param>
        public override void SetSpeed(short speed)
        {
            if (speed < 0)
            {
                speed = 0;
            }

            if (speed > 255)
            {
                speed = 255;
            }

            _pca9685.SetPwm(_pwmPin, 0, speed * 16);
        }

        /// <summary>
        /// Control the DC Motor speed/throttle
        /// </summary>
        /// <param name="speed">The 12-bit PWM value, 0 is off, 4096 is on</param>
        public void PreciseSpeed(short speed)
        {
            if (speed > 4096)
                speed = 4096;

            if (speed < 0)
                speed = 0;

            _pca9685.SetPwm(_pwmPin, 0, speed);
        }

        public void Stop()
        {
            Run(Commmand.RELEASE);
            _pca9685.SetPwm(_pwmPin, 0, 0);
        }
    }
}

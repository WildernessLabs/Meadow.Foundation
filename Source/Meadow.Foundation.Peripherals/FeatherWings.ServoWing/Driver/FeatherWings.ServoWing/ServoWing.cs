using Meadow.Foundation.ICs;
using Meadow.Foundation.Servos;
using Meadow.Hardware;
using System;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents Adafruit's Feather Servo Wing and 16-Channel 12-bit PWM/Servo Shield
    /// </summary>
    /// <remarks>All PWM channels run at the same Frequency</remarks>
    public class ServoWing 
    {
        readonly short ports;
        Pca9685 pca9685;
        
        public ServoWing(II2cBus i2cBus, byte address = 0x40, int frequency = 50, short ports = 8)
        {
            if (ports != 8 && ports != 16)
            {
                throw new ArgumentException("Channels need to be 8 or 16", "ports");
            }

            this.ports = ports;
            pca9685 = new Pca9685(i2cBus, address, frequency);
        }

        public void Initialize()
        {
            pca9685.Initialize();
        }

        public Servo GetServo(byte num, ServoConfig servoConfig)
        {
            if ((num < 0) || (num > ports))
            {
                throw new ArgumentException($"Servo num must be between 1 and {ports}", "num");
            }

            var pwm = pca9685.CreatePwmPort(num);
            var servo = new Servo(pwm, servoConfig);

            return servo;
        }

        public IContinuousRotationServo GetContinuousRotatioServo(byte num, ServoConfig servoConfig)
        {
            if ((num < 0) || (num > ports))
            {
                throw new ArgumentException($"Continuous Rotatio Servo num must be between 1 and {ports}", "num");
            }

            var pwm = pca9685.CreatePwmPort(num);
            var servo = new ContinuousRotationServo(pwm, servoConfig);

            return servo;
        }
    }
}
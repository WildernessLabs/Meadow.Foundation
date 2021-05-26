using System;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Motors;

namespace Meadow.Foundation.Motors
{
    /// <summary>
    /// Generic h-bridge motor controller.
    /// </summary>
    public class HBridgeMotor : IDCMotor
    {
        protected IPwmPort motorLeftPwm; // H-Bridge 1A pin
        protected IPwmPort motorRighPwm; // H-Bridge 2A pin
        protected IDigitalOutputPort enablePort; // if enabled, then IsNeutral = false

        /// <summary>
        /// When true, the wheels spin "freely"
        /// </summary>
        public bool IsNeutral
        {
            get => isNeutral;
            set
            {
                isNeutral = value;
                // if neutral, we disable the port
                enablePort.State = !isNeutral;
            }
        }
        protected bool isNeutral = true;

        /// <summary>
        /// The power applied to the motor, as a percentage between
        /// `-1.0` and `1.0`.
        /// </summary>
        public float Power {
            get => power;
            set {
                motorLeftPwm.Stop();
                motorRighPwm.Stop();

                power = value;

                var calibratedSpeed = power * MotorCalibrationMultiplier;
                var absoluteSpeed = Math.Min(Math.Abs(calibratedSpeed), 1);
                var isForward = calibratedSpeed > 0;

                motorLeftPwm.DutyCycle = (isForward) ? absoluteSpeed : 0;
                motorRighPwm.DutyCycle = (isForward) ? 0 : absoluteSpeed;
                IsNeutral = false;

                motorLeftPwm.Start();
                motorRighPwm.Start();
            }
        }
        protected float power = 0;

        /// <summary>
        /// Obsolete, please use `Power`.
        /// </summary>
        [Obsolete]
        public float Speed
        {
            get => Power;
            set { Power = value; }
        }

        /// <summary>
        /// The frequency of the PWM used to drive the motors. 
        /// Default value is 1600.
        /// </summary>
        public float PwmFrequency
        {
            get => pwmFrequency;
        }
        protected readonly float pwmFrequency;

        /// <summary>
        /// Not all motors are created equally. This number scales the Speed Input so
        /// that you can match motor speeds without changing your logic.
        /// </summary>
        public float MotorCalibrationMultiplier { get; set; } = 1;

        // TODO: this convenience constructor is weird. we create the PWM but
        // not the digital output port. i think if we're going to have a convenience
        // constructor it should be:
        public HBridgeMotor(IMeadowDevice device, IPin a1Pin, IPin a2Pin, IPin enablePin, float pwmFrequency = 1600) :
            this(device.CreatePwmPort(a1Pin), device.CreatePwmPort(a2Pin), device.CreateDigitalOutputPort(enablePin), pwmFrequency)
        { }
        // and we should [obsolete] or delete this one:
        public HBridgeMotor(IPwmOutputController device, IPin a1Pin, IPin a2Pin, IDigitalOutputPort enablePin, float pwmFrequency = 1600) :
            this(device.CreatePwmPort(a1Pin), device.CreatePwmPort(a2Pin), enablePin, pwmFrequency)
        { }

        public HBridgeMotor(IPwmPort a1Pin, IPwmPort a2Pin, IDigitalOutputPort enablePin, float pwmFrequency = 1600)
        {
            this.pwmFrequency = pwmFrequency;

            motorLeftPwm = a1Pin;
            motorLeftPwm.Frequency = 1600;
            motorLeftPwm.Start();

            motorRighPwm = a2Pin;
            motorRighPwm.Frequency = 1600;
            motorRighPwm.Start();

            enablePort = enablePin;
        }
    }
}
using Meadow.Hardware;
using Meadow.Peripherals.Motors;
using System;

namespace Meadow.Foundation.Motors
{
    /// <summary>
    /// Generic h-bridge motor controller.
    /// </summary>
    public class HBridgeMotor : IDCMotor
    {
        protected IPwmPort _motorLeftPwm = null; // H-Bridge 1A pin
        protected IPwmPort _motorRighPwm = null; // H-Bridge 2A pin
        protected IDigitalOutputPort _enablePort = null; // if enabled, then IsNeutral = false

        /// <summary>
        /// When true, the wheels spin "freely"
        /// </summary>
        public bool IsNeutral
        {
            get => _isNeutral;
            set
            {
                _isNeutral = value;
                // if neutral, we disable the port
                _enablePort.State = !_isNeutral;
            }
        }
        protected bool _isNeutral = true;

        /// <summary>
        /// 0 - 1 for the speed.
        /// </summary>
        public float Speed
        {
            get => _speed;
            set
            {
                _motorLeftPwm.Stop();
                _motorRighPwm.Stop();

                _speed = value;

                var calibratedSpeed = _speed * MotorCalibrationMultiplier;
                var absoluteSpeed = Math.Min(Math.Abs(calibratedSpeed), 1);
                var isForward = calibratedSpeed > 0;

                _motorLeftPwm.DutyCycle = (isForward) ? absoluteSpeed : 0;
                _motorRighPwm.DutyCycle = (isForward) ? 0 : absoluteSpeed;
                IsNeutral = false;

                _motorLeftPwm.Start();
                _motorRighPwm.Start();
            }
        }
        protected float _speed = 0;

        /// <summary>
        /// The frequency of the PWM used to drive the motors. 
        /// Default value is 1600.
        /// </summary>
        public float PwmFrequency
        {
            get => _pwmFrequency;
        }
        protected readonly float _pwmFrequency;

        /// <summary>
        /// Not all motors are created equally. This number scales the Speed Input so
        /// that you can match motor speeds without changing your logic.
        /// </summary>
        public float MotorCalibrationMultiplier { get; set; } = 1;

        public HBridgeMotor(IIODevice device, IPin a1Pin, IPin a2Pin, IDigitalOutputPort enablePin, float pwmFrequency = 1600) :
            this(device.CreatePwmPort(a1Pin), device.CreatePwmPort(a2Pin), enablePin, pwmFrequency)
        { }

        public HBridgeMotor(IPwmPort a1Pin, IPwmPort a2Pin, IDigitalOutputPort enablePin, float pwmFrequency = 1600)
        {
            _pwmFrequency = pwmFrequency;

            _motorLeftPwm = a1Pin;
            _motorLeftPwm.Frequency = 1600;
            _motorLeftPwm.Start();

            _motorRighPwm = a2Pin;
            _motorRighPwm.Frequency = 1600;
            _motorRighPwm.Start();

            _enablePort = enablePin;
        }
    }
}
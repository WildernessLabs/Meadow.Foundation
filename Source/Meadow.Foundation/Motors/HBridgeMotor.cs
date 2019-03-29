using Meadow.Hardware;
using Meadow.Peripherals.Motors;
using System;

namespace Meadow.Foundation.Motors
{
    /// <summary>
    /// Generic h-bridge motor controller.
    /// 
    /// Note: this class is not yet implemented.
    /// </summary>
    public class HBridgeMotor : IDCMotor
    {
        /// <summary>
        /// 0 - 1 for the speed.
        /// </summary>
        public float Speed {
            get { return _speed; }
            set
            {
                _speed = value;
                
                var calibratedSpeed = _speed * MotorCalibrationMultiplier;
                var absoluteSpeed = Math.Min(Math.Abs(calibratedSpeed), 1);
                var isForward = calibratedSpeed > 0;

                //Console.WriteLine("calibrated speed: " + calibratedSpeed.ToString() + ", isForward: " + isForward.ToString());

                // set speed. if forward, disable right pwm. otherwise disable left
                _motorLeftPwm.DutyCycle = (isForward) ? absoluteSpeed : 0;
                _motorRighPwm.DutyCycle = (isForward) ? 0 : absoluteSpeed;

                // clear our neutral to enable
                this.IsNeutral = false;

                _motorLeftPwm.Start();
                _motorRighPwm.Start();
            }
        }
        protected float _speed = 0;

        /// <summary>
        /// Not all motors are created equally. This number scales the Speed Input so
        /// that you can match motor speeds without changing your logic.
        /// </summary>
        public float MotorCalibrationMultiplier { get; set; } = 1;

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
        } protected bool _isNeutral = true;

        /// <summary>
        /// The frequency of the PWM used to drive the motors. 
        /// Default value is 1600.
        /// </summary>
        public float PwmFrequency
        {
            get { return _pwmFrequency; }
        } protected readonly float _pwmFrequency;

        protected IPwmPort _motorLeftPwm = null; // H-Bridge 1A pin
        protected IPwmPort _motorRighPwm = null; // H-Bridge 2A pin
        protected DigitalOutputPort _enablePort = null; // if enabled, then IsNeutral = false

 
		// TODO: change constructor to new pattern
        //public HBridgeMotor(Cpu.PWMChannel a1Pin, Cpu.PWMChannel a2Pin, Pins enablePin, float pwmFrequency = 1600)
        //{
        //    _pwmFrequency = pwmFrequency;
        //    // create our PWM outputs
        //    _motorLeftPwm = new PWM(a1Pin, _pwmFrequency, 0, false);
        //    _motorRighPwm = new PWM(a2Pin, _pwmFrequency, 0, false);
        //    _enablePort = new DigitalOutputPort(enablePin, false);
        //}
    }
}

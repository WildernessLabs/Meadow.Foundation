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
        /// <summary>
        /// PWM port for left motor
        /// </summary>
        protected IPwmPort motorLeftPwm; // H-Bridge 1A pin
        /// <summary>
        /// PWM port for right motor
        /// </summary>
        protected IPwmPort motorRighPwm; // H-Bridge 2A pin
        /// <summary>
        /// Digital output port to enable h-bridge
        /// </summary>
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
        bool isNeutral = true;

        /// <summary>
        /// The power applied to the motor, as a percentage between
        /// `-1.0` and `1.0`.
        /// </summary>
        public float Power 
        {
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
        float power = 0;

        /// <summary>
        /// Obsolete, please use `Power`.
        /// </summary>
        [Obsolete("Use Power property")]
        public float Speed
        {
            get => Power;
            set { Power = value; }
        }

        /// <summary>
        /// The frequency of the PWM used to drive the motors. 
        /// Default value is 1600.
        /// </summary>
        public float PwmFrequency => motorLeftPwm.Frequency;

        /// <summary>
        /// Not all motors are created equally. This number scales the Speed Input so
        /// that you can match motor speeds without changing your logic.
        /// </summary>
        public float MotorCalibrationMultiplier { get; set; } = 1;

        /// <summary>
        /// Create an HBridgeMotor object
        /// </summary>
        /// <param name="device"></param>
        /// <param name="a1Pin"></param>
        /// <param name="a2Pin"></param>
        /// <param name="enablePin"></param>
        /// <param name="pwmFrequency"></param>
        public HBridgeMotor(IMeadowDevice device, IPin a1Pin, IPin a2Pin, IPin enablePin, float pwmFrequency = 1600) :
            this(device.CreatePwmPort(a1Pin), device.CreatePwmPort(a2Pin), device.CreateDigitalOutputPort(enablePin), pwmFrequency)
        { }

        /// <summary>
        /// Create an HBridgeMotor object
        /// </summary>
        /// <param name="a1Port"></param>
        /// <param name="a2Port"></param>
        /// <param name="enablePort"></param>
        /// <param name="pwmFrequency"></param>

        public HBridgeMotor(IPwmPort a1Port, IPwmPort a2Port, IDigitalOutputPort enablePort, float pwmFrequency = 1600)
        {
            motorLeftPwm = a1Port;
            motorLeftPwm.Frequency = pwmFrequency;
            motorLeftPwm.Start();

            motorRighPwm = a2Port;
            motorRighPwm.Frequency = pwmFrequency;
            motorRighPwm.Start();

            this.enablePort = enablePort;
        }
    }
}
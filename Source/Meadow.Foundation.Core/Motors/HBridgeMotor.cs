using System;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Motors;
using Meadow.Units;

namespace Meadow.Foundation.Motors
{
    /// <summary>
    /// Generic h-bridge motor controller.
    /// </summary>
    public class HBridgeMotor : IDCMotor, IDisposable
    {
        static readonly Frequency DefaultFrequency = new Frequency(1600, Frequency.UnitType.Hertz);

        /// <summary>
        /// PWM port for left motor
        /// </summary>
        protected IPwmPort motorLeftPwm; // H-Bridge 1A pin
        /// <summary>
        /// PWM port for right motor
        /// </summary>
        protected IPwmPort motorRightPwm; // H-Bridge 2A pin
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
            set 
            {
                motorLeftPwm.Stop();
                motorRightPwm.Stop();

                power = value;

                var calibratedSpeed = power * MotorCalibrationMultiplier;
                var absoluteSpeed = Math.Min(Math.Abs(calibratedSpeed), 1);
                var isForward = calibratedSpeed > 0;

                motorLeftPwm.DutyCycle = (isForward) ? absoluteSpeed : 0;
                motorRightPwm.DutyCycle = (isForward) ? 0 : absoluteSpeed;
                IsNeutral = false;

                motorLeftPwm.Start();
                motorRightPwm.Start();
            }
        }
        float power = 0;

        /// <summary>
        /// The frequency of the PWM used to drive the motors. 
        /// Default value is 1600.
        /// </summary>
        public Frequency PwmFrequency => motorLeftPwm.Frequency;

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
        public HBridgeMotor(IMeadowDevice device, IPin a1Pin, IPin a2Pin, IPin enablePin) :
            this(device.CreatePwmPort(a1Pin, DefaultFrequency), device.CreatePwmPort(a2Pin, DefaultFrequency), device.CreateDigitalOutputPort(enablePin), DefaultFrequency)
        { }

        /// <summary>
        /// Create an HBridgeMotor object
        /// </summary>
        /// <param name="device"></param>
        /// <param name="a1Pin"></param>
        /// <param name="a2Pin"></param>
        /// <param name="enablePin"></param>
        /// <param name="pwmFrequency"></param>
        public HBridgeMotor(IMeadowDevice device, IPin a1Pin, IPin a2Pin, IPin enablePin, Frequency pwmFrequency) :
            this(device.CreatePwmPort(a1Pin, pwmFrequency), device.CreatePwmPort(a2Pin, pwmFrequency), device.CreateDigitalOutputPort(enablePin), pwmFrequency)
        { }

        /// <summary>
        /// Create an HBridgeMotor object
        /// </summary>
        /// <param name="a1Port"></param>
        /// <param name="a2Port"></param>
        /// <param name="enablePort"></param>
        public HBridgeMotor(IPwmPort a1Port, IPwmPort a2Port, IDigitalOutputPort enablePort)
            : this(a1Port, a2Port, enablePort, DefaultFrequency)
        {
        }

        /// <summary>
        /// Create an HBridgeMotor object
        /// </summary>
        /// <param name="a1Port"></param>
        /// <param name="a2Port"></param>
        /// <param name="enablePort"></param>
        /// <param name="pwmFrequency"></param>
        public HBridgeMotor(IPwmPort a1Port, IPwmPort a2Port, IDigitalOutputPort enablePort, Frequency pwmFrequency)
        {
            motorLeftPwm = a1Port;
            motorLeftPwm.Frequency = pwmFrequency;
            motorLeftPwm.Start();

            motorRightPwm = a2Port;
            motorRightPwm.Frequency = pwmFrequency;
            motorRightPwm.Start();

            this.enablePort = enablePort;
        }

        /// <summary>
		/// Dispose peripheral
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                motorLeftPwm.Dispose();
                motorRightPwm.Dispose();
            }
        }

        /// <summary>
        /// Dispose Peripheral
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
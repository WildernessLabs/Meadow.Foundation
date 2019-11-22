using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// Base class implementation for servos that can rotate continuously.
    /// </summary>
    public abstract class ContinuousRotationServoBase : IContinuousRotationServo
    {
        protected IPwmPort _pwm = null;

        /// <summary>
        /// Gets the ServoConfig that describes this servo.
        /// </summary>
        public ServoConfig Config
        {
            get { return _config; }
        }
        protected ServoConfig _config = null;

        /// <summary>
        /// Gets the current rotation direction.
        /// </summary>
        public RotationDirection CurrentDirection
        {
            get { return _currentDirection; }
        }
        protected RotationDirection _currentDirection = RotationDirection.None;

        /// <summary>
        /// Gets the current rotation speed.
        /// </summary>
        public float CurrentSpeed
        {
            get { return _currentSpeed; }
        }
        protected float _currentSpeed = -1;

        /// <summary>
        /// Instantiates a new continuous rotation servo on the specified pin, with the specified configuration.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="config"></param>
        public ContinuousRotationServoBase(IPwmPort pwm, ServoConfig config)
        {
            _config = config;
            // OLD
			//_pwm = new PWM(pin, config.Frequency, 0, false);
            // NEW
            _pwm = pwm;
            _pwm.Frequency = config.Frequency;
            _pwm.DutyCycle = 0;

        }

        /// <summary>
        /// Starts rotating the servo in the specified direction, at the specified speed.
        /// </summary>
        /// <param name="direction">Clockwise/counterclockwise.</param>
        /// <param name="speed">0.0 to 1.0 (0% to 100%).</param>
        public void Rotate(RotationDirection direction, float speed)
        {
            if (speed < 0 || speed > 1)
            {
                throw new ArgumentOutOfRangeException("speed", "speed must be 0.0 - 1.0.");
            }

            // calculate the appropriate pulse duration for the speed and direction
            float pulseDuration = CalculatePulseDuration(direction, speed);
            //Console.WriteLine("Pulse Duration: " + pulseDuration.ToString());

            // send our pulse to the servo to make it move
            SendCommandPulse(pulseDuration);

            // update state
            _currentDirection = direction;
            _currentSpeed = speed;
        }

        /// <summary>
        /// Stops rotation of the servo.
        /// </summary>
        public void Stop()
        {
            _pwm.Stop();
            _currentDirection = RotationDirection.None;
            _currentSpeed = 0.0f;
        }

        /// <summary>
        /// Continuous rotation servos usually have a zero speed at their midpoint pulse 
        /// duration (between min and max). As you lower the duration from midpoint, they 
        /// rotate clockwise and rotate their fastest at the minimum pulse duration. As 
        /// you increase the pulse duration, they rotate counter-clockwise.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        protected float CalculatePulseDuration(RotationDirection direction, float speed)
        {
            // calculate the midpoint/neutral/stop
            int midpointPulseDuration = _config.MinimumPulseDuration + ((_config.MaximumPulseDuration - _config.MinimumPulseDuration) / 2);
            // the delta is how fast; speed * (max - midpoint)
            int midPointPulseDelta = (int)(speed * (float)(_config.MaximumPulseDuration - midpointPulseDuration));

            // calculate the pulse direction as less or more than midpoint
            int pulseDuration = midpointPulseDuration;
            if (direction == RotationDirection.Clockwise)
            {
                pulseDuration -= midPointPulseDelta;
            }
            else
            {
                pulseDuration += midPointPulseDelta;
            }

            return pulseDuration;
        }

        protected float CalculateDutyCycle(float pulseDuration)
        {
            // the pulse duration is dependent on the frequency we're driving the servo at
            return pulseDuration / ((1.0f / (float)_config.Frequency) * 1000000f);
        }

        protected void SendCommandPulse(float pulseDuration)
        {
            //Console.WriteLine("Sending Command Pulse");
            _pwm.DutyCycle = CalculateDutyCycle(pulseDuration);
            _pwm.Start(); // servo expects to run continuously
        }

    }
}

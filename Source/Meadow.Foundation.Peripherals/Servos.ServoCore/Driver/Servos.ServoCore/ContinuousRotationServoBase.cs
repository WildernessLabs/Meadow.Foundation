using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// Base class implementation for servos that can rotate continuously.
    /// </summary>
    public abstract class ContinuousRotationServoBase : ServoBase, IContinuousRotationServo
    {
        /// <summary>
        /// Gets the current rotation direction.
        /// </summary>
        public RotationDirection CurrentDirection { get; protected set; } = RotationDirection.None;

        /// <summary>
        /// Gets the current rotation speed.
        /// </summary>
        public float CurrentSpeed { get; protected set; } = -1;

        /// <summary>
        /// Instantiates a new continuous rotation servo on the specified pin, with the specified configuration.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="config"></param>
        public ContinuousRotationServoBase(IPwmPort pwm, ServoConfig config)
            : base(pwm, config)
        {
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

            // send our pulse to the servo to make it move
            SendCommandPulse(pulseDuration);

            // update state
            CurrentDirection = direction;
            CurrentSpeed = speed;
        }

        /// <summary>
        /// Stops rotation of the servo.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            CurrentDirection = RotationDirection.None;
            CurrentSpeed = 0.0f;
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
            int midpointPulseDuration = Config.MinimumPulseDuration + ((Config.MaximumPulseDuration - Config.MinimumPulseDuration) / 2);
            // the delta is how fast; speed * (max - midpoint)
            int midPointPulseDelta = (int)(speed * (float)(Config.MaximumPulseDuration - midpointPulseDuration));

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

        protected override void SendCommandPulse(float pulseDuration)
        {
            base.SendCommandPulse(pulseDuration);
            PwmPort.Start(); // servo expects to run continuously
        }

    }
}

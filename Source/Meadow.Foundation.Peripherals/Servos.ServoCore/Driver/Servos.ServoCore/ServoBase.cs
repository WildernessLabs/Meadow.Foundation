using Meadow.Hardware;

namespace Meadow.Foundation.Servos
{
    public abstract class ServoBase : IServo
    {
        /// <summary>
        /// Gets the PWM port used to drive the Servo
        /// </summary>
        protected IPwmPort PwmPort { get; }

        /// <summary>
        /// Gets the ServoConfig that describes this servo.
        /// </summary>
        public ServoConfig Config { get; protected set; }

        protected ServoBase(IPwmPort pwm, ServoConfig config)
        {
            Config = config;

            PwmPort = pwm;
            PwmPort.Frequency = config.Frequency;
            PwmPort.DutyCycle = 0;
        }

        public virtual void Stop()
        {
            PwmPort.Stop();
        }

        /// <summary>
        /// Note that this calculation expects a pulse duration in _microseconds_.
        /// </summary>
        /// <param name="pulseDuration">Microseconds</param>
        /// <returns></returns>
        protected float CalculateDutyCycle(float pulseDuration)
        {
            // the pulse duration is dependent on the frequency we're driving the servo at
            return pulseDuration / ((1.0f / (float)Config.Frequency) * 1000000f);
        }

        protected virtual void SendCommandPulse(float pulseDuration)
        {
            PwmPort.DutyCycle = CalculateDutyCycle(pulseDuration);
        }
    }
}
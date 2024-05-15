using Meadow.Hardware;

namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// Servo base class
    /// </summary>
    public abstract class ServoBase : IServo
    {
        /// <summary>
        /// Gets the PWM port used to drive the servo
        /// </summary>
        protected IPwmPort PwmPort { get; }

        /// <summary>
        /// Gets the ServoConfig that describes this servo
        /// </summary>
        public ServoConfig Config { get; protected set; }

        /// <summary>
        /// Create a new ServoBase object
        /// </summary>
        /// <param name="pwmPort">PWM port</param>
        /// <param name="config">Servo configuration</param>
        protected ServoBase(IPwmPort pwmPort, ServoConfig config)
        {
            Config = config;

            PwmPort = pwmPort;
            PwmPort.Frequency = config.Frequency;
            PwmPort.DutyCycle = 0;
        }

        /// <summary>
        /// Stop the servo
        /// </summary>
        public virtual void Stop()
        {
            PwmPort.Stop();
        }

        /// <summary>
        /// Note that this calculation expects a pulse duration in microseconds
        /// </summary>
        /// <param name="pulseDuration">Microseconds</param>
        /// <returns></returns>
        protected float CalculateDutyCycle(float pulseDuration)
        {
            // the pulse duration is dependent on the frequency we're driving the servo at
            return pulseDuration / ((1.0f / (float)Config.Frequency.Hertz) * 1000000f);
        }

        /// <summary>
        /// Send a command pulse
        /// </summary>
        /// <param name="pulseDuration">The pulse duration</param>
        protected virtual void SendCommandPulse(float pulseDuration)
        {
            PwmPort.DutyCycle = CalculateDutyCycle(pulseDuration);
        }
    }
}
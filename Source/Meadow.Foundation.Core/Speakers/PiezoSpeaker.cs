using Meadow.Hardware;
using Meadow.Peripherals.Speakers;
using System.Threading.Tasks;

namespace Meadow.Foundation.Audio
{
    /// <summary>
    /// Represents a 2 pin piezo-electric speaker capable of generating tones
    /// </summary>
    public class PiezoSpeaker : IToneGenerator
    {
        /// <summary>
        /// Gets the port that is driving the Piezo Speaker
        /// </summary>
        public IPwmPort Port { get; protected set; }

        private bool isPlaying = false;

        /// <summary>
        /// Create a new PiezoSpeaker instance
        /// </summary>
        /// <param name="device">IPwmOutputController to create PWM port</param>
        /// <param name="pin">PWM Pin connected to the PiezoSpeaker</param>
        /// <param name="frequency">PWM frequency</param>
        /// <param name="dutyCycle">Duty cycle</param>
        public PiezoSpeaker(IPwmOutputController device, IPin pin, float frequency = 100, float dutyCycle = 0) :
            this (device.CreatePwmPort(pin, frequency, dutyCycle)) { }

        /// <summary>
        /// Create a new PiezoSpeaker instance
        /// </summary>
        /// <param name="port"></param>
        public PiezoSpeaker(IPwmPort port)
        {
            Port = port;
            Port.Start();
        }

        /// <summary>
        /// Play a frequency for a specified duration
        /// </summary>
        /// <param name="frequency">The frequency in hertz of the tone to be played</param>
        /// <param name="duration">How long the note is played in milliseconds, if durration is 0, tone plays indefinitely</param>
        public async Task PlayTone(float frequency, int duration = 0)
        {
            if (frequency <= 1)
            {
                throw new System.Exception("Piezo frequency must be greater than 1");
            }

            if (!isPlaying)
            {
                isPlaying = true;

                Port.Frequency = frequency;
                Port.DutyCycle = 0.5f;

                if (duration > 0)
                {
                    await Task.Delay(duration);
                    Port.DutyCycle = 0f;
                }

                isPlaying = false;
            }
        }

        /// <summary>
        /// Stops a tone playing
        /// </summary>
        public void StopTone()
        {
            Port.DutyCycle = 0f;
        }
    }
}
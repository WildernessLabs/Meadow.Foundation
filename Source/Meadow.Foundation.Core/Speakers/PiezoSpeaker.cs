using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Speakers;

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

        private bool _isPlaying = false;

        /// <summary>
        /// Create a new PiezoSpeaker instance
        /// </summary>
        /// <param name="pin">PWM Pin connected to the PiezoSpeaker</param>
        public PiezoSpeaker(IIODevice device, IPin pin, float frequency = 100, float dutyCycle = 0) :
            this (device.CreatePwmPort(pin, frequency, dutyCycle)) { }

        /// <summary>
        /// Create a new PiezoSpeaker instance
        /// </summary>
        /// <param name="port"></param>
        public PiezoSpeaker(IPwmPort port)
        {
            Port = port;
        }

        /// <summary>
        /// Play a frequency for a specified duration
        /// </summary>
        /// <param name="frequency">The frequency in hertz of the tone to be played</param>
        /// <param name="duration">How long the note is played in milliseconds, if durration is 0, tone plays indefinitely</param>
        public void PlayTone(float frequency, int duration = 0)
        {
            if (frequency <= 0)
                throw new System.Exception("frequency must be greater than 0");

            if (!_isPlaying)
            {
                _isPlaying = true;

                Port.Frequency = frequency;
                Port.DutyCycle = 0.5f;

                Port.Start();

                if (duration > 0)
                {
                    Thread.Sleep(duration);
                    Port.Stop();
                }

                _isPlaying = false;
            }
        }

        /// <summary>
        /// Stops a tone playing
        /// </summary>
        public void StopTone()
        {
            Port.Stop();
        }
    }
}
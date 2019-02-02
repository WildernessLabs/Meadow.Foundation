using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Speakers;

namespace Meadow.Foundation.Audio
{
    /// <summary>
    /// Represents a 2 pin piezo-electric speaker capable of generating tones
    /// 
    /// Note: This driver is not yet implemented
    /// </summary>
    public class PiezoSpeaker : IToneGenerator
    {
        private IPwmPort _pwm;
        private bool _isPlaying = false;

        /// <summary>
        /// Create a new PiezoSpeaker instance
        /// </summary>
        /// <param name="pin">PWM Pin connected to the PiezoSpeaker</param>
        public PiezoSpeaker(IPwmPin pin)
        {
            _pwm = new PwmPort(pin, 100, 0, false);
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

                var period = (uint)(1000000 / frequency);

                _pwm.Period = period;
                _pwm.Duration = period / 2;

                _pwm.Start();

                if (duration > 0)
                {
                    Thread.Sleep(duration);
                    _pwm.Stop();
                }

                _isPlaying = false;
            }
        }

        /// <summary>
        /// Stops a tone playing
        /// </summary>
        public void StopTone()
        {
            _pwm.Stop();
        }
    }
}
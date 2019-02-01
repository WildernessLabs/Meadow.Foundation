using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Audio
{
    public class PiezoSpeaker : IToneGenerator
    {
        private IPwmPort _pwm;
        private bool _isPlaying = false;

        public PiezoSpeaker(IPwmPin pin)
        {
            _pwm = new PwmPort(pwmChannel, 100, 0, false);
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

        public void StopTone()
        {
            _pwm.Stop();
        }
    }
}
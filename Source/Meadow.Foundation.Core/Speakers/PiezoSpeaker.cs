using Meadow.Hardware;
using Meadow.Peripherals.Speakers;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Audio
{
    /// <summary>
    /// Represents a 2 pin piezo-electric speaker capable of generating tones
    /// </summary>
    public class PiezoSpeaker : IToneGenerator, IDisposable
    {
        private bool isPlaying = false;

        /// <summary>
        /// Gets the port that is driving the Piezo Speaker
        /// </summary>
        protected IPwmPort PwmPort { get; set; }

        /// <summary>
        /// Track if we created the input port in the PushButton instance (true)
        /// or was it passed in via the ctor (false)
        /// </summary>
        protected bool ShouldDisposePort = false;

        /// <summary>
        /// Is the peripheral disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Create a new PiezoSpeaker instance
        /// </summary>
        /// <param name="device">IPwmOutputController to create PWM port</param>
        /// <param name="pwmPin">PWM Pin connected to the PiezoSpeaker</param>
        /// <param name="frequency">PWM frequency</param>
        /// <param name="dutyCycle">Duty cycle</param>
        public PiezoSpeaker(
            IPwmOutputController device, 
            IPin pwmPin, 
            Frequency frequency, 
            float dutyCycle = 0) 
            : this (
                device.CreatePwmPort(pwmPin, frequency, dutyCycle)) 
        {
            ShouldDisposePort = true;
        }

        /// <summary>
        /// Create a new PiezoSpeaker instance
        /// </summary>
        /// <param name="device">IPwmOutputController to create PWM port</param>
        /// <param name="pwmPin">PWM Pin connected to the PiezoSpeaker</param>
        public PiezoSpeaker(
            IPwmOutputController device, 
            IPin pwmPin)
            : this (
                device.CreatePwmPort(pwmPin, new Frequency(100, Frequency.UnitType.Hertz), 0))
        {
            ShouldDisposePort = true;
        }

        /// <summary>
        /// Create a new PiezoSpeaker instance
        /// </summary>
        /// <param name="port"></param>
        public PiezoSpeaker(IPwmPort port)
        {
            PwmPort = port;
            PwmPort.Start();
        }

        /// <summary>
        /// Play a frequency until stopped by StopTone
        /// </summary>
        /// <param name="frequency">The frequency in hertz of the tone to be played</param>
        public Task PlayTone(Frequency frequency)
        {
            return PlayTone(frequency, TimeSpan.Zero);
        }

        /// <summary>
        /// Play a frequency for a specified duration
        /// </summary>
        /// <param name="frequency">The frequency in hertz of the tone to be played</param>
        /// <param name="duration">How long the note is played in milliseconds, if durration is 0, tone plays indefinitely</param>
        public async Task PlayTone(Frequency frequency, TimeSpan duration)
        {
            if (frequency.Hertz <= 1)
            {
                throw new Exception("Piezo frequency must be greater than 1Hz");
            }

            if (!isPlaying)
            {
                isPlaying = true;

                PwmPort.Frequency = frequency;
                PwmPort.DutyCycle = 0.5f;

                if (duration.TotalMilliseconds > 0)
                {
                    await Task.Delay(duration);
                    PwmPort.DutyCycle = 0f;
                }

                isPlaying = false;
            }
        }

        /// <summary>
        /// Stops a tone playing
        /// </summary>
        public void StopTone()
        {
            PwmPort.DutyCycle = 0f;
        }

        /// <summary>
        /// Dispose peripheral
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && ShouldDisposePort)
                {
                    PwmPort.Dispose();
                }

                IsDisposed = true;
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
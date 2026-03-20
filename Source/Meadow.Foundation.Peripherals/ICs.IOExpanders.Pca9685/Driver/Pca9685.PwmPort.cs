using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Pca9685
{
    /// <summary>
    /// Pca9685 PWM port
    /// </summary>
    public class PwmPort : IPwmPort
    {
        private readonly Pca9685 controller;
        private double dutyCycle;
        private readonly byte portNumber;
        private bool isRunning;
        private bool inverted;

        /// <summary>
        /// Channel info
        /// </summary>
        public IPwmChannelInfo Channel { get; }

        IDigitalChannelInfo IPort<IDigitalChannelInfo>.Channel { get; }

        /// <summary>
        /// Duration
        /// </summary>
        public TimePeriod Duration
        {
            get => TimePeriod.FromSeconds(1d / Frequency.Hertz * DutyCycle);
            set
            {
                Console.WriteLine($"setting duration to {value}");
                DutyCycle = (value.Seconds * Frequency.Hertz / 2d);
            }
        }

        /// <summary>
        /// Period
        /// </summary>
        public TimePeriod Period
        {
            get => TimePeriod.FromSeconds(1 / Frequency.Hertz);
            set => throw new Exception("Frequency is set for the controller and cannot be changed per port");
        }

        /// <summary>
        /// Gets the overall PWM Frequency set for the PCA9685. Can't be changed per port.
        /// </summary>
        public Frequency Frequency
        {
            get => controller.Frequency;
            set => throw new Exception("Frequency is set for the controller and cannot be changed per port");
        }

        /// <summary>
        /// State
        /// </summary>
        public bool State => isRunning;

        /// <summary>
        /// Pin
        /// </summary>
        public IPin Pin { get; }

        /// <summary>
        /// Duty cycle
        /// </summary>
        public double DutyCycle
        {
            get => dutyCycle;
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Duty cycle must be between 0.0 and 1.0.");
                }

                dutyCycle = value;
                Start();
            }
        }

        /// <summary>
        /// Get or set inversion
        /// </summary>
        public bool Inverted
        {
            get => inverted;
            set
            {
                if (Inverted == value) return;
                inverted = value;
                if (isRunning)
                {
                    Start();
                }
            }
        }

        /// <summary>
        /// Create new PwmPort
        /// </summary>
        /// <param name="controller">The parent PCA9685</param>
        /// <param name="pin">The controller pin</param>
        /// <param name="frequency">PWM frequency</param>
        /// <param name="dutyCycle">Duty cycle</param>
        /// <param name="inverted">Invert the output signal</param>
        internal PwmPort(
            Pca9685 controller,
            IPin pin,
            Units.Frequency frequency,
            float dutyCycle,
            bool inverted)
        {
            this.controller = controller;
            this.Pin = pin;
            this.Channel = (IPwmChannelInfo)pin.SupportedChannels.First(c => c is IPwmChannelInfo);
            this.dutyCycle = dutyCycle;
            this.portNumber = (byte)pin.Key;
            this.Inverted = inverted;

            Stop();
        }

        /// <summary>
        /// Start PWM ports
        /// </summary>
        public void Start()
        {
            // The PWM value is based on the duty cycle, from 0 to 4096
            var pwmValue = (int)(4096 * DutyCycle);
            int on = 0;
            int off = 0;

            if (Inverted)
            {
                if (pwmValue == 0)
                {
                    on = 4096;
                    off = 0;
                }
                else if (pwmValue >= 4096)
                {
                    on = 0;
                    off = 4096;
                }
                else
                {
                    on = 0;
                    off = 4096 - pwmValue;
                }

            }
            else
            {
                if (pwmValue == 0)
                {
                    on = 0;
                    off = 4096;
                }
                else if (pwmValue >= 4096)
                {
                    on = 4096;
                    off = 0;
                }
                else
                {
                    on = 0;
                    off = pwmValue;
                }
            }

            controller.SetPwm(portNumber, on, off);
            isRunning = true;
        }

        /// <summary>
        /// Stop PWM ports
        /// </summary>
        public void Stop()
        {
            controller.SetPwm(portNumber, 4096, 4096);
            isRunning = false;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
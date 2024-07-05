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
        private readonly Frequency frequency;
        private int onCount = 0;

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
            get => TimePeriod.FromSeconds(1 / frequency.Hertz);
            set => Frequency = new Frequency(1 / value.Seconds, Units.Frequency.UnitType.Hertz);
        }

        /// <summary>
        /// Gets the overall PWM Frequency set for the PCA9685. Can't be changed per port.
        /// </summary>
        public Units.Frequency Frequency
        {
            get => frequency;
            set
            {
            }
        }

        /// <summary>
        /// State
        /// </summary>
        public bool State => onCount > 0;

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
                dutyCycle = value;
                Start();
            }
        }

        /// <summary>
        /// Get or set inversion
        /// </summary>
        public bool Inverted { get; set; }

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
            this.frequency = frequency;
            this.Inverted = inverted;

            Stop();
        }

        /// <summary>
        /// Start PWM ports
        /// </summary>
        public void Start()
        {
            // the 9685 PWM pulse is 4096 cycles long.  You must tell it how many to be on and off
            var newOnCount = (int)(DutyCycle * 4096);
            if (newOnCount == onCount)
            {
                return;
            }

            onCount = newOnCount;
            var offCount = 4096 - onCount;

            if (Inverted)
            {
                controller.SetPwm(portNumber, onCount, offCount);
            }
            else
            {
                controller.SetPwm(portNumber, offCount, onCount);
            }
        }

        /// <summary>
        /// Stop PWM ports
        /// </summary>
        public void Stop()
        {
            onCount = 0;
            controller.SetPwm(portNumber, onCount, 4096);
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
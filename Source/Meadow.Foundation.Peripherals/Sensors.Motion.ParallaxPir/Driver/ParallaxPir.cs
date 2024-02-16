using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Represents a Parallax Passive Infrared (PIR) motion sensor.
    /// </summary>
    public class ParallaxPir : IDisposable
    {
        /// <summary>
        /// Digital input port connected to the PIR sensor.
        /// </summary>
        private readonly IDigitalInterruptPort digitalInputPort;

        /// <summary>
        /// Delegate for motion start and end events.
        /// </summary>
        public delegate void MotionChange(object sender);

        /// <summary>
        /// Event raised when motion is detected.
        /// </summary>
        public event MotionChange OnMotionStart = default!;

        /// <summary>
        /// Event raised when the PIR sensor indicates that there is no longer any motion.
        /// </summary>
        public event MotionChange OnMotionEnd = default!;

        /// <summary>
        /// Gets a value indicating whether the object is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the peripheral port(s) were created by the object.
        /// </summary>
        readonly bool createdPort = false;

        /// <summary>
        /// Creates a new instance of the Parallax PIR sensor connected to an input pin with specified interrupt and resistor modes.
        /// </summary>
        /// <param name="pin">The input pin to which the PIR sensor is connected.</param>
        /// <param name="interruptMode">The interrupt mode for the input pin.</param>
        /// <param name="resistorMode">The resistor mode for the input pin.</param>
        public ParallaxPir(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode) :
            this(pin.CreateDigitalInterruptPort(interruptMode, resistorMode, TimeSpan.FromMilliseconds(2), TimeSpan.Zero))
        {
            createdPort = true;
        }

        /// <summary>
        /// Creates a new instance of the Parallax PIR sensor connected to an input pin with specified interrupt and resistor modes, debounce duration, and glitch filter cycle count.
        /// </summary>
        /// <param name="pin">The input pin to which the PIR sensor is connected.</param>
        /// <param name="interruptMode">The interrupt mode for the input pin.</param>
        /// <param name="resistorMode">The resistor mode for the input pin.</param>
        /// <param name="debounceDuration">The duration for input signal debouncing.</param>
        /// <param name="glitchFilterCycleCount">The glitch filter cycle count for input signal filtering.</param>
        public ParallaxPir(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, TimeSpan debounceDuration, TimeSpan glitchFilterCycleCount) :
            this(pin.CreateDigitalInterruptPort(interruptMode, resistorMode, debounceDuration, glitchFilterCycleCount))
        {
            createdPort = true;
        }

        /// <summary>
        /// Creates a new instance of the Parallax PIR sensor connected to a digital interrupt port.
        /// </summary>
        /// <param name="digitalInputPort">The digital interrupt port connected to the PIR sensor.</param>
        public ParallaxPir(IDigitalInterruptPort digitalInputPort)
        {
            if (digitalInputPort != null)
            {
                this.digitalInputPort = digitalInputPort;
                this.digitalInputPort.Changed += DigitalInputPortChanged;
            }
            else
            {
                throw new Exception("Invalid pin for the PIR interrupts.");
            }
        }

        /// <summary>
        /// Handles the PIR motion change interrupts and raises the appropriate events.
        /// </summary>
        private void DigitalInputPortChanged(object sender, DigitalPortResult e)
        {
            if (digitalInputPort.State)
            {
                OnMotionStart?.Invoke(this);
            }
            else
            {
                OnMotionEnd?.Invoke(this);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the object and releases any associated resources.
        /// </summary>
        /// <param name="disposing">A value indicating whether the object is being disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPort)
                {
                    digitalInputPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}

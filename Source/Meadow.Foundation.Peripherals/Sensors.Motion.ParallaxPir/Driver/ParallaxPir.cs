using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Create a new Parallax PIR object.
    /// </summary>
    public class ParallaxPir
    {
        /// <summary>
        /// Digital input port
        /// </summary>
        private readonly IDigitalInterruptPort _digitalInputPort;

        /// <summary>
        /// Delgate for the motion start and end events.
        /// </summary>
        public delegate void MotionChange(object sender);

        /// <summary>
        /// Event raised when motion is detected.
        /// </summary>
        public event MotionChange OnMotionStart;

        /// <summary>
        /// Event raised when the PIR indicates that there is not longer any motion.
        /// </summary>
        public event MotionChange OnMotionEnd;

        /// <summary>
        /// Create a new Parallax PIR object connected to an input pin and IO Device.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        public ParallaxPir(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode) :
            this(pin.CreateDigitalInterruptPort(interruptMode, resistorMode, TimeSpan.FromMilliseconds(2), TimeSpan.Zero))
        { }

        /// <summary>
        /// Create a new Parallax PIR object connected to an input pin and IO Device.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        /// <param name="debounceDuration"></param>
        /// <param name="glitchFilterCycleCount"></param>
        public ParallaxPir(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, TimeSpan debounceDuration, TimeSpan glitchFilterCycleCount) :
            this(pin.CreateDigitalInterruptPort(interruptMode, resistorMode, debounceDuration, glitchFilterCycleCount))
        { }

        /// <summary>
        /// Create a new Parallax PIR object connected to a interrupt port.
        /// </summary>
        /// <param name="digitalInputPort"></param>
        public ParallaxPir(IDigitalInterruptPort digitalInputPort)
        {
            //TODO: I changed this from Pins.GPIO_NONE to null
            if (digitalInputPort != null)
            {
                _digitalInputPort = digitalInputPort;
                _digitalInputPort.Changed += DigitalInputPortChanged;
            }
            else
            {
                throw new Exception("Invalid pin for the PIR interrupts.");
            }
        }

        /// <summary>
        /// Catch the PIR motion change interrupts and work out which interrupt should be raised.
        /// </summary>
        private void DigitalInputPortChanged(object sender, DigitalPortResult e)
        {
            if (_digitalInputPort.State)
            {
                OnMotionStart?.Invoke(this);
            }
            else
            {
                OnMotionEnd?.Invoke(this);
            }
        }
    }
}
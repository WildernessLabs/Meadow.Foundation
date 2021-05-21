using System;
using Meadow.Hardware;

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
        private readonly IDigitalInputPort _digitalInputPort;

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
        /// <param name="device"></param>
        /// <param name="inputPin"></param>
        public ParallaxPir(IDigitalInputController device, IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, int debounceDuration = 20, int glitchFilterCycleCount = 0) : 
            this (device.CreateDigitalInputPort(pin, interruptMode, resistorMode, debounceDuration, glitchFilterCycleCount)) { }

        /// <summary>
        /// Create a new Parallax PIR object connected to a interrupt port.
        /// </summary>
        /// <param name="digitalInputPort"></param>        
        public ParallaxPir(IDigitalInputPort digitalInputPort)
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
using Meadow.Hardware;
using System;
using static Meadow.Hardware.DigitalPortBase;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    ///     Create a new Parallax PIR object.
    /// </summary>
    public class ParallaxPir
    {
        #region Member variables and fields

        /// <summary>
        ///     Digital input port
        /// </summary>
        private readonly IDigitalInputPort _digitalInputPort;

        #endregion Member variables and fields

        #region Delegates and events

        /// <summary>
        ///     Delgate for the motion start and end events.
        /// </summary>
        public delegate void MotionChange(object sender);

        /// <summary>
        ///     Event raised when motion is detected.
        /// </summary>
        public event MotionChange OnMotionStart;

        /// <summary>
        ///     Event raised when the PIR indicates that there is not longer any motion.
        /// </summary>
        public event MotionChange OnMotionEnd;

        #endregion Delegates and events

        #region Constructors

        /// <summary>
        /// Default constructor is private to prevent it being called.
        /// </summary>
        private ParallaxPir() { }

        /// <summary>
        /// Create a new Parallax PIR object connected to an input pin and IO Device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputPin"></param>        
        public ParallaxPir(IIODevice device, IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, int debounceDuration = 20, int glitchFilterCycleCount = 0) : 
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

        #endregion Constructors

        #region Interrupt handlers

        /// <summary>
        ///     Catch the PIR motion change interrupts and work out which interrupt should be raised.
        /// </summary>
        private void DigitalInputPortChanged(object sender, DigitalInputPortEventArgs e)
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

        #endregion Interrupt handlers
    }
}
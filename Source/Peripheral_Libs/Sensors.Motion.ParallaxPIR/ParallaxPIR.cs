using Meadow.Hardware;
using System;
using static Meadow.DigitalPortBase;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    ///     Create a new Parallax PIR object.
    /// </summary>
    public class ParallaxPIR
    {
        #region Member variables and fields

        /// <summary>
        ///     Digital input port
        /// </summary>
        private readonly DigitalInputPort _digitalInputPort;

        #endregion Member variables and fields

        #region Delegates and events

        /// <summary>
        ///     Delgate for the motion start and end events.
        /// </summary>
        public delegate void MotionChange(object sender);

        /// <summary>
        ///     Event raied when motion is detected.
        /// </summary>
        public event MotionChange OnMotionStart;

        /// <summary>
        ///     Event raised when the PIR indicates that there is not longer any motion.
        /// </summary>
        public event MotionChange OnMotionEnd;

        #endregion Delegates and events

        #region Constructors

        /// <summary>
        ///     Default constructor is private to prevent it being called.
        /// </summary>
        private ParallaxPIR()
        {
        }

        /// <summary>
        ///     Create a new Parallax PIR object and hook up the Changed event handler.
        /// </summary>
        /// <param name="digitalInputPort"></param>
        public ParallaxPIR(Pins digitalInputPort)
        {
            if (digitalInputPort != Pins.GPIO_NONE)
            {
                _digitalInputPort = new DigitalInputPort(digitalInputPort, false, ResistorMode.Disabled, InterruptMode.InterruptEdgeBoth);
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
        private void DigitalInputPortChanged(object sender, PortEventArgs e)
        {
            if (_digitalInputPort.Value)
            {
                if (OnMotionStart != null)
                {
                    OnMotionStart(this);
                }
            }
            else
            {
                if (OnMotionEnd != null)
                {
                    OnMotionEnd(this);
                }
            }
        }

        #endregion Interrupt handlers
    }
}
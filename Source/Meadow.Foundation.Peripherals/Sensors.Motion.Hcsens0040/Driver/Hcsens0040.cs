using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Create a new Hscens0040 object
    /// </summary>
    public class Hcsens0040
    {
        /// <summary>
        /// Digital input port
        /// </summary>
        private readonly IDigitalInterruptPort digitalInputPort;

        /// <summary>
        /// Delegate for the motion start and end events
        /// </summary>
        public delegate void MotionChange(object sender);

        /// <summary>
        /// Event raised when motion is detected
        /// </summary>
        public event MotionChange OnMotionDetected = default!;

        /// <summary>
        /// Create a new Parallax PIR object connected to an input pin and IO Device
        /// </summary>
        /// <param name="inputPin">The input pin</param>        
        public Hcsens0040(IPin inputPin) :
            this(inputPin.CreateDigitalInterruptPort(InterruptMode.EdgeRising, ResistorMode.InternalPullDown))
        { }

        /// <summary>
        /// Create a new Parallax PIR object connected to a interrupt port
        /// </summary>
        /// <param name="digitalInputPort"></param>        
        public Hcsens0040(IDigitalInterruptPort digitalInputPort)
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
        /// Catch the PIR motion change interrupts and work out which interrupt should be raised
        /// </summary>
        private void DigitalInputPortChanged(object sender, DigitalPortResult e)
        {
            if (digitalInputPort.State == true)
            {
                OnMotionDetected?.Invoke(this);
            }
        }
    }
}
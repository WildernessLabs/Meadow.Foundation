using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    ///     Create a new Hscens0040 object.
    /// </summary>
    public class Hcsens0040
    {
        

        /// <summary>
        ///     Digital input port
        /// </summary>
        private readonly IDigitalInputPort _digitalInputPort;

        

        

        /// <summary>
        ///     Delgate for the motion start and end events.
        /// </summary>
        public delegate void MotionChange(object sender);

        /// <summary>
        ///     Event raised when motion is detected.
        /// </summary>
        public event MotionChange OnMotionDetected;

        

        

        /// <summary>
        /// Create a new Parallax PIR object connected to an input pin and IO Device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="inputPin"></param>        
        public Hcsens0040(IIODevice device, IPin pin) : 
            this (device.CreateDigitalInputPort(pin, InterruptMode.EdgeRising, ResistorMode.InternalPullDown)) { }

        /// <summary>
        /// Create a new Parallax PIR object connected to a interrupt port.
        /// </summary>
        /// <param name="digitalInputPort"></param>        
        public Hcsens0040(IDigitalInputPort digitalInputPort)
        {
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
        ///     Catch the PIR motion change interrupts and work out which interrupt should be raised.
        /// </summary>
        private void DigitalInputPortChanged(object sender, DigitalInputPortEventArgs e)
        {
            if (_digitalInputPort.State == true)
            {
                OnMotionDetected?.Invoke(this);
            }
        }

        
    }
}
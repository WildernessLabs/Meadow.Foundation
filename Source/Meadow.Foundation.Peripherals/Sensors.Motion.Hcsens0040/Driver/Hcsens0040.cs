using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Create a new Hscens0040 object
    /// </summary>
    public class Hcsens0040 : IDisposable
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
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPort = false;

        /// <summary>
        /// Create a new Parallax PIR object connected to an input pin and IO Device
        /// </summary>
        /// <param name="inputPin">The input pin</param>        
        public Hcsens0040(IPin inputPin) :
            this(inputPin.CreateDigitalInterruptPort(InterruptMode.EdgeRising, ResistorMode.InternalPullDown))
        {
            createdPort = true;
        }

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

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
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
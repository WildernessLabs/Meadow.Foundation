using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Driver for the HCSENS0040 (RCWL-0516) microwave Doppler radar motion sensor.
    /// The output pin goes high for approximately 2 seconds when motion is detected.
    /// The timer is retriggerable — sustained motion keeps the output high continuously.
    /// The output goes low approximately 2 seconds after the last detected movement.
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
        /// Event raised when motion is first detected (rising edge). The output remains
        /// high while motion continues due to the sensor's retriggerable 2-second timer.
        /// </summary>
        public event MotionChange OnMotionDetected = default!;

        /// <summary>
        /// Event raised when motion ends (falling edge), approximately 2 seconds after
        /// the last detected movement.
        /// </summary>
        public event MotionChange OnMotionEnded = default!;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPort = false;

        /// <summary>
        /// Create a new Hcsens0040 object connected to an input pin
        /// </summary>
        /// <param name="inputPin">The input pin</param>
        public Hcsens0040(IPin inputPin) :
            this(inputPin.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, ResistorMode.InternalPullDown))
        {
            createdPort = true;
        }

        /// <summary>
        /// Create a new Hcsens0040 object connected to a digital interrupt port
        /// </summary>
        /// <param name="digitalInputPort">The digital interrupt port. Must be configured for EdgeBoth.</param>
        public Hcsens0040(IDigitalInterruptPort digitalInputPort)
        {
            if (digitalInputPort != null)
            {
                this.digitalInputPort = digitalInputPort;
                this.digitalInputPort.Changed += DigitalInputPortChanged;
            }
            else
            {
                throw new Exception("Invalid digital interrupt port for Hcsens0040.");
            }
        }

        /// <summary>
        /// Handles rising and falling edge interrupts and raises the appropriate motion event
        /// </summary>
        private void DigitalInputPortChanged(object sender, DigitalPortResult e)
        {
            if (e.New.State)
            {
                OnMotionDetected?.Invoke(this);
            }
            else
            {
                OnMotionEnded?.Invoke(this);
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
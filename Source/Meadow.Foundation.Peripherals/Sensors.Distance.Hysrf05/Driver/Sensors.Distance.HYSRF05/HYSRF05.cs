using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Distance;

namespace Meadow.Foundation.Sensors.Distance
{
    /// <summary>
    /// HYSRF05 Distance Sensor
    /// </summary>
    public class HYSRF05 : IRangeFinder
    {
        #region Properties

        /// <summary>
        /// Returns current distance detected in cm.
        /// </summary>
        public float CurrentDistance { get; private set; } = -1;

        /// <summary>
        /// Minimum valid distance in cm (CurrentDistance returns -1 if below).
        /// </summary>
        public float MinimumDistance => 2;

        /// <summary>
        /// Maximum valid distance in cm (CurrentDistance returns -1 if above).
        /// </summary>
        public float MaximumDistance => 450;

        /// <summary>
        /// Raised when an received a rebound trigger signal
        /// </summary>
        public event EventHandler<DistanceEventArgs> DistanceDetected;

        #endregion

        #region Member variables / fields

        /// <summary>
        /// Trigger Pin.
        /// </summary>
        protected IDigitalOutputPort triggerPort;

        /// <summary>
        /// Echo Pin.
        /// </summary>
        protected IDigitalInputPort echoPort;

        protected long tickStart;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor is private to prevent it being called.
        /// </summary>
        private HYSRF05() { }

        /// <summary>
        /// Create a new HYSRF05 object with a IO Device
        /// HSSRF05 must be running the default 4/5 pin mode
        /// 3 pin mode is not supported on Meadow
        /// </summary>
        /// <param name="triggerPin"></param>
        /// <param name="echoPin"></param>
        public HYSRF05(IIODevice device, IPin triggerPin, IPin echoPin) :
            this(device.CreateDigitalOutputPort(triggerPin, false),
                device.CreateDigitalInputPort(echoPin, InterruptMode.EdgeBoth)) { }

        /// <summary>
        /// Create a new HYSRF05 object and hook up the interrupt handler
        /// HSSRF05 must be running the default 4/5 pin mode
        /// 3 pin mode is not supported on Meadow
        /// </summary>
        /// <param name="triggerPort"></param>
        /// <param name="echoPort"></param>
        public HYSRF05(IDigitalOutputPort triggerPort, IDigitalInputPort echoPort)
        {
            this.triggerPort = triggerPort;

            this.echoPort = echoPort;
            this.echoPort.Changed += OnEchoPortChanged;
        }

        #endregion

        /// <summary>
        /// Sends a trigger signal
        /// </summary>
        public void MeasureDistance()
        {
            CurrentDistance = -1;

            // Raise trigger port to high for 10+ micro-seconds
            triggerPort.State = true;
            Thread.Sleep(1); //smallest amount of time we can wait

            // Start Clock
            tickStart = DateTime.Now.Ticks;
            // Trigger device to measure distance via sonic pulse
            triggerPort.State = false;
        }

        private void OnEchoPortChanged(object sender, DigitalInputPortEventArgs e)
        {
            if (e.Value == true) //echo is high
            {
                tickStart = DateTime.Now.Ticks;
                return;
            }

            // Calculate Difference
            float elapsed = DateTime.Now.Ticks - tickStart;

            // Return elapsed ticks
            // x10 for ticks to micro sec
            // divide by 58 for cm (assume speed of sound is 340m/s)
            CurrentDistance = elapsed / 580f;

            if (CurrentDistance < MinimumDistance || CurrentDistance > MaximumDistance)
                CurrentDistance = -1;

            DistanceDetected?.Invoke(this, new DistanceEventArgs(CurrentDistance));
        }
    }
}
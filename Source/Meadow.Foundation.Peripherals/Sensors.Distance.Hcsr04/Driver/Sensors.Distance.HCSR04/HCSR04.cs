using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Distance;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Distance
{
    /// <summary>
    /// HCSR04 Distance Sensor
    /// </summary>
    public class HCSR04 : IRangeFinder
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
        public float MaximumDistance => 400;

        /// <summary>
        /// Raised when an received a rebound trigger signal
        /// </summary>
        public event EventHandler<DistanceEventArgs> DistanceDetected = delegate { };

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
        private HCSR04() { }

        /// <summary>
        /// Create a new HCSR04 object with an IO Device
        /// </summary>
        /// <param name="triggerPin"></param>
        /// <param name="echoPin"></param>
        public HCSR04(IIODevice device, IPin triggerPin, IPin echoPin) :
            this (device.CreateDigitalOutputPort(triggerPin, false), 
                  device.CreateDigitalInputPort(echoPin, InterruptMode.EdgeBoth)) { }

        /// <summary>
        /// Create a new HCSR04 object 
        /// </summary>
        /// <param name="triggerPort"></param>
        /// <param name="echoPort"></param>
        public HCSR04(IDigitalOutputPort triggerPort, IDigitalInputPort echoPort)
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
            if (e.Value == true)
          // if(_echoPort.State == true)
            {
          ///      Console.WriteLine("true");
                tickStart = DateTime.Now.Ticks;
                return;
            }

        //    Console.WriteLine("false");

            // Calculate Difference
            float elapsed = DateTime.Now.Ticks - tickStart;

            // Return elapsed ticks
            // x10 for ticks to micro sec
            // divide by 58 for cm (assume speed of sound is 340m/s)
            CurrentDistance = elapsed / 580f;

        //    if (CurrentDistance < MinimumDistance || CurrentDistance > MaximumDistance)
        //       CurrentDistance = -1;

            DistanceDetected?.Invoke(this, new DistanceEventArgs(CurrentDistance));
        }
    }
}
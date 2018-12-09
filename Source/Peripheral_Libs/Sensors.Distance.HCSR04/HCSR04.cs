using Meadow.Hardware;
using System;
using System.Threading;

namespace Netduino.Foundation.Sensors.Distance
{
    public class HCSR04 : IRangeFinder
    {
        #region Properties

        public float CurrentDistance { get; private set; } = -1;

        public float MinimumDistance => 2; //in cm
        public float MaximumDistance => 400; //in cm

        public event DistanceDetectedEventHandler DistanceDetected = delegate { };

        #endregion

        #region Member variables / fields

        /// <summary>
        ///     Trigger Pin.
        /// </summary>
        protected DigitalOutputPort _triggerPort;

        /// <summary>
        ///     Echo Pin.
        /// </summary>
        protected DigitalInputPort _echoPort;

        protected long _tickStart;

        #endregion

        #region Constructors

        /// <summary>
        ///     Default constructor is private to prevent it being called.
        /// </summary>
        private HCSR04()
        {
        }

        /// <summary>
        ///     Create a new HCSR04 object and hook up the interrupt handler.
        /// </summary>
        /// <param name="triggerPin"></param>
        /// <param name="echoPin"></param>
        public HCSR04(IDigitalPin triggerPin, IDigitalPin echoPin)
        {
            _triggerPort = new DigitalOutputPort(triggerPin, false);

            _echoPort = new DigitalInputPort(echoPin, false, ResistorMode.Disabled);
            _echoPort.Changed += OnEchoPort_Changed;
        }

        #endregion

        public void MeasureDistance()
        {
            CurrentDistance = -1;

            // Raise trigger port to high for 10+ micro-seconds
            _triggerPort.State = true;
            Thread.Sleep(1); //smallest amount of time we can wait

            // Start Clock
            _tickStart = DateTime.Now.Ticks;
            // Trigger device to measure distance via sonic pulse
            _triggerPort.State = false;
        }

        private void OnEchoPort_Changed(object sender, PortEventArgs e)
        {
            if (e.Value == true)
            { 
                _tickStart = DateTime.Now.Ticks;
                return;
            }

            // Calculate Difference
            float elapsed = DateTime.Now.Ticks - _tickStart;

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
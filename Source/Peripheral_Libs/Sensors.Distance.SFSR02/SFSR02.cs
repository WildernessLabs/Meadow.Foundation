﻿using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Distance;
using System;
using System.Threading;

namespace Sensors.Distance.SFSR02
{
    public class SFSR02 : IRangeFinder
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
        public float MaximumDistance => 800;

        /// <summary>
        /// Raised when an received a rebound trigger signal
        /// </summary>
        public event DistanceDetectedEventHandler DistanceDetected = delegate { };

        #endregion

        #region Member variables / fields

        /// <summary>
        /// Trigger/Echo Pin
        /// </summary>
        protected IBiDirectionalPort _triggerEchoPort;

        protected long _tickStart;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor is private to prevent it being called.
        /// </summary>
        private SFSR02() { }

        /// <summary>
        /// Create a new SFSR02 object with an IO Device
        /// </summary>
        /// <param name="triggerPin"></param>
        /// <param name="echoPin"></param>
        public SFSR02(IIODevice device, IPin triggerEchoPin) :
            this(device.CreateBiDirectionalPort(triggerEchoPin, false))
        { }

        /// <summary>
        /// Create a new SFSR02 object 
        /// </summary>
        /// <param name="triggerPin"></param>
        /// <param name="echoPin"></param>
        public SFSR02(IBiDirectionalPort triggerEchoPort)
        {
            _triggerEchoPort = triggerEchoPort;

            _triggerEchoPort.Changed += OnEchoPortChanged;
        }

        #endregion

        /// <summary>
        /// Sends a trigger signal
        /// </summary>
        public void MeasureDistance()
        {
            _triggerEchoPort.Direction = PortDirectionType.Output;
            _triggerEchoPort.State = false;
            Thread.Sleep(1); //smallest amount of time we can wait

            CurrentDistance = -1;

            // Raise trigger port to high for 20 micro-seconds
            _triggerEchoPort.State = true;
            Thread.Sleep(1); //smallest amount of time we can wait

            // Start Clock
            _tickStart = DateTime.Now.Ticks;
            // Trigger device to measure distance via sonic pulse
            _triggerEchoPort.State = false;
        
            _triggerEchoPort.Direction = PortDirectionType.Input;
        }

        private void OnEchoPortChanged(object sender, DigitalInputPortEventArgs e)
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

            //    if (CurrentDistance < MinimumDistance || CurrentDistance > MaximumDistance)
            //       CurrentDistance = -1;

            DistanceDetected?.Invoke(this, new DistanceEventArgs(CurrentDistance));
        }
    }
}

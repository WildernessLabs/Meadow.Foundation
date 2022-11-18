﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Distance
{
    /// <summary>
    /// HYSRF05 Distance Sensor
    /// </summary>
    public class Hysrf05: SensorBase<Length>, IRangeFinder
    {

		/// <summary>
        /// Raised when an received a rebound trigger signal
        /// </summary>
        public event EventHandler<IChangeResult<Length>> DistanceUpdated;

        /// <summary>
        /// Returns current distance
        /// </summary>
        public Length? Distance { get; private set; }

        /// <summary>
        /// Minimum valid distance in cm
        /// </summary>
        public double MinimumDistance => 2;

        /// <summary>
        /// Maximum valid distance in cm
        /// </summary>
        public double MaximumDistance => 450;

        /// <summary>
        /// Trigger Pin.
        /// </summary>
        protected IDigitalOutputPort triggerPort;

        /// <summary>
        /// Echo Pin.
        /// </summary>
        protected IDigitalInputPort echoPort;

        long tickStart;

        /// <summary>
        /// Create a new HYSRF05 object with a IO Device
        /// HSSRF05 must be running the default 4/5 pin mode
        /// 3 pin mode is not supported on Meadow
        /// </summary>
        /// <param name="device">The device conneced to the sensor</param>
        /// <param name="triggerPin">The trigger pin</param>
        /// <param name="echoPin">The echo pin</param>
        public Hysrf05(IDigitalInputOutputController device, IPin triggerPin, IPin echoPin) :
            this(device.CreateDigitalOutputPort(triggerPin, false),
                device.CreateDigitalInputPort(echoPin, InterruptMode.EdgeBoth)) { }

        /// <summary>
        /// Create a new HYSRF05 object and hook up the interrupt handler
        /// HSSRF05 must be running the default 4/5 pin mode
        /// 3 pin mode is not supported on Meadow
        /// </summary>
        /// <param name="triggerPort">The port for the trigger pin</param>
        /// <param name="echoPort">The port for the echo pin</param>
        public Hysrf05(IDigitalOutputPort triggerPort, IDigitalInputPort echoPort)
        {
            this.triggerPort = triggerPort;

            this.echoPort = echoPort;
            this.echoPort.Changed += OnEchoPortChanged;
        }

        /// <summary>
        /// Sends a trigger signal
        /// </summary>
        public void MeasureDistance()
        {
            //Distance = -1;

            // Raise trigger port to high for 10+ micro-seconds
            triggerPort.State = true;
            Thread.Sleep(1); //smallest amount of time we can wait

            // Start Clock
            tickStart = DateTime.Now.Ticks;
            // Trigger device to measure distance via sonic pulse
            triggerPort.State = false;
        }

        private void OnEchoPortChanged(object sender, DigitalPortResult e)
        {
            if (e.New.State) //echo is high
            {
                tickStart = DateTime.Now.Ticks;
                return;
            }

            // Calculate Difference
            var elapsed = DateTime.Now.Ticks - tickStart;

            // Return elapsed ticks
            // x10 for ticks to micro sec
            // divide by 58 for cm (assume speed of sound is 340m/s)
            var curDis = elapsed / 580;

            var oldDistance = Distance;
            var newDistance = new Length(curDis, Length.UnitType.Centimeters);
            Distance = newDistance;

            //debug - remove 
            Console.WriteLine($"{elapsed}, {curDis}, {Distance}, {DateTime.Now.Ticks}");

            //restore this before publishing to hide false results 
            //    if (CurrentDistance < MinimumDistance || CurrentDistance > MaximumDistance)
            //       CurrentDistance = -1;

            var result = new ChangeResult<Length>(newDistance, oldDistance);

            RaiseEventsAndNotify(result);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<Length> ReadSensor()
        {
            // TODO:
            throw new NotImplementedException();
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<Length> changeResult)
        {
            DistanceUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}
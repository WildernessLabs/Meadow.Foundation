using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Distance
{
    /// <summary>
    /// HCSR04 Distance Sensor
    /// </summary>
    public class Hcsr04: SensorBase<Length>, IRangeFinder
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
        public double MaximumDistance => 400;

        /// <summary>
        /// Trigger Pin.
        /// </summary>
        protected IDigitalOutputPort triggerPort;

        /// <summary>
        /// Echo Pin.
        /// </summary>
        protected IDigitalInputPort echoPort;

        protected long tickStart;

        /// <summary>
        /// Create a new HCSR04 object with an IO Device
        /// </summary>
        /// <param name="triggerPin"></param>
        /// <param name="echoPin"></param>
        public Hcsr04(IDigitalInputOutputController device, IPin triggerPin, IPin echoPin) :
            this (device.CreateDigitalOutputPort(triggerPin, false), 
                  device.CreateDigitalInputPort(echoPin, InterruptMode.EdgeBoth)) { }

        /// <summary>
        /// Create a new HCSR04 object 
        /// </summary>
        /// <param name="triggerPort"></param>
        /// <param name="echoPort"></param>
        public Hcsr04(IDigitalOutputPort triggerPort, IDigitalInputPort echoPort)
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
            if (e.New.State)
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

        protected override Task<Length> ReadSensor()
        {
            // TODO:
            throw new NotImplementedException();
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Length> changeResult)
        {
            DistanceUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}
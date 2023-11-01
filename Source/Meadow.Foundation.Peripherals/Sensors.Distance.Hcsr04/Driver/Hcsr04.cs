using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Distance;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Distance
{
    /// <summary>
    /// HCSR04 Distance Sensor - driver not complete
    /// </summary>
    public class Hcsr04 : SamplingSensorBase<Length>, IRangeFinder
    {
        /// <summary>
        /// Raised when an received a rebound trigger signal
        /// </summary>
        public event EventHandler<IChangeResult<Length>> DistanceUpdated = default!;

        /// <summary>
        /// Returns current distance
        /// </summary>
        public Length? Distance { get; protected set; }

        /// <summary>
        /// Minimum valid distance in cm
        /// </summary>
        public double MinimumDistance => 2;

        /// <summary>
        /// Maximum valid distance in cm
        /// </summary>
        public double MaximumDistance => 400;

        /// <summary>
        /// Port for trigger Pin
        /// </summary>
        protected IDigitalOutputPort? triggerPort;

        /// <summary>
        /// Port for echo Pin
        /// </summary>
        protected IDigitalInterruptPort? echoPort;

        private long tickStart;

        /// <summary>
        /// Create a new HCSR04 object with an IO Device
        /// </summary>
        /// <param name="triggerPin">The trigger pin</param>
        /// <param name="echoPin">The echo pin</param>
        public Hcsr04(IPin triggerPin, IPin echoPin) :
            this(triggerPin.CreateDigitalOutputPort(false),
                  echoPin.CreateDigitalInterruptPort(InterruptMode.EdgeBoth))
        { }

        /// <summary>
        /// Create a new HCSR04 object
        /// </summary>
        protected Hcsr04()
        { }

        /// <summary>
        /// Create a new HCSR04 object 
        /// </summary>
        /// <param name="triggerPort">The port for the trigger pin</param>
        /// <param name="echoPort">The port for the echo pin</param>
        public Hcsr04(IDigitalOutputPort triggerPort, IDigitalInterruptPort echoPort)
        {
            this.triggerPort = triggerPort;

            this.echoPort = echoPort;
            this.echoPort.Changed += OnEchoPortChanged;
        }

        /// <summary>
        /// Sends a trigger signal
        /// </summary>
        public virtual void MeasureDistance()
        {
            if (triggerPort == null) { throw new NullReferenceException("Trigger port is null"); }

            //Distance = -1;

            // Raise trigger port to high for 10+ micro-seconds
            triggerPort.State = true;
            Thread.Sleep(1); //smallest amount of time we can wait

            // Start Clock
            tickStart = DateTime.UtcNow.Ticks;
            // Trigger device to measure distance via sonic pulse
            triggerPort.State = false;
        }

        private void OnEchoPortChanged(object sender, DigitalPortResult e)
        {
            if (e.New.State)
            {
                tickStart = DateTime.UtcNow.Ticks;
                return;
            }

            // Calculate Difference
            var elapsed = DateTime.UtcNow.Ticks - tickStart;

            // Return elapsed ticks
            // x10 for ticks to micro sec
            // divide by 58 for cm (assume speed of sound is 340m/s)
            var curDis = elapsed / 580;

            var oldDistance = Distance;
            var newDistance = new Length(curDis, Length.UnitType.Centimeters);
            Distance = newDistance;

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
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<Length> changeResult)
        {
            DistanceUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Starts continuously sampling the sensor
        /// </summary>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            //ToDo
            throw new NotImplementedException();
        }

        /// <summary>
        /// Stops sampling the sensor
        /// </summary>
        public override void StopUpdating()
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Distance
{
    // TODO: consider renaming. the new hardware is GP2Y0D805Z0F. Maybe we just
    // call it the `Gp2xx` distance sensor

    // also, it's been updated to the new patterns but not published yet.

    /// <summary>
    /// GP2D12 Distance Sensor
    /// </summary>
    public class Gp2d12 : SensorBase<Length>, IRangeFinder
    {
        protected IAnalogInputPort AnalogInputPort { get; set; }

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
        /// Create a new Gp2d12 object with an IO Device
        /// </summary>
        public Gp2d12(IAnalogInputController device, 
            IPin analogInputPin,
            int sampleCount = 5,
            TimeSpan? sampleInterval = null,
            Voltage? voltage = null)
        {
            AnalogInputPort = device.CreateAnalogInputPort(analogInputPin, sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), voltage ?? new Voltage(3.3));

            // wire up our observable
            // have to convert from voltage to length units for our consumers
            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    result => {
                        // create a new change result from the new value
                        ChangeResult<Length> changeResult = new ChangeResult<Length>() {
                            New = VoltageToDistance(result.New),
                            Old = Distance
                        };
                        // save state
                        Distance = changeResult.New;
                        // notify
                        RaiseEventsAndNotify(changeResult);
                    }
                )
           );

        }

        /// <summary>
        /// Convenience method to get the current distance. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        /// <returns>A float value that's ann average value of all the samples taken.</returns>
        protected override async Task<Length> ReadSensor()
        {
            // read the voltage
            Voltage voltage = await AnalogInputPort.Read();

            // convert the voltage
            var newReading = VoltageToDistance(voltage);
            Distance = newReading;

            return newReading;
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `Changed` events and IObservable
        /// subscribers getting notified. Use the `readIntervalDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.
        /// The default is 5 seconds.</param>
        public void StartUpdating(TimeSpan? updateInterval)
        {
            // thread safety
            lock (samplingLock) {
                if (IsSampling) return;
                IsSampling = true;
                AnalogInputPort.StartUpdating(updateInterval);
            }
        }

        /// <summary>
        /// Stops sampling the sensor.
        /// </summary>
        public void StopUpdating()
        {
            lock (samplingLock) {
                if (!IsSampling) return;
                base.IsSampling = false;
                AnalogInputPort.StopUpdating();
            }
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Length> changeResult)
        {
            DistanceUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        protected Length VoltageToDistance(Voltage voltage)
        {
            var distance = 26 / voltage.Volts;

            distance = Math.Max(distance, MinimumDistance);
            var newDistance = new Length(distance, Length.UnitType.Meters);
            Distance = newDistance;
            return newDistance;
        }

    }
}
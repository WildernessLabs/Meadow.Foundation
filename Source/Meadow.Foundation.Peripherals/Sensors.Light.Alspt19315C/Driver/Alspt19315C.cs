using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// Represents an Alspt19315C analog light sensor
    /// </summary>
    public class Alspt19315C : SamplingSensorBase<Voltage>
    {
        /// <summary>
        /// Analog port connected to the sensor
        /// </summary>
        private readonly IAnalogInputPort AnalogInputPort;

        /// <summary>
        /// The current voltage reading of the sensor
        /// </summary>
        public Voltage Voltage { get; protected set; }

        /// <summary>
        /// Create a new light sensor object using a static reference voltage
        /// </summary>
        /// <param name="device">The device connected to the sensor</param>
        /// <param name="pin">The analog pin</param>
        /// <param name="sampleCount">The sample count</param>
        /// <param name="sampleInterval">The sample interval</param>
        /// <param name="voltage">The peak voltage</param>
        public Alspt19315C(IAnalogInputController device, IPin pin, int sampleCount = 5, TimeSpan? sampleInterval = null, Voltage? voltage = null)
            : this(device.CreateAnalogInputPort(pin, sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), voltage ?? new Voltage(3.3)))
        { }

        /// <summary>
        /// Create a new light sensor object using a static reference voltage
        /// </summary>
        /// <param name="port"></param>
        public Alspt19315C(IAnalogInputPort port)
        {
            AnalogInputPort = port;

            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    result => {
                        ChangeResult<Voltage> changeResult = new ChangeResult<Voltage>() {
                            New = result.New,
                            Old = Voltage
                        };
                        Voltage = changeResult.New;
                        RaiseEventsAndNotify(changeResult);
                    }
                )
           );
        }

        /// <summary>
        /// Starts continuously sampling the sensor
        ///
        /// This method also starts raising `Changed` events and IObservable
        /// subscribers getting notified. Use the `readIntervalDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.
        /// </param>
        public override void StartUpdating(TimeSpan? updateInterval)
        {
            lock (samplingLock) {
                if (IsSampling) return;
                IsSampling = true;
                AnalogInputPort.StartUpdating(updateInterval);
            }
        }

        /// <summary>
        /// Stops sampling the sensor
        /// </summary>
        public override void StopUpdating()
        {
            lock (samplingLock) {
                if (!IsSampling) return;
                base.IsSampling = false;
                AnalogInputPort.StopUpdating();
            }
        }

        /// <summary>
        /// Convenience method to get the current temperature. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        /// <returns>A float value that's ann average value of all the samples taken.</returns>
        protected override async Task<Voltage> ReadSensor()
        {
            Voltage = await AnalogInputPort.Read();
            return Voltage;
        }
    }
}
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Base
{
    /// <summary>
    /// Represents an AnalogSamplingBase sensor
    /// </summary>
    public abstract class AnalogSamplingBase : SamplingSensorBase<Voltage>
    {
        /// <summary>
        /// Analog port connected to the sensor
        /// </summary>
        private readonly IAnalogInputPort AnalogInputPort;

        /// <summary>
        /// Current voltage
        /// </summary>
        public Voltage Voltage { get; protected set; }

        /// <summary>
        /// Creates a new AnalogObservableBase driver
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="pin">AnalogChannel connected to the sensor.</param>
        /// <param name="sampleCount">Number of sample to average for a reading</param>
        /// <param name="sampleInterval">Time between intervals</param>
        /// <param name="voltage">Max voltage of analog port</param>
        public AnalogSamplingBase(IAnalogInputController device, IPin pin, int sampleCount = 5, TimeSpan? sampleInterval = null, Voltage? voltage = null)
            : this(device.CreateAnalogInputPort(pin, sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), voltage ?? new Voltage(3.3)))
        { }

        /// <summary>
        /// Creates a new AnalogObservableBase driver
        /// </summary>
        /// <param name="port">analog input port</param>
        public AnalogSamplingBase(IAnalogInputPort port)
        {
            AnalogInputPort = port;

            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    result =>
                    {
                        ChangeResult<Voltage> changeResult = new ChangeResult<Voltage>()
                        {
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
        public override void StartUpdating(TimeSpan? updateInterval)
        {
            lock (samplingLock)
            {
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
            lock (samplingLock)
            {
                if (!IsSampling) return;
                IsSampling = false;
                AnalogInputPort.StopUpdating();
            }
        }

        /// <summary>
        /// Convenience method to get the current voltage
        /// </summary>
        /// <returns>A float value that represents the current voltage</returns>
        protected override async Task<Voltage> ReadSensor()
        {
            Voltage = await AnalogInputPort.Read();
            return Voltage;
        }
    }
}
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Base
{
    /// <summary>
    /// Represents an AnalogObservableBase sensor
    /// </summary>
    public abstract class AnalogObservableBase : SensorBase<Voltage>
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
        /// <param name="sampleCount">number of sample to average for a reading</param>
        /// <param name="sampleInterval">time between intervals</param>
        /// <param name="voltage">max voltage of analog port</param>
        public AnalogObservableBase(IAnalogInputController device, IPin pin, int sampleCount = 5, TimeSpan? sampleInterval = null, Voltage? voltage = null)
            : this(device.CreateAnalogInputPort(pin, sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), voltage ?? new Voltage(3.3)))
        {
        }

        /// <summary>
        /// Creates a new AnalogObservableBase driver
        /// </summary>
        /// <param name="port">analog input port</param>
        public AnalogObservableBase(IAnalogInputPort port)
        {
            AnalogInputPort = port;

            // wire up our observable
            // have to convert from voltage to temp units for our consumers
            // this is where the magic is: this allows us to extend the IObservable
            // pattern through the sensor driver
            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    result =>
                    {
                        // create a new change result from the new value
                        ChangeResult<Voltage> changeResult = new ChangeResult<Voltage>()
                        {
                            New = result.New,
                            Old = Voltage
                        };
                        // save state
                        Voltage = changeResult.New;
                        // notify
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
        public void StartUpdating(TimeSpan? updateInterval)
        {
            // thread safety
            lock (samplingLock)
            {
                if (IsSampling) return;
                IsSampling = true;
                AnalogInputPort.StartUpdating(updateInterval);
            }
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (samplingLock)
            {
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
            // read the voltage
            Voltage = await AnalogInputPort.Read();
            return Voltage;
        }
    }
}
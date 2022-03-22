using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Moisture;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// Capacitive Soil Moisture Sensor
    /// </summary>
    public class Capacitive : SensorBase<double>, IMoistureSensor
    {
        /// <summary>
        /// Raised when a new sensor reading has been made. To enable, call StartUpdating().
        /// </summary>        
        public event EventHandler<IChangeResult<double>> HumidityUpdated = delegate { };

        // internal thread lock
        object _lock = new object();

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        public IAnalogInputPort AnalogInputPort { get; protected set; }

        /// <summary>
        /// Last value read from the moisture sensor.
        /// </summary>
        public double? Moisture { get; protected set; }

        /// <summary>
        /// Voltage value of most dry soil. Default of `0V`.
        /// </summary>
        public Voltage MinimumVoltageCalibration { get; set; } = new Voltage(0);

        /// <summary>
        /// Voltage value of most moist soil. Default of `3.3V`.
        /// </summary>
        public Voltage MaximumVoltageCalibration { get; set; } = new Voltage(3.3);

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the specified analog pin and a IO device.
        /// </summary>
        /// <param name="device">The `IAnalogInputController` to create the port on.</param>
        /// <param name="analogPin">Analog pin the temperature sensor is connected to.</param>
        /// <param name="updateInterval">The time, to wait between sets of sample readings. 
        /// This value determines how often`Changed` events are raised and `IObservable` consumers are notified.</param>
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleInterval">The time, to wait in between samples during a reading.</param>
        public Capacitive(
            IAnalogInputController device, IPin analogPin,
            Voltage? minimumVoltageCalibration, Voltage? maximumVoltageCalibration,
            TimeSpan? updateInterval = null,
            int sampleCount = 5, TimeSpan? sampleInterval = null)
                : this(device.CreateAnalogInputPort(analogPin, sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), new Voltage(3.3)),
                      minimumVoltageCalibration, maximumVoltageCalibration)
        {
            this.UpdateInterval = updateInterval ?? TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the especified AnalogInputPort.
        /// </summary>
        /// <param name="analogPort"></param>
        public Capacitive(
            IAnalogInputPort analogPort,
            Voltage? minimumVoltageCalibration, Voltage? maximumVoltageCalibration)
        {
            AnalogInputPort = analogPort;

            if(minimumVoltageCalibration is { } min) { MinimumVoltageCalibration = min; }
            if(maximumVoltageCalibration is { } max) { MaximumVoltageCalibration = max; }

            // wire up our observable
            // have to convert from voltage to temp units for our consumers
            // this is where the magic is: this allows us to extend the IObservable
            // pattern through the sensor driver
            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    h => {
                        var newMoisture = VoltageToMoisture(h.New);
                        double? oldMoisture = null;
                        if(h.Old is { } oldValue) { oldMoisture = VoltageToMoisture(oldValue); }
                        Moisture = newMoisture;
                        RaiseChangedAndNotify(
                            new ChangeResult<double>(newMoisture, oldMoisture)
                        );
                    }
                )
           );
        }

        protected override async Task<double> ReadSensor()
        {
            // read the voltage
            Voltage voltage = await AnalogInputPort.Read();
            // convert and save to our temp property for later retrieval
            var newMoisture = VoltageToMoisture(voltage);
            Moisture = newMoisture;
            // return new and old Moisture values
            return newMoisture;
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `Updated` events and IObservable
        /// subscribers getting notified. Use the `standbyDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.
        /// The default is 5 seconds.</param>
        public void StartUpdating(TimeSpan updateInterval)
        {
            AnalogInputPort.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stops sampling the sensor.
        /// </summary>
        public void StopUpdating()
        {
            AnalogInputPort.StopUpdating();
        }

        protected void RaiseChangedAndNotify(IChangeResult<double> changeResult)
        {
            HumidityUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        protected double VoltageToMoisture(Voltage voltage)
        {
            if (MinimumVoltageCalibration > MaximumVoltageCalibration) {
                return (1f - voltage.Volts.Map(MaximumVoltageCalibration.Volts, MinimumVoltageCalibration.Volts, 0f, 1.0f));
            }

            return (1f - voltage.Volts.Map(MinimumVoltageCalibration.Volts, MaximumVoltageCalibration.Volts, 0f, 1.0f));
        }
    }
}
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents an analog water level sensor
    /// </summary>
    public partial class AnalogWaterLevel
        : SensorBase<float>
    {
        //==== internals
        IAnalogInputPort AnalogInputPort { get; }

        /// <summary>
        /// Calibration of water level
        /// </summary>
        public Calibration LevelCalibration { get; protected set; }

        /// <summary>
        /// Water level
        /// </summary>
        public float WaterLevel { get; protected set; }

        /// <summary>
        ///     New instance of the AnalogWaterLevel class.
        /// </summary>
        /// <param name="device">The `IAnalogInputController` to create the port on.</param>
        /// <param name="analogPin">Analog pin the sensor is connected to.</param>
        /// <param name="calibration">Calibration for the analog sensor.</param>
        /// <param name="updateIntervalMs">The time, in milliseconds, to wait
        /// between sets of sample readings. This value determines how often
        /// `Changed` events are raised and `IObservable` consumers are notified.</param>
        public AnalogWaterLevel(
            IAnalogInputController device,
            IPin analogPin,
            Calibration? calibration = null,
            int updateIntervalMs = 1000)
                : this(device.CreateAnalogInputPort(analogPin, 5, TimeSpan.FromMilliseconds(40), new Voltage(3.3, Voltage.UnitType.Volts)), calibration)
        {
            base.UpdateInterval = TimeSpan.FromMilliseconds(updateIntervalMs);
        }

        /// <summary>
        /// New instance of the AnalogWaterLevel class.
        /// </summary>
        /// <param name="analogInputPort">Analog port the sensor is connected to.</param>
        /// <param name="calibration">Calibration for the analog sensor.</param>
        public AnalogWaterLevel(IAnalogInputPort analogInputPort,
                                 Calibration? calibration = null)
        {
            AnalogInputPort = analogInputPort;

            //
            //  If the calibration object is null use the defaults for TMP35.
            //
            LevelCalibration = calibration ?? new Calibration();

            // wire up our observable
            AnalogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    h => {
                        // capture the old water leve.
                        var oldWaterLevel = WaterLevel;
                        //var oldWaterLevel = VoltageToWaterLevel(h.Old);

                        // get the new one
                        var newWaterLevel = VoltageToWaterLevel(h.New);
                        WaterLevel = newWaterLevel; // save state

                        base.RaiseEventsAndNotify(
                            new ChangeResult<float>(newWaterLevel, oldWaterLevel)
                        );
                    }
                )
           );
        }
        
        /// <summary>
        /// Convenience method to get the current water level. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        protected override async Task<float> ReadSensor()
        {
            // read the voltage
            Voltage voltage = await AnalogInputPort.Read();

            // convert and save to our temp property for later retreival
            WaterLevel = VoltageToWaterLevel(voltage);

            // return
            return WaterLevel;
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
            AnalogInputPort.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            AnalogInputPort.StopUpdating();
        }

        /// <summary>
        /// Converts a voltage value to a level in centimeters, based on the current
        /// calibration values.
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns></returns>
        protected float VoltageToWaterLevel(Voltage voltage)
        {
            if(voltage <= LevelCalibration.VoltsAtZero)
            {
                return 0;
            }
            return (float)((voltage.Volts - LevelCalibration.VoltsAtZero.Volts) / LevelCalibration.VoltsPerCentimeter.Volts);
        }
    }
}
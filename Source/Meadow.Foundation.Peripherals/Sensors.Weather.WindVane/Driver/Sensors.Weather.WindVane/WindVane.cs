using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Weather;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Weather
{
    /// <summary>
    /// Driver for a wind vane that outputs variable voltage, based on the
    /// azimuth of the wind. Matches the input voltage to the `AzimuthVoltages`
    /// dictionary lookup and returns the nearest azimuth to the voltage specified.
    ///
    /// By default it will use look ups that match voltage outputs from the windvane
    /// in the Sparkfun/Shenzen Fine Offset Electronics with a voltage divider of
    /// 4.7kΩ / 1kΩ, as can be found in the SparkFun weather shield, or Wilderness
    /// Labs Clima Pro board.
    /// </summary>
    public partial class WindVane
        : FilterableChangeObservableBase<Azimuth>, IWindVane
    {
        //==== events
        /// <summary>
        /// Raised when the azimuth of the wind changes.
        /// </summary>
        public event EventHandler<IChangeResult<Azimuth>> Updated = delegate { };

        /// <summary>
        /// The last recorded azimuth of the wind.
        /// </summary>
        public Azimuth? WindAzimuth { get; protected set; }

        // TODO: consider making an `ImmutableDictionary` (need to add package
        /// <summary>
        /// Voltage -> wind azimuth lookup dictionary.
        /// </summary>
        public IDictionary<Voltage, Azimuth> AzimuthVoltages { get; protected set; }

        protected IAnalogInputPort inputPort;

        /// <summary>
        /// Creates a new `WindVane` on the specified IO Device's analog input.
        /// Optionally, with a custom voltage to azimuth lookup.
        /// </summary>
        /// <param name="device">The IO Device.</param>
        /// <param name="analogInputPin">The analog input pin.</param>
        /// <param name="azimuthVoltages">Optional. Supply if you have custom azimuth voltages.</param>
        public WindVane(IAnalogInputController device, IPin analogInputPin, IDictionary<Voltage, Azimuth> azimuthVoltages = null)
            : this(device.CreateAnalogInputPort(analogInputPin), azimuthVoltages)
        {
        }

        /// <summary>
        /// Creates a new `WindVane` on the specified input port. Optionally,
        /// with a custom voltage to azimuth lookup.
        /// </summary>
        /// <param name="inputPort">The analog input.</param>
        /// <param name="azimuthVoltages">Optional. Supply if you have custom azimuth voltages.</param>
        public WindVane(IAnalogInputPort inputPort, IDictionary<Voltage, Azimuth> azimuthVoltages = null)
        {
            this.AzimuthVoltages = azimuthVoltages;
            this.inputPort = inputPort;
            this.Init();
        }

        protected void Init()
        {
            // if no lookup has been provided, load the defaults
            if (AzimuthVoltages == null) { LoadDefaultAzimuthVoltages(); }

            inputPort.Subscribe(
                IAnalogInputPort.CreateObserver(
                handler: result => HandleAnalogUpdate(result),
                filter: null
                ));
        }

        /// <summary>
        /// Starts continuously sampling the sensor.
        ///
        /// This method also starts raising `Updated` events and IObservable
        /// subscribers getting notified. Use the `standbyDuration` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="sampleCount">How many samples to take during a given
        /// reading. These are automatically averaged to reduce noise.</param>
        /// <param name="sampleIntervalDuration">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        /// <param name="standbyDuration">The time, in milliseconds, to wait
        /// between sets of sample readings. This value determines how often
        /// `Changed` events are raised and `IObservable` consumers are notified.</param>
        public void StartUpdating(
            int sampleCount = 5,
            int sampleIntervalDuration = 20,
            int standbyDuration = 500)
        {
            inputPort.StartSampling(sampleCount, sampleIntervalDuration, standbyDuration);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            inputPort.StopSampling();
        }

        /// <summary>
        /// Convenience method to get the current wind azimuth. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        /// <param name="sampleCount">The number of sample readings to take. 
        /// Must be greater than 0. These samples are automatically averaged.</param>
        /// <param name="sampleIntervalDuration">The time, in milliseconds,
        /// to wait in between samples during a reading.</param>
        /// <returns>A float value that's ann average value of all the samples taken.</returns>
        public async Task<Azimuth> Read(int sampleCount = 5, int sampleIntervalDuration = 20)
        {
            // read the voltage
            Voltage voltage = await inputPort.Read(sampleCount, sampleIntervalDuration);
            // get the azimuth
            return LookupWindDirection(voltage);
        }

        /// <summary>
        /// Takes the analog reading and converts to the wind azimuth, then
        /// raises the event/updates subscribers.
        /// </summary>
        protected void HandleAnalogUpdate(IChangeResult<Voltage> result)
        {
            var windAzimuth = LookupWindDirection(result.New);
            ChangeResult<Azimuth> windChangeResult = new ChangeResult<Azimuth>()
            //WindVaneChangeResult windChangeResult = new WindVaneChangeResult()
            {
                Old = this.WindAzimuth,
                New = windAzimuth
            };
            RaiseUpdated(windChangeResult);
            this.WindAzimuth = windAzimuth;
        }

        /// <summary>
        /// Thread and inheritance safe way to raise the event and notify subs
        /// </summary>
        /// <param name="windAzimuth"></param>
        //protected void RaiseUpdated(WindVaneChangeResult changeResult)
        protected void RaiseUpdated(IChangeResult<Azimuth> changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Finds the closest wind azimuth that matches the passed in voltage,
        /// based on the `AziumuthVoltages`.
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns></returns>
        protected Azimuth LookupWindDirection(Voltage voltage)
        {
            Tuple<Azimuth, Voltage> closestFit = null;

            // loop through each azimuth lookup and compute the difference
            // between the measured voltage and the voltage for that azimumth
            Voltage difference;
            foreach (var a in AzimuthVoltages)
            {
                difference = (a.Key - voltage.Volts).Abs();
                // if the closest fit hasn't been set or is further than the
                // computed voltage difference, then we've found a better fit.
                if (closestFit == null || closestFit.Item2 > difference)
                {
                    closestFit = new Tuple<Azimuth, Voltage>(a.Value, difference);
                }
            }

            return closestFit.Item1;
        }

        /// <summary>
        /// Loads a default set of voltage -> azimuth lookup values based on
        /// a 4.7kΩ / 1kΩ voltage divider.
        /// </summary>
        protected void LoadDefaultAzimuthVoltages()
        {
            Console.WriteLine("Loading default azimuth voltages");
            this.AzimuthVoltages = new Dictionary<Voltage, Azimuth> {
                { new Voltage(2.9f), new Azimuth(Azimuth16PointCardinalNames.N) },
                { new Voltage(2.04f), new Azimuth(Azimuth16PointCardinalNames.NNE) },
                { new Voltage(2.19f), new Azimuth(Azimuth16PointCardinalNames.NE) },
                { new Voltage(0.95f), new Azimuth(Azimuth16PointCardinalNames.ENE) },
                { new Voltage(0.989f), new Azimuth(Azimuth16PointCardinalNames.E) },
                { new Voltage(0.874f), new Azimuth(Azimuth16PointCardinalNames.ESE) },
                { new Voltage(1.34f), new Azimuth(Azimuth16PointCardinalNames.SE) },
                { new Voltage(1.12f), new Azimuth(Azimuth16PointCardinalNames.SSE) },
                { new Voltage(1.689f), new Azimuth(Azimuth16PointCardinalNames.S) },
                { new Voltage(1.55f), new Azimuth(Azimuth16PointCardinalNames.SSW) },
                { new Voltage(2.59f), new Azimuth(Azimuth16PointCardinalNames.SW) },
                { new Voltage(2.522f), new Azimuth(Azimuth16PointCardinalNames.WSW) },
                { new Voltage(3.18f), new Azimuth(Azimuth16PointCardinalNames.W) },
                { new Voltage(2.98f), new Azimuth(Azimuth16PointCardinalNames.WNW) },
                { new Voltage(3.08f), new Azimuth(Azimuth16PointCardinalNames.NW) },
                { new Voltage(2.74f), new Azimuth(Azimuth16PointCardinalNames.NNW) },
            };
        }
    }
}
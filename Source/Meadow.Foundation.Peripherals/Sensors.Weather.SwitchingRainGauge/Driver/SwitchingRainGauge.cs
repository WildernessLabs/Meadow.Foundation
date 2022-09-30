using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Weather
{
    /// <summary>
    /// Represents a simple switching rain gauge
    /// </summary>
    public partial class SwitchingRainGauge : SensorBase<Length>
    {
        readonly IDigitalInputPort  rainGaugePort;

        /// <summary>
        /// The number of times the rain tilt sensor has triggered
        /// This count is multiplied by the depth per click to
        /// calculate the rain depth
        /// </summary>
        public int ClickCount { get; protected set; }

        /// <summary>
        /// The total accumulated rain depth
        /// </summary>
        public Length RainDepth => DepthPerClick * ClickCount;

        /// <summary>
        /// The amount of rain recorded per raingauge event
        /// </summary>
        public Length DepthPerClick { get; set; }

        public SwitchingRainGauge(IDigitalInputController device, IPin rainSensorPin) :
            this(device, rainSensorPin, new Length(0.2794, Length.UnitType.Millimeters))
        { }

        public SwitchingRainGauge(IDigitalInputController device, IPin rainSensorPin, Length depthPerClick) :
            this(device.CreateDigitalInputPort(rainSensorPin, InterruptMode.EdgeRising, ResistorMode.InternalPullUp, TimeSpan.FromMilliseconds(100), TimeSpan.Zero), depthPerClick)
        { }

        public SwitchingRainGauge(IDigitalInputPort rainSensorPort, Length depthPerClick)
        {
            DepthPerClick = depthPerClick;
            rainGaugePort = rainSensorPort;
        }

        /// <summary>
        /// Reset the rain height
        /// </summary>
        public void Reset()
        {
            ClickCount = 0;
        }

        private void RainSensorPortChanged(object sender, DigitalPortResult e)
        {
            ClickCount++;

            // create a new change result from the new value
            ChangeResult<Length> changeResult = new ChangeResult<Length>()
            {
                New = RainDepth,
                Old = DepthPerClick * (ClickCount - 1), //last reading, ClickCount will always be at least 1
            };

            // notify
            RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Start the sensor
        /// </summary>
        public void StartUpdating()
        {
            lock (samplingLock)
            {
                if (IsSampling) return;

                IsSampling = true;
                rainGaugePort.Changed += RainSensorPortChanged;
            }
        }

        /// <summary>
        /// Stops sampling the sensor
        /// </summary>
        public void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) { return; }

                IsSampling = false;
                rainGaugePort.Changed -= RainSensorPortChanged;
            }
        }

        /// <summary>
        /// Convenience method to get the current rain depth
        /// </summary>
        protected override Task<Length> ReadSensor()
        {
            return Task.FromResult(RainDepth);
        }
    }
}
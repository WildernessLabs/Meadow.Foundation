using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Weather
{
    public partial class SwitchingRainGauge
    : SensorBase<Length>
    {
        protected IDigitalInputPort rainGaugePort;

        /// <summary>
        /// The number of times the rain tilt sensor has triggered
        /// This count is multiplied by the depth per click to
        /// calculate the rain depth
        /// </summary>
        public int ClickCount { get; protected set; }

        public Length RainDepth => DepthPerClick * ClickCount;

        public Length DepthPerClick { get; set; }

        public SwitchingRainGauge(IDigitalInputController device, IPin rainSensorPin) :
            this(device, rainSensorPin, new Length(0.2794, Length.UnitType.Millimeters))
        {
        }

        public SwitchingRainGauge(IDigitalInputController device, IPin rainSensorPin, Length depthPerClick) :
            this(device.CreateDigitalInputPort(rainSensorPin, InterruptMode.EdgeRising, ResistorMode.InternalPullUp, 500), depthPerClick)
        {

        }

        public SwitchingRainGauge(IDigitalInputPort rainSensorPort, Length depthPerClick)
        {
            this.DepthPerClick = depthPerClick;
            this.rainGaugePort = rainSensorPort;
        }

        public void Reset()
        {
            ClickCount = 0;
        }

        private void RainSensorPort_Changed(object sender, DigitalPortResult e)
        {
            //state
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
            // thread safety
            lock (samplingLock)
            {
                if (IsSampling) return;

                IsSampling = true;
                rainGaugePort.Changed += RainSensorPort_Changed;
            }
        }

        /// <summary>
        /// Stops sampling the sensor.
        /// </summary>
        public void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) return;

                base.IsSampling = false;
                rainGaugePort.Changed -= RainSensorPort_Changed;
            }
        }

        /// <summary>
        /// Convenience method to get the current rain depth. 
        /// </summary>
        protected override Task<Length> ReadSensor()
        {
            return Task.FromResult(RainDepth);
        }
    }
}
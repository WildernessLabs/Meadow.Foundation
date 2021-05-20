using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Distance
{
    /// <summary>
    /// GP2D12 Distance Sensor
    /// </summary>
    public class Gp2d12 :
        FilterableChangeObservableBase<Length>, 
        IRangeFinder
    {
        IAnalogInputPort analogInputPort;

		/// <summary>
        /// Raised when an received a rebound trigger signal
        /// </summary>
        public event EventHandler<IChangeResult<Length>> DistanceUpdated;
        public event EventHandler<IChangeResult<Length>> Updated;

        /// <summary>
        /// Returns current distance
        /// </summary>
        public Length? Distance { get; private set; } = 0;

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
        public Gp2d12(IAnalogInputController device, IPin analogInputPin)
        {
            analogInputPort = device.CreateAnalogInputPort(analogInputPin);
            analogInputPort.Updated += AnalogInputPort_Changed;
        }

        private void AnalogInputPort_Changed(object sender, IChangeResult<Voltage> e)
        {
            var oldDistance = Distance;
            var newDistance = new Length(26 / e.New.Volts, Length.UnitType.Meters);
            Distance = newDistance;

            var result = new ChangeResult<Length>(newDistance, oldDistance);

            Updated?.Invoke(this, result);
            DistanceUpdated?.Invoke(this, result);
        }

        public async Task<Length> ReadDistance()
        {
            var value = await analogInputPort.Read();

            var distance = 26 / value.Volts;

            distance = Math.Max(distance, MinimumDistance);
            var newDistance = new Length(distance, Length.UnitType.Meters);
            Distance = newDistance;
            return newDistance;
        }
    }
}
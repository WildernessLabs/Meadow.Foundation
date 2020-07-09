using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Distance;

namespace Sensors.Distance
{
    public class Gp2D12 : IRangeFinder
    {
        #region Member variables / fields

        IAnalogInputPort analogInputPort;

        public event EventHandler<DistanceEventArgs> DistanceDetected;

        public float CurrentDistance { get; private set; } = -1;

        public float MinimumDistance => 0.098f;

        public float MaximumDistance => 0.79f;

        #endregion Member variables / fields

        #region Constructors

        public Gp2D12(IIODevice device, IPin analogInputPin)
        {
            analogInputPort = device.CreateAnalogInputPort(analogInputPin);
            analogInputPort.Changed += AnalogInputPort_Changed;
        }

        private void AnalogInputPort_Changed(object sender, FloatChangeResult e)
        {
            CurrentDistance = 26 / e.New;
            DistanceDetected?.Invoke(this, new DistanceEventArgs(CurrentDistance));
        }

        #endregion

        #region Methods

        public async Task<float> ReadDistance()
        {
            var value = await analogInputPort.Read();

            CurrentDistance = 26 / value;

            CurrentDistance = Math.Max(CurrentDistance, MinimumDistance);
            CurrentDistance = Math.Min(CurrentDistance, MaximumDistance);

            return CurrentDistance;
        }

        #endregion Methods
    }
}
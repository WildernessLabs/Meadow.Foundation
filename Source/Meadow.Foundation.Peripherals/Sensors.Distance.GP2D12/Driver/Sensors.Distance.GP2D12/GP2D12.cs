using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Distance;

namespace Meadow.Foundation.Sensors.Distance
{
    public class Gp2D12 : IRangeFinder
    {
        IAnalogInputPort analogInputPort;

        public event EventHandler<DistanceEventArgs> DistanceDetected;
        public event EventHandler<DistanceConditionChangeResult> Updated;

        public float CurrentDistance { get; private set; } = -1;

        public float MinimumDistance => 0.098f;

        public float MaximumDistance => 0.79f;

        public DistanceConditions Conditions => throw new NotImplementedException();

        

        

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

        

        

        public async Task<float> ReadDistance()
        {
            var value = await analogInputPort.Read();

            CurrentDistance = 26 / value;

            CurrentDistance = Math.Max(CurrentDistance, MinimumDistance);
            CurrentDistance = Math.Min(CurrentDistance, MaximumDistance);

            return CurrentDistance;
        }

        public IDisposable Subscribe(IObserver<DistanceConditionChangeResult> observer)
        {
            throw new NotImplementedException();
        }

        
    }
}
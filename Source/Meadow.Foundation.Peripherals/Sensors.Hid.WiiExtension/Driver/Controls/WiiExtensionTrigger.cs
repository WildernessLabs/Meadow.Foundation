using Meadow;
using System;

namespace Meadow.Foundation.Sensors.Hid
{
    internal class WiiExtensionTrigger : IAnalogTrigger
    {
        public double? Position { get; protected set; } = 0;
        

        public event EventHandler<ChangeResult<double>> Updated;

        public void Update(byte triggerPosition)
        {
            var newPosition = triggerPosition / 31.0;

            if (newPosition != Position)
            {
                Updated?.Invoke(this, new ChangeResult<double>(newPosition, Position));
                Position = newPosition;
            }
        }
    }
}
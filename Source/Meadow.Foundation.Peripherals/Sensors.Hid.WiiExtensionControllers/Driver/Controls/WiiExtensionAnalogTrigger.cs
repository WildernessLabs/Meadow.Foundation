using Meadow.Peripherals.Sensors.Hid;
using System;

namespace Meadow.Foundation.Sensors.Hid
{
    internal class WiiExtensionAnalogTrigger : IAnalogTrigger
    {
        public double? Position { get; protected set; } = 0;

        public event EventHandler<ChangeResult<double>> Updated;

        //precision scale multiplier 
        readonly double scale = 1;

        public WiiExtensionAnalogTrigger(byte precision)
        {
            scale = 1.0 / Math.Pow(2, precision);
        }

        public void Update(byte triggerPosition)
        {
            var newPosition = triggerPosition * scale;

            if (newPosition != Position)
            {
                Updated?.Invoke(this, new ChangeResult<double>(newPosition, Position));
                Position = newPosition;
            }
        }
    }
}
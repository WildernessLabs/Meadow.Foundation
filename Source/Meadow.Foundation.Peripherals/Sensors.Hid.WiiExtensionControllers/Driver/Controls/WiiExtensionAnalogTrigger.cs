using Meadow.Peripherals.Sensors.Hid;
using System;

namespace Meadow.Foundation.Sensors.Hid
{
    internal class WiiExtensionAnalogTrigger : IAnalogTrigger
    {
        public double? Position { get; protected set; } = 0;

        public event EventHandler<ChangeResult<double>> Updated = delegate { };

        readonly double precisionMultiplier = 1;

        public WiiExtensionAnalogTrigger(byte precision)
        {
            precisionMultiplier = 1.0 / Math.Pow(2, precision);
        }

        public void Update(byte triggerPosition)
        {
            var newPosition = triggerPosition * precisionMultiplier;

            if (newPosition != Position)
            {
                Updated?.Invoke(this, new ChangeResult<double>(newPosition, Position));
                Position = newPosition;
            }
        }
    }
}
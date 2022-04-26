using Meadow;
using System;

namespace Meadow.Foundation.Sensors.Hid
{
    public interface IAnalogTrigger
    {
        public double? Position { get; }

        public event EventHandler<ChangeResult<double>> Updated;
    }
}
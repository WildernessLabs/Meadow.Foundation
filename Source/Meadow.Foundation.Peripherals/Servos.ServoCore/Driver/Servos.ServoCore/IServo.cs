using System;
using Meadow.Units;

namespace Meadow.Foundation.Servos
{
    public interface IServo
    {
        ServoConfig Config { get; }

        void RotateTo(Angle angle);

        Angle? Angle { get; }
    }
}
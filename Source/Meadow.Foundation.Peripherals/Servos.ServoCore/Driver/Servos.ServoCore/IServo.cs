using System;
using Meadow.Units;

namespace Meadow.Foundation.Servos
{
    public interface IAngularServo : IServo
    {
        void RotateTo(Angle angle, bool stopAfterMotion = false);

        Angle? Angle { get; }
    }

    public interface IServo
    {
        ServoConfig Config { get; }
    }
}
using System;
using Meadow.Units;

namespace Meadow.Foundation.Servos
{
    public interface IServo
    {
        ServoConfig Config { get; }

        void RotateTo(Angle angle);

        //void RotateTo(int angle, double speed);

        Angle Angle { get; }
    }
}
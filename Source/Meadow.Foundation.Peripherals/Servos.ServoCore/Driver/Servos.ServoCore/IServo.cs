using System;

namespace Meadow.Foundation.Servos
{
    public interface IServo
    {
        ServoConfig Config { get; }

        void RotateTo(int angle);

        //void RotateTo(int angle, double speed);

        int Angle { get; }
    }
}
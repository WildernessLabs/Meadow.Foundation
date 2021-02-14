using System;

namespace Meadow.Foundation.Servos
{
    public interface IContinuousRotationServo
    {
        ServoConfig Config { get; }

        RotationDirection CurrentDirection { get; }

        float CurrentSpeed { get; }

        void Rotate(RotationDirection direction, float speed);

        void Stop();

    }
}
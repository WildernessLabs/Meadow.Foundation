namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// Continuous rotation servo abstraction
    /// </summary>
    public interface IContinuousRotationServo : IServo
    {
        /// <summary>
        /// Current direction
        /// </summary>
        RotationDirection CurrentDirection { get; }

        /// <summary>
        /// Current servo speed
        /// </summary>
        float CurrentSpeed { get; }

        /// <summary>
        /// Rotate to a direction
        /// </summary>
        /// <param name="direction">The direction</param>
        /// <param name="speed">The rotation speed</param>
        void Rotate(RotationDirection direction, float speed);

        /// <summary>
        /// Stop movement
        /// </summary>
        void Stop();
    }
}
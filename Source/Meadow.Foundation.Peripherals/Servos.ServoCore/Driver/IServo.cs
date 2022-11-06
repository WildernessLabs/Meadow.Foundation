namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// Servo motor abstraction
    /// </summary>
    public interface IServo
    {
        /// <summary>
        /// The servo configuration
        /// </summary>
        ServoConfig Config { get; }
    }
}
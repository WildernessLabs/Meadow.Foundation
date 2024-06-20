using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// Angular servo abstraction
    /// </summary>
    public interface IAngularServo : IServo
    {
        /// <summary>
        /// Rotate to an angle
        /// </summary>
        /// <param name="angle">The angle</param>
        /// <param name="stopAfterMotion">True to stop the servo after motion</param>
        /// <returns></returns>
        Task RotateTo(Angle angle, bool stopAfterMotion = false);

        /// <summary>
        /// The current angle
        /// </summary>
        Angle? Angle { get; }
    }
}
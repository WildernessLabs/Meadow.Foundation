using System.Threading.Tasks;
using Meadow.Units;

namespace Meadow.Foundation.Servos
{
    public interface IAngularServo : IServo
    {
        Task RotateTo(Angle angle, bool stopAfterMotion = false);

        Angle? Angle { get; }
    }
}
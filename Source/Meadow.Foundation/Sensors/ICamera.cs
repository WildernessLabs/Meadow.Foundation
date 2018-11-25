using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors
{
    public interface ICamera
    {
        Task<bool> TakePicture(string filename);
    }
}
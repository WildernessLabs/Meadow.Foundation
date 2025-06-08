using Meadow.Hardware;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

/// <summary>
/// Represents a Temco Controls T3xxx series module
/// </summary>
public interface IT3Module : IPinController
{
    /// <summary>
    /// Reads the serial number of the T3 module.
    /// </summary>
    Task<int> ReadSerialNumber();

    /// <summary>
    /// Reads the firmware version of the T3 module.
    /// </summary>
    Task<float> ReadFirmwareVersion();

    /// <summary>
    /// Reads the model identifier of the T3 module.
    /// </summary>
    Task<T3ModuleModel> ReadModel();

    /// <summary>
    /// Reads the hardware revision of the T3 module.
    /// </summary>
    Task<byte> ReadHardwareRevision();
}

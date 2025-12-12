using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Transceivers;

/// <summary>
/// Interface for LoRaWAN modem operations
/// </summary>
public interface IModem
{
    /// <summary>
    /// Gets the firmware version of the modem
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The firmware version string</returns>
    Task<string> GetFirmwareVersion(CancellationToken cancellationToken);

    /// <summary>
    /// Sets the work mode of the modem
    /// </summary>
    /// <param name="mode">The desired work mode</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetWorkMode(WorkMode mode, CancellationToken cancellationToken);

    //Task<Temperature> GetTemperature();

    //Task FactoryReset();
    //Task SetNetworkSessionKey(byte[] key);
    //Task SetAppSessionKey(byte[] key);
    //Task<WorkMode> GetWorkMode();
    //Task EnterSleepMode();
    //Task<Voltage> GetSupplyVoltage();

}


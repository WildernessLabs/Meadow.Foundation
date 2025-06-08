using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.VFDs;

/// <summary>
/// Represents a generic interface for communicating with an XDrive motor controller.
/// </summary>
public interface IXDrive
{
    /// <summary>
    /// Establishes a connection to the drive.
    /// </summary>
    Task Connect();

    /// <summary>
    /// Disconnects from the drive.
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Reads the ambient temperature from the drive.
    /// </summary>
    /// <returns>The ambient <see cref="Temperature"/>.</returns>
    Task<Temperature> ReadAmbientTemperature();

    /// <summary>
    /// Reads the current control mode of the drive.
    /// </summary>
    /// <returns>The control mode as an unsigned short.</returns>
    Task<ushort> ReadControlMode();

    /// <summary>
    /// Reads the DC bus voltage from the drive.
    /// </summary>
    /// <returns>The DC bus <see cref="Voltage"/>.</returns>
    Task<Voltage> ReadDCBusVoltage();

    /// <summary>
    /// Reads the status of the digital inputs.
    /// </summary>
    /// <returns>A bitmask representing the digital input status.</returns>
    Task<ushort> ReadDigitalInputStatus();

    /// <summary>
    /// Reads the status of the digital outputs.
    /// </summary>
    /// <returns>A bitmask representing the digital output status.</returns>
    Task<ushort> ReadDigitalOutputStatus();

    /// <summary>
    /// Reads the overall drive status.
    /// </summary>
    /// <returns>The drive status as an unsigned short.</returns>
    Task<ushort> ReadDriveStatus();

    /// <summary>
    /// Reads any active or historical error codes from the drive.
    /// </summary>
    /// <returns>The error codes as an unsigned short.</returns>
    Task<ushort> ReadErrorCodes();

    /// <summary>
    /// Reads the IGBT (Insulated Gate Bipolar Transistor) temperature.
    /// </summary>
    /// <returns>The IGBT <see cref="Temperature"/>.</returns>
    Task<Temperature> ReadIGBTTemperature();

    /// <summary>
    /// Reads the current operational status of the drive.
    /// </summary>
    /// <returns>The operational status as an unsigned short.</returns>
    Task<ushort> ReadOperationalStatus();

    /// <summary>
    /// Reads the output current being delivered to the motor.
    /// </summary>
    /// <returns>The output <see cref="Current"/>.</returns>
    Task<Current> ReadOutputCurrent();

    /// <summary>
    /// Reads the output frequency of the drive.
    /// </summary>
    /// <returns>The output <see cref="Frequency"/>.</returns>
    Task<Frequency> ReadOutputFrequency();

    /// <summary>
    /// Reads the output voltage being delivered to the motor.
    /// </summary>
    /// <returns>The output <see cref="Voltage"/>.</returns>
    Task<Voltage> ReadOutputVoltage();
}

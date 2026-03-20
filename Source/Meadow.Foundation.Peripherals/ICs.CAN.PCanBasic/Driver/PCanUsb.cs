using Meadow.Hardware;
using Peak.Can.Basic.BackwardCompatibility;

namespace Meadow.Foundation.ICs.CAN;

/// <summary>
/// Represents a PCAN USB CAN controller implementation.
/// </summary>
/// <remarks>
/// This class provides functionality to create CAN buses for PCAN USB devices. 
/// Note that this implementation is only supported on Windows and requires the PCANBasic DLL.
/// </remarks>
public class PCanUsb : ICanController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PCanUsb"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor checks for PCAN USB compatibility and assumes the presence of the PCANBasic DLL.
    /// </remarks>
    public PCanUsb()
    {
        // TODO: only supported on Windows
        // TODO: check for PCANBasic DLL
    }

    /// <summary>
    /// Creates a CAN bus with the specified bitrate and bus number.
    /// </summary>
    /// <param name="bitrate">The bitrate to configure for the CAN bus.</param>
    /// <param name="busNumber">
    /// The bus number to use for the CAN interface. Defaults to 0, corresponding to <see cref="PCANBasic.PCAN_USBBUS1"/>.
    /// </param>
    /// <returns>
    /// An instance of <see cref="ICanBus"/> configured for the specified bitrate and bus number.
    /// </returns>
    /// <remarks>
    /// If the specified bitrate is <see cref="CanBitrate.Can_FD"/>, the returned bus will support Flexible Data Rate (CAN FD).
    /// Otherwise, a standard CAN bus will be created.
    /// </remarks>
    /// <exception cref="NotImplementedException">
    /// Thrown if the method is invoked without implementing the required functionality.
    /// </exception>
    public ICanBus CreateCanBus(CanBitrate bitrate, int busNumber = 0)
    {
        var config = new PCanConfiguration
        {
            Bitrate = bitrate,
            BusHandle = (ushort)(PCANBasic.PCAN_USBBUS1 + busNumber)
        };

        if (bitrate == CanBitrate.Can_FD)
        {
            return new PCanFdBus(config);
        }
        else
        {
            return new PCanBasic(config);
        }
    }
}
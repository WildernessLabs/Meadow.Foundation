using Meadow.Hardware;
using Peak.Can.Basic.BackwardCompatibility;

namespace Meadow.Foundation.ICs.CAN;

/// <summary>
/// Represents the configuration settings for a PCAN device, including the bus handle and bitrate.
/// </summary>
public class PCanConfiguration
{
    /// <summary>
    /// Gets or sets the handle for the CAN bus.
    /// Default value is <see cref="PCANBasic.PCAN_USBBUS1"/>.
    /// </summary>
    /// <remarks>
    /// This handle corresponds to the specific PCAN device to be used, such as a USB or other supported interface.
    /// </remarks>
    public ushort BusHandle { get; set; } = PCANBasic.PCAN_USBBUS1;

    /// <summary>
    /// Gets or sets the bitrate for the CAN communication.
    /// Default value is <see cref="CanBitrate.Can_250kbps"/>.
    /// </summary>
    /// <remarks>
    /// The bitrate determines the speed of communication on the CAN bus. Common values include 125kbps, 250kbps, and 500kbps.
    /// </remarks>
    public CanBitrate Bitrate { get; set; } = CanBitrate.Can_250kbps;
}

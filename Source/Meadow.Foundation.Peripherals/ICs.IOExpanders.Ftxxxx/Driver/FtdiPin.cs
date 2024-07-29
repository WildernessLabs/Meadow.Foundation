using Meadow.Hardware;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents a single FTDI expander pin
/// </summary>
public class FtdiPin : Pin
{
    /// <summary>
    /// Returns <b>true</b> if the pin is on the low byte of the expander, otherwise <b>false</b>
    /// </summary>
    public bool IsLowByte { get; internal set; }

    internal FtdiPin(bool isLowByte, IPinController? controller, string name, object key, IList<IChannelInfo>? supportedChannels)
        : base(controller, name, key, supportedChannels)
    {
        IsLowByte = isLowByte;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Name}";
    }
}

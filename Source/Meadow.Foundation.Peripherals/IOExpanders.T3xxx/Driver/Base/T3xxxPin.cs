using Meadow.Hardware;
using System.Collections.Generic;

namespace Meadow.Foundation.IOExpanders;

/// <summary>
/// Represents a pin on a Temco Controls T3 module, implementing the IPin interface.
/// Provides access to pin metadata including supported channels and identification.
/// </summary>
public class T3xxxPin : IPin
{
    /// <summary>
    /// Gets the controller that manages this pin.
    /// </summary>
    /// <value>The IPinController instance that owns this pin, or null if not assigned.</value>
    public IPinController? Controller { get; }

    /// <summary>
    /// Gets the list of channel types supported by this pin.
    /// </summary>
    /// <value>A collection of IChannelInfo objects describing the capabilities of this pin.</value>
    public IList<IChannelInfo>? SupportedChannels { get; }

    /// <summary>
    /// Gets the human-readable name of this pin.
    /// </summary>
    /// <value>The descriptive name identifier for this pin.</value>
    public string Name { get; }

    /// <summary>
    /// Gets the unique key identifier for this pin.
    /// </summary>
    /// <value>An object that uniquely identifies this pin within the controller.</value>
    public object Key { get; }

    /// <summary>
    /// Initializes a new instance of the T3xxxPin class.
    /// </summary>
    /// <param name="device">The pin controller that manages this pin.</param>
    /// <param name="name">The human-readable name for this pin.</param>
    /// <param name="key">The unique identifier for this pin.</param>
    /// <param name="channelInfo">Variable number of channel information objects describing pin capabilities.</param>
    internal T3xxxPin(IPinController device, string name, int key, params IChannelInfo[] channelInfo)
    {
        Controller = device;
        Name = name;
        Key = key;
        SupportedChannels = channelInfo;
    }

    /// <summary>
    /// Determines whether this pin is equal to another pin by comparing name and key.
    /// </summary>
    /// <param name="other">The other pin to compare with.</param>
    /// <returns>True if the pins have the same name and key; otherwise, false.</returns>
    public bool Equals(IPin other)
    {
        if (Name != other.Name) return false;
        if (Key != other.Key) return false;
        return true;
    }
}
using System;

namespace Meadow.Foundation.Transceivers;

/// <summary>
/// Event arguments for data received from the LoRaWAN network
/// </summary>
public class DataReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The port on which the data was received
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// The data received as a string
    /// </summary>
    public string Data { get; set; } = string.Empty;
}


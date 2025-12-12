namespace Meadow.Foundation.Transceivers;

/// <summary>
/// Work mode for the LoRaWAN module
/// </summary>
public enum WorkMode
{
    /// <summary>
    /// LoRaWAN Activation By Personalization
    /// </summary>
    LWABP,
    /// <summary>
    /// LoRaWAN Over The Air Activation
    /// </summary>
    LWOTTA,
    /// <summary>
    /// Test mode for diagnostic operations
    /// </summary>
    Test,
}


using Meadow.Hardware;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.ADC;

public partial class Ads7128
{
    /// <summary>
    /// Represents a physical pin on the ADS7128 ADC.
    /// </summary>
    /// <remarks>
    /// Extends the base Pin class with ADS7128-specific functionality
    /// and provides access to the pin's address within the ADC.
    /// </remarks>
    public class Ads7128Pin : Pin
    {
        internal byte Index => (byte)Key;

        internal Ads7128Pin(Ads7128? controller, string name, object key, IList<IChannelInfo>? supportedChannels)
            : base(controller, name, key, supportedChannels)
        {
        }
    }
}
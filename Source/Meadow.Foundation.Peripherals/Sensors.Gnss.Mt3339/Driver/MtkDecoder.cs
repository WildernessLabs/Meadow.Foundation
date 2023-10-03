using Meadow.Foundation.Sensors.Location.Gnss;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace Meadow.Foundation.Sensors.Gnss
{
    /// <summary>
    /// Represents an MTK decoder
    /// </summary>
    public class MtkDecoder : INmeaDecoder
    {
        /// <summary>
        /// Event raised when a message is received 
        /// </summary>
        public event EventHandler<string> MessageReceived = delegate { };

        /// <summary>
        /// Friendly name for the MTK messages.
        /// </summary>
        public string Name => "MediaTek";

        /// <summary>
        /// Prefix for the GLL (Geographic position Latitude / Longitude) decoder.
        /// </summary>
        public string Prefix => "MTK";

        /// <summary>
        /// Process the data from a RMC
        /// </summary>
        /// <param name="sentence">String array of the message components for a RMC message.</param>
        public void Process(NmeaSentence sentence)
        {
            // get the packet type (command number)
            var packetType = sentence.DataElements[0];
            Resolver.Log.Info($"Packet Type:{packetType}, {Lookups.KnownPacketTypes[packetType]}");

            for (int i = 0; i < sentence.DataElements.Count; i++)
            {
                Resolver.Log.Info($"index [{i}], value{sentence.DataElements[i]}");
            }
        }
    }
}
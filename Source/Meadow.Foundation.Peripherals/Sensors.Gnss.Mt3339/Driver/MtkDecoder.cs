using Meadow.Foundation.Sensors.Location.Gnss;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Gnss
{
    /// <summary>
    /// Represents an MTK decoder
    /// </summary>
    public class MtkDecoder : INmeaDecoder
    {
        /// <summary>
        /// Friendly name for the MTK messages.
        /// </summary>
        public string Name => "MediaTek";

        /// <summary>
        /// Prefix for the GLL (Geographic position Latitude / Longitude) decoder.
        /// </summary>
        public string Prefix => "MTK";

        /// <summary>
        /// Process a raw MTK sentence string
        /// </summary>
        /// <param name="sentence">The raw NMEA sentence string</param>
        public void Process(string sentence)
        {
            if (!NmeaSentence.TryParse(sentence, out var s))
            {
                Resolver.Log.Debug($"Failure parsing {sentence}", "nmea processor");
                return;
            }
            Process(s!);
        }

        /// <summary>
        /// Process the data from an MTK sentence
        /// </summary>
        /// <param name="sentence">Parsed NMEA sentence</param>
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
using System;
using Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Sensors.Location.MediaTek
{
    public class MtkDecoder : INmeaDecoder
    {
        public event EventHandler<string> MessageReceived = delegate { };

        /// <summary>
        /// Friendly name for the MTK messages.
        /// </summary>
        public string Name {
            get => "MediaTek";
        }

        /// <summary>
        /// Prefix for the GLL (Geographic position Latitude / Longitude) decoder.
        /// </summary>
        public string Prefix {
            get => "MTK";
        }

        /// <summary>
        /// Process the data from a RMC
        /// </summary>
        /// <param name="data">String array of the message components for a RMC message.</param>
        public void Process(NmeaSentence sentence)
        {
            // get the packet type (command number)
            var packetType = sentence.DataElements[0];
            Console.WriteLine($"Packet Type:{packetType}, {Lookups.KnownPacketTypes[packetType]}");

            for (int i = 0; i < sentence.DataElements.Count; i++) {
                Console.WriteLine($"index [{i}], value{sentence.DataElements[i]}");
            }
            

        }
    }
}

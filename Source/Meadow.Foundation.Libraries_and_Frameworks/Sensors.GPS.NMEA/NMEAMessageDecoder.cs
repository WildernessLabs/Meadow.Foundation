using System;
using System.Collections;
using Meadow.Foundation.Helpers;

namespace Meadow.Foundation.Sensors.GPS
{
    /// <summary>
    ///     NMEA GPS parser
    /// </summary>
    public class NMEAMessageDecoder
    {
        #region Member variables / fields

        /// <summary>
        ///     NMEA decoders available to the GPS.
        /// </summary>
        private readonly Hashtable decoders = new Hashtable();

        #endregion Member variables / fields

        #region Constructors

        /// <summary>
        ///     Default constructor for a NMEA GPS object, this is private to prevent the user from
        ///     using it.
        /// </summary>
        public NMEAMessageDecoder()
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Add a new NMEA decoder to the GPS.
        /// </summary>
        /// <param name="decoder">NMEA decoder.</param>
        public void AddDecoder(INMEADecoder decoder)
        {
            if (decoders.Contains(decoder.Prefix))
            {
                throw new Exception(decoder.Prefix + " already registered.");
            }
            decoders.Add(decoder.Prefix, decoder);
        }

        #endregion Methods

        #region Interrupts

        /// <summary>
        ///     GPS message ready for processing.
        /// </summary>
        /// <remarks>
        ///     Unknown message types will be discarded.
        /// </remarks>
        /// <param name="line">GPS text for processing.</param>
        public void SetNmeaMessage(string line)
        {
            if(string.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("SetPartialNMEAMessage - no data");
                return;
            }

            var checksumLocation = line.LastIndexOf('*');
            if (checksumLocation > 0)
            {
                var checksumDigits = line.Substring(checksumLocation + 1);
                var actualData = line.Substring(0, checksumLocation);
                if (DebugInformation.Hexadecimal(Checksum.XOR(actualData.Substring(1))) == ("0x" + checksumDigits))
                {
                    var elements = actualData.Split(',');
                    if (elements.Length > 0)
                    {
                        var decoder = (INMEADecoder) decoders[elements[0]];
                        if (decoder != null)
                        {
                            decoder.Process(elements);
                        }
                    }
                }
            }
            
        }

        #endregion Interrupts
    }
}
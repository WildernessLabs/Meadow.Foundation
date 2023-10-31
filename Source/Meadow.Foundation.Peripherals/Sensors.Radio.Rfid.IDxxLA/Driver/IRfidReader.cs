using System;

namespace Meadow.Foundation.Sensors.Radio.Rfid
{
    /// <summary>
    /// RFID reader abstraction
    /// </summary>
    public interface IRfidReader : IObservable<byte[]>, IDisposable
    {
        /// <summary>
        /// Event fired when an RFID tag is read.
        /// Check the read status to see if the read was successful.
        /// </summary>
        event RfidReadEventHandler RfidRead;

        /// <summary>
        /// A cached copy of the last successfully read RFID tag.
        /// </summary>
        /// <returns>
        /// The last read RFID tag.
        /// </returns>
        byte[]? LastRead { get; }

        /// <summary>
        /// Start reading for RFID tags.
        /// </summary>
        void StartReading();

        /// <summary>
        /// Stop reading for RFID tags.
        /// </summary>
        void StopReading();
    }

    /// <summary>
    /// RRfid Read Event Handler
    /// </summary>
    /// <param name="sender">The sender object</param>
    /// <param name="e">RfidReadResult event args</param>
    public delegate void RfidReadEventHandler(object sender, RfidReadResult e);
}

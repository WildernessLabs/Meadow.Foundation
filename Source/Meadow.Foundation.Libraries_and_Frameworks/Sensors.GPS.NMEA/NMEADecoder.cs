namespace Meadow.Foundation.Sensors.GPS
{
    /// <summary>
    ///     Base class for NMEADecoder classes.
    /// </summary>
    public abstract class NMEADecoder
    {
        /// <summary>
        ///     Prefix for the decoder (text that occurs at the start of a GPS message
        ///     including the $ symbol - $GPGSA etc.).
        /// </summary>
        public abstract string Prefix { get; }

        /// <summary>
        ///     Friendly name for the decoder.
        /// </summary>
        /// <returns></returns>
        public abstract string Name { get; }

        /// <summary>
        ///     Process the message from the GPS.
        /// </summary>
        /// <param name="elements">String array of the elements of the message.</param>
        public abstract void Process(string[] elements);
    }
}
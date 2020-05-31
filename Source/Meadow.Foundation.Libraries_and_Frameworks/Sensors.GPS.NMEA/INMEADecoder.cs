namespace Meadow.Foundation.Sensors.GPS
{
    /// <summary>
    ///     Base class for NMEADecoder classes.
    /// </summary>
    public interface INMEADecoder
    {
        /// <summary>
        ///     Prefix for the decoder (text that occurs at the start of a GPS message
        ///     including the $ symbol - $GPGSA etc.).
        /// </summary>
        string Prefix { get; }

        /// <summary>
        ///     Friendly name for the decoder.
        /// </summary>
        /// <returns></returns>
        string Name { get; }

        /// <summary>
        ///     Process the message from the GPS.
        /// </summary>
        /// <param name="elements">String array of the elements of the message.</param>
        void Process(string[] elements);
    }
}
namespace Meadow.Foundation.Leds
{
    // TODO: convert to `Units.Voltage`
    /// <summary>
    /// Typical forward voltage values by LED color
    /// </summary>
    public static class TypicalForwardVoltage
    {
        /// <summary>
        /// Typical forward voltage for a red led
        /// </summary>
        public const float Red = 1.7F;
        /// <summary>
        /// Typical forward voltage for a green led
        /// </summary>
        public const float Green = 2.2F;
        /// <summary>
        /// Typical forward voltate for a blue led
        /// </summary>
        public const float Blue = 3.2F;
        /// <summary>
        /// Typical forward voltage for a yellow led
        /// </summary>
        public const float Yellow = 2.1F;
        /// <summary>
        /// Typical forward voltage for a white led
        /// </summary>
        public const float White = 3.2F;

        /// <summary>
        /// Forward voltage for an led with an external resistor
        /// </summary>
        public const float ResistorLimited = 3.3F;
    }
}
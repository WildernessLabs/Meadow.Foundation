using Meadow.Units;

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
        public static Voltage Red = new Voltage(1.7);
        /// <summary>
        /// Typical forward voltage for a green led
        /// </summary>
        public static Voltage Green = new Voltage(2.2);
        /// <summary>
        /// Typical forward voltate for a blue led
        /// </summary>
        public static Voltage Blue = new Voltage(3.2);
        /// <summary>
        /// Typical forward voltage for a yellow led
        /// </summary>
        public static Voltage Yellow = new Voltage(2.1);
        /// <summary>
        /// Typical forward voltage for a white led
        /// </summary>
        public static Voltage White = new Voltage(3.2);
        /// <summary>
        /// Forward voltage for an led with an external resistor
        /// </summary>
        public static Voltage ResistorLimited = new Voltage(3.3);
    }
}
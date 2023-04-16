using Meadow.Units;
using System;

namespace Meadow.Foundation.Audio
{
    /// <summary>
    /// A utility class for converting musical notes to their corresponding frequencies in hertz
    /// </summary>
    public class NotesToFrequency
    {
        /// <summary>
        /// The frequency of the A4 note, in hertz.
        /// </summary>
        public static Frequency A4Frequency { get; set; } = new Frequency(440.0, Frequency.UnitType.Hertz);

        private static double SemitoneRatio { get; } = 1.059463094359;

        /// <summary>
        /// Converts the specified musical note to its frequency in hertz.
        /// </summary>
        /// <param name="note">The musical note to convert.</param>
        /// <returns>The frequency of the note in hertz.</returns>
        public static Frequency ConvertToFrequency(Note note)
        {
            int semitonesFromA4 = CalculateSemitonesFromA4(note.Pitch, note.Octave);
            return A4Frequency * Math.Pow(SemitoneRatio, semitonesFromA4);
        }

        private static int CalculateSemitonesFromA4(Pitch pitch, int octave)
        {
            int semitonesFromC0 = (int)pitch + (octave - 1) * 12;
            return semitonesFromC0 - 9;
        }
    }
}

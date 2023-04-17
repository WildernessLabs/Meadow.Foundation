namespace Meadow.Foundation.Audio
{
    /// <summary>
    /// Represents a musical note, with a specific pitch, octave, and duration
    /// </summary>
    public class Note
    {
        /// <summary>
        /// The pitch of the note
        /// </summary>
        public Pitch Pitch { get; }
        /// <summary>
        /// The octave of the note
        /// </summary>
        public int Octave { get; }
        /// <summary>
        /// The duration of the note
        /// </summary>
        public NoteDuration Duration { get; }

        /// <summary>
        /// Creates a new instance of the Note class, with the specified pitch, octave, and duration
        /// </summary>
        /// <param name="pitch">The pitch of the note</param>
        /// <param name="octave">The octave of the note</param>
        /// <param name="duration">The duration of the note</param>
        public Note(Pitch pitch, int octave, NoteDuration duration)
        {
            Pitch = pitch;
            Octave = octave;
            Duration = duration;
        }
    }
}
using Meadow.Peripherals.Speakers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meadow.Foundation.Audio
{
    /// <summary>
    /// A class for playing a sequence of musical notes
    /// </summary>
    public class Song
    {
        /// <summary>
        /// The collection of notes in order for the song
        /// </summary>
        public readonly List<Note> Notes = new List<Note>();

        /// <summary>
        /// Creates a new instance of the Song class
        /// </summary>
        public Song()
        {
        }

        /// <summary>
        /// Adds a musical note to the sequence of notes to be played
        /// </summary>
        /// <param name="note">The musical note to add</param>
        public void AddNote(Note note)
        {
            Notes.Add(note);
        }

        /// <summary>
        /// Plays the sequence of musical notes, with the specified tempo
        /// </summary>
        /// <param name="speaker">The IToneGenerator object to play the song</param>
        /// <param name="tempo">The tempo of the music, in beats per minute</param>
        /// <returns>A Task representing the asynchronous playback operation</returns>
        public async Task Play(IToneGenerator speaker, int tempo = 120)
        {
            foreach (var note in Notes)
            {

                int duration = (int)(60.0 / tempo * (int)note.Duration);
                Console.WriteLine($"Duration in ms: {duration}");


                if (note.Pitch == Pitch.Rest)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(duration));
                }
                else
                {
                    var frequency = NotesToFrequency.ConvertToFrequency(note);
                    await speaker.PlayTone(frequency, TimeSpan.FromMilliseconds(duration));
                }
            }
        }
    }
}
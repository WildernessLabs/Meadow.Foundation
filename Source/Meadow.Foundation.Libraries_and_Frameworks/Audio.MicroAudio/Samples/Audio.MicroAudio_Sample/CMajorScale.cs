using Meadow.Foundation.Audio;

namespace Audio.MicroAudio_Sample
{
    namespace SongPlayer
    {
        internal class CMajorScale : Song
        {
            public CMajorScale()
            {
                AddNotes();
            }

            void AddNotes()
            {
                for (int i = 0; i < 12; i++)
                {
                    AddNote(new Note((Pitch)(i), 3, NoteDuration.Quarter));
                }
                AddNote(new Note(Pitch.C, 4, NoteDuration.Quarter));
            }
        }
    }
}
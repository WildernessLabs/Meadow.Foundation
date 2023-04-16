using Meadow.Peripherals.Speakers;
using System.Threading.Tasks;

namespace Meadow.Foundation.Audio
{
    /// <summary>
    /// Provide high level audio functions
    /// </summary>
    public partial class MicroAudio
    {
        IToneGenerator speaker;

        SystemSounds systemSounds;
        GameSounds gameSounds;

        /// <summary>
        /// Create a new MicroAudio instance from a ITOneGenerator driver instance
        /// </summary>
        /// <param name="speaker">An IToneGenerator object</param>
        public MicroAudio(IToneGenerator speaker)
        {
            this.speaker = speaker;
        }

        /// <summary>
        /// Plays the specified system sound effect
        /// </summary>
        /// <param name="effect">The sound effect to play</param>
        public Task PlaySystemSound(SystemSoundEffect effect)
        {
            if(systemSounds == null)
            {
                systemSounds = new SystemSounds(speaker);
            }
            return systemSounds.PlayEffect(effect);
        }

        /// <summary>
        /// Plays the specified game sound effect
        /// </summary>
        /// <param name="effect">The sound effect to play</param>
        public Task PlayGameSound(SystemSoundEffect effect)
        {
            if (gameSounds == null)
            {
                gameSounds = new GameSounds(speaker);
            }
            return systemSounds.PlayEffect(effect);
        }

        public Task PlaySong(Song song)
        {
            return song.Play(speaker);
        }
    }
}
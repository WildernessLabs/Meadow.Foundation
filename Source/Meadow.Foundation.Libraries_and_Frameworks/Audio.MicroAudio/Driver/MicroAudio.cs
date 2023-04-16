using Meadow.Peripherals.Speakers;
using System.Threading.Tasks;

namespace Meadow.Foundation.Audio
{
    /// <summary>
    /// Provide high level audio functions
    /// </summary>
    public partial class MicroAudio
    {
        readonly IToneGenerator speaker;

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
            systemSounds ??= new SystemSounds(speaker);
            return systemSounds.PlayEffect(effect);
        }

        /// <summary>
        /// Plays the specified game sound effect
        /// </summary>
        /// <param name="effect">The sound effect to play</param>
        public Task PlayGameSound(GameSoundEffect effect)
        {
            gameSounds ??= new GameSounds(speaker);
            return gameSounds.PlayEffect(effect);
        }

        /// <summary>
        /// Play the specified song
        /// </summary>
        /// <param name="song">The song object</param>
        public Task PlaySong(Song song)
        {
            return song.Play(speaker);
        }
    }
}
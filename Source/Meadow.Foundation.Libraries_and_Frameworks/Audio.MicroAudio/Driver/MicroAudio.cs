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
        /// Create a new MicroAudio instance from a IToneGenerator driver instance
        /// </summary>
        /// <param name="speaker">An IToneGenerator object</param>
        public MicroAudio(IToneGenerator speaker)
        {
            this.speaker = speaker;
        }

        /// <summary>
        /// Set the playback volume
        /// </summary>
        /// <param name="volume">The volume from 0-1</param>
        public void SetVolume(float volume)
        {
            speaker?.SetVolume(volume);
        }

        /// <summary>
        /// Plays the specified system sound effect
        /// </summary>
        /// <param name="effect">The sound effect to play</param>
        /// <param name="numberOfLoops">The number of times to play the sound effect</param>
        public async Task PlaySystemSound(SystemSoundEffect effect, int numberOfLoops = 1)
        {
            systemSounds ??= new SystemSounds(speaker);

            for (int i = 0; i < numberOfLoops; i++)
            {
                await systemSounds.PlayEffect(effect);
            }
        }

        /// <summary>
        /// Plays the specified game sound effect
        /// </summary>
        /// <param name="effect">The sound effect to play</param>
        /// /// <param name="numberOfLoops">The number of times to play the sound effect</param>
        public async Task PlayGameSound(GameSoundEffect effect, int numberOfLoops = 1)
        {
            gameSounds ??= new GameSounds(speaker);

            for (int i = 0; i < numberOfLoops; i++)
            {
                await gameSounds.PlayEffect(effect);
            }
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
using Meadow.Peripherals.Speakers;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Audio
{
    /// <summary>
    /// A class for playing game sounds using an IToneGenerator
    /// </summary>
    public class GameSounds
    {
        private readonly IToneGenerator toneGenerator;
        private readonly int defaultDuration = 100;
        private readonly int defaultPause = 50;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameSounds"/> class
        /// </summary>
        /// <param name="toneGenerator">The <see cref="IToneGenerator"/> object to use for audio playback</param>
        public GameSounds(IToneGenerator toneGenerator)
        {
            this.toneGenerator = toneGenerator;
        }

        /// <summary>
        /// Plays the specified sound effect
        /// </summary>
        /// <param name="effect">The sound effect to play</param>
        public Task PlayEffect(GameSoundEffect effect)
        {
            return effect switch
            {
                GameSoundEffect.Activation => PlayActivation(),
                GameSoundEffect.Blip => PlayBlip(),
                GameSoundEffect.BossBattle => PlayBossBattle(),
                GameSoundEffect.ButtonPress => PlayButtonPress(),
                GameSoundEffect.Coin => PlayCoin(),
                GameSoundEffect.Collectible => PlayCollectible(),
                GameSoundEffect.Countdown => PlayCountdown(),
                GameSoundEffect.EnemyDeath => PlayEnemyDeath(),
                GameSoundEffect.Explosion => PlayExplosion(),
                GameSoundEffect.Footstep => PlayFootstep(),
                GameSoundEffect.GameOver => PlayGameOver(),
                GameSoundEffect.Health => PlayHealth(),
                GameSoundEffect.Hit => PlayHit(),
                GameSoundEffect.Jump => PlayJump(),
                GameSoundEffect.Laser => PlayLaser(),
                GameSoundEffect.LevelComplete => PlayLevelComplete(),
                GameSoundEffect.MenuNavigate => PlayMenuNavigate(),
                GameSoundEffect.PowerDown => PlayPowerDown(),
                GameSoundEffect.PowerUp => PlayPowerUp(),
                GameSoundEffect.SecretFound => PlaySecretFound(),
                GameSoundEffect.Teleport => PlayTeleport(),
                GameSoundEffect.Victory => PlayVictory(),
                GameSoundEffect.Warning => PlayWarning(),
                GameSoundEffect.WeaponSwitch => PlayWeaponSwitch(),
                _ => throw new ArgumentException("Invalid game sound effect specified.", nameof(effect)),
            };
        }


        /// <summary>
        /// Plays a simple blip sound effect
        /// </summary>
        private async Task PlayBlip()
        {
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration));
        }

        /// <summary>
        /// Plays a power-up or item pick-up sound effect
        /// </summary>
        private async Task PlayPowerUp()
        {
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(1760), TimeSpan.FromMilliseconds(defaultDuration));
        }

        /// <summary>
        /// Plays a coin or currency collection sound effect
        /// </summary>
        private async Task PlayCoin()
        {
            await toneGenerator.PlayTone(new Frequency(1047), TimeSpan.FromMilliseconds(defaultDuration));
        }

        /// <summary>
        /// Plays a jump or hop sound effect
        /// </summary>
        private async Task PlayJump()
        {
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration));
        }

        /// <summary>
        /// Plays a hit or damage sound effect
        /// </summary>
        private async Task PlayHit()
        {
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(220), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays a laser or projectile firing sound effect
        /// </summary>
        private async Task PlayLaser()
        {
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(1760), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays an explosion or destruction sound effect
        /// </summary>
        private async Task PlayExplosion()
        {
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(220), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(110), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(55), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays a game over or failure sound effect
        /// </summary>
        private async Task PlayGameOver()
        {
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(220), TimeSpan.FromMilliseconds(defaultDuration));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(110), TimeSpan.FromMilliseconds(defaultDuration));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(55), TimeSpan.FromMilliseconds(defaultDuration));
        }

        /// <summary>
        /// Plays a victory or success sound effect
        /// </summary>
        private async Task PlayVictory()
        {
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(659), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(1175), TimeSpan.FromMilliseconds(defaultDuration / 4));
        }

        /// <summary>
        /// Plays a countdown or timer sound effect
        /// </summary>
        private async Task PlayCountdown()
        {
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(783.99), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(698.46), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(587.33), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(523.25), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays a power-down sound effect
        /// </summary>
        private async Task PlayPowerDown()
        {
            await toneGenerator.PlayTone(new Frequency(523.25), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(349.23), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays a button press sound effect
        /// </summary>
        private async Task PlayButtonPress()
        {
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration));
        }

        /// <summary>
        /// Plays a menu navigation sound effect
        /// </summary>
        private async Task PlayMenuNavigate()
        {
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration / 4));
        }

        /// <summary>
        /// Plays a collectible item sound effect
        /// </summary>
        private async Task PlayCollectible()
        {
            await toneGenerator.PlayTone(new Frequency(1047), TimeSpan.FromMilliseconds(defaultDuration));
        }

        /// <summary>
        /// Plays a boss battle theme.
        /// </summary>
        private async Task PlayBossBattle()
        {
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(554.37), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(659.25), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(783.99), TimeSpan.FromMilliseconds(defaultDuration / 4));
        }

        /// <summary>
        /// Plays a secret found sound effect
        /// </summary>
        private async Task PlaySecretFound()
        {
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(1174.66), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(1396.91), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays a level complete sound effect
        /// </summary>
        private async Task PlayLevelComplete()
        {
            await toneGenerator.PlayTone(new Frequency(1047), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(1396.91), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(1760), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(2217.46), TimeSpan.FromMilliseconds(defaultDuration / 4));
        }

        /// <summary>
        /// Plays a weapon switch sound effect
        /// </summary>
        private async Task PlayWeaponSwitch()
        {
            await toneGenerator.PlayTone(new Frequency(523.25), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(349.23), TimeSpan.FromMilliseconds(defaultDuration / 4));
        }

        /// <summary>
        /// Plays a warning or alarm sound effect
        /// </summary>
        private async Task PlayWarning()
        {
            await toneGenerator.PlayTone(new Frequency(1047), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(783.99), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays a teleport or warp sound effect
        /// </summary>
        private async Task PlayTeleport()
        {
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(783.99), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(659.25), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(523.25), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays a health pickup or healing sound effect
        /// </summary>
        private async Task PlayHealth()
        {
            await toneGenerator.PlayTone(new Frequency(622.25), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(659.25), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(698.46), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(783.99), TimeSpan.FromMilliseconds(defaultDuration / 4));
        }

        /// <summary>
        /// Plays a footstep or movement sound effect
        /// </summary>
        private async Task PlayFootstep()
        {
            await toneGenerator.PlayTone(new Frequency(196), TimeSpan.FromMilliseconds(defaultDuration / 8));
        }

        /// <summary>
        /// Plays an item activation or use sound effect
        /// </summary>
        private async Task PlayActivation()
        {
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 4));
        }

        /// <summary>
        /// Plays an enemy death or defeat sound effect
        /// </summary>
        private async Task PlayEnemyDeath()
        {
            await toneGenerator.PlayTone(new Frequency(1568), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(1244.51), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(1046.5), TimeSpan.FromMilliseconds(defaultDuration / 2));
            await toneGenerator.PlayTone(new Frequency(783.99), TimeSpan.FromMilliseconds(defaultDuration / 2));
        }

        /// <summary>
        /// Plays a splash sound effect
        /// </summary>
        public async Task PlaySplash()
        {
            await toneGenerator.PlayTone(new Frequency(220), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(880), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(440), TimeSpan.FromMilliseconds(defaultDuration / 4));
            await Task.Delay(TimeSpan.FromMilliseconds(defaultPause));
            await toneGenerator.PlayTone(new Frequency(220), TimeSpan.FromMilliseconds(defaultDuration / 4));
        }
    }
}
using Audio.MicroAudio_Sample.SongPlayer;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio;
using Meadow.Peripherals.Speakers;
using System;
using System.Threading.Tasks;

namespace MicroAudio_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private MicroAudio audio;

        IToneGenerator speaker;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            speaker = new PiezoSpeaker(Device.Pins.D11);

            audio = new MicroAudio(speaker);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Play happy birthday");
            await HappyBirthDay(speaker);

            await Task.Delay(1000);

            Resolver.Log.Info("Play C scale");
            var scale = new CScale();
            await audio.PlaySong(scale);

            await Task.Delay(1000);

            Resolver.Log.Info("Sound effects test");
            await SoundEffectsTest();

            Resolver.Log.Info("Game effects test");
            await GameEffectsTest();
        }

        Task HappyBirthDay(IToneGenerator speaker)
        {
            var happyBirthday = new Song();
            happyBirthday.AddNote(new Note(Pitch.C, 3, NoteDuration.Quarter));
            happyBirthday.AddNote(new Note(Pitch.C, 3, NoteDuration.Quarter));
            happyBirthday.AddNote(new Note(Pitch.D, 3, NoteDuration.Half));
            happyBirthday.AddNote(new Note(Pitch.C, 3, NoteDuration.Half));
            happyBirthday.AddNote(new Note(Pitch.F, 3, NoteDuration.Half));
            happyBirthday.AddNote(new Note(Pitch.E, 3, NoteDuration.Whole));

            return happyBirthday.Play(speaker, 160);
        }

        async Task GameEffectsTest()
        {
            foreach (GameSoundEffect effect in Enum.GetValues(typeof(GameSoundEffect)))
            {
                Resolver.Log.Info($"Playing {effect} game effect...");
                await audio.PlayGameSound(effect);
                await Task.Delay(1000);
            }

            Resolver.Log.Info("Sound effects demo complete.");
        }

        async Task SoundEffectsTest()
        {
            foreach (SystemSoundEffect effect in Enum.GetValues(typeof(SystemSoundEffect)))
            {
                Resolver.Log.Info($"Playing {effect} sound effect...");
                await audio.PlaySystemSound(effect);
                await Task.Delay(1000);
            }

            Resolver.Log.Info("Sound effects demo complete.");
        }
    }
}
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio.Mp3;
using System;
using System.Threading.Tasks;

namespace Audio.Mp3.Yx5300_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Yx5300 mp3Player;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            mp3Player = new Yx5300(Device, Device.PlatformOS.GetSerialPortName("COM4"));

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            mp3Player.SetVolume(15);

            var status = await mp3Player.GetStatus();
            Resolver.Log.Info($"Status: {status}");

            var count = await mp3Player.GetNumberOfTracksInFolder(0);
            Resolver.Log.Info($"Number of tracks: {count}");

            mp3Player.Play();

            await Task.Delay(5000); //leave playing for 5 seconds

            mp3Player.Next();

            await Task.Delay(5000); //leave playing for 5 seconds
        }

        //<!=SNOP=>
    }
}
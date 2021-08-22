using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio.Mp3;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        //<!—SNIP—>

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            var mp3Player = new Yx5300(Device, Device.SerialPortNames.Com4);

            //using an async task - this code would likely go in an async method
            Task.Run(async () =>
            {
                mp3Player.SetVolume(15);

                var status = await mp3Player.GetStatus();
                Console.WriteLine($"Status: {status}");

                var count = await mp3Player.GetNumberOfTracksInFolder(0);
                Console.WriteLine($"Number of tracks: {count}");

                mp3Player.Play();

                await Task.Delay(5000); //leave playing for 5 seconds

                mp3Player.Next();

                await Task.Delay(5000); //leave playing for 5 seconds
            });
        }

        //<!—SNOP—>
    }
}
using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio;

namespace Audio.PiezoSpeaker_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {        
        protected PiezoSpeaker piezoSpeaker;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            piezoSpeaker = new PiezoSpeaker(Device.CreatePwmPort(Device.Pins.D05));

            _ = PlayTriad();
        }

        async Task PlayTriad()
        {
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Playing A major triad starting at A4");
                await piezoSpeaker.PlayTone(440, 500); //A
                await piezoSpeaker.PlayTone(554.37f, 500); //C#
                await piezoSpeaker.PlayTone(659.25f, 500); //E

                await Task.Delay(2500);
            }
        }
    }
}
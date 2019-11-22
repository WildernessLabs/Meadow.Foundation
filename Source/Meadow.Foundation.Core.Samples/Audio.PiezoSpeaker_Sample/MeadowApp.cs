using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio;

namespace Audio.PiezoSpeaker_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {        
        protected PiezoSpeaker piezoSpeaker;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            piezoSpeaker = new PiezoSpeaker(Device.CreatePwmPort(Device.Pins.D05));

            TestPiezoSpeaker();
        }

        protected void TestPiezoSpeaker()
        {
            Console.WriteLine("TestPiezoSpeaker...");

            while (true)
            {
                Console.WriteLine("Playing A4 note!");
                piezoSpeaker.PlayTone(440, 1000);
                piezoSpeaker.StopTone();
                Thread.Sleep(500);
            }
        }
    }
}
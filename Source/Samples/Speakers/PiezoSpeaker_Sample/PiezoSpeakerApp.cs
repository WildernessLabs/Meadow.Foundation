using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Generators;
using Meadow.Hardware;
using System;
using System.Threading;

namespace PiezoSpeaker_Sample
{
    public class PiezoSpeakerApp : App<F7Micro, PiezoSpeakerApp>
    {
        readonly IDigitalOutputPort port;
        readonly IPwmPort pwm; 
        readonly PiezoSpeaker piezoSpeaker;

        public PiezoSpeakerApp()
        {
            pwm = Device.CreatePwmPort(Device.Pins.D05);
            piezoSpeaker = new PiezoSpeaker(pwm);

            TestPiezoSpeaker();
        }

        protected void TestPiezoSpeaker()
        {
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
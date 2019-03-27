using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio;
using Meadow.Foundation.Generators;
using Meadow.Hardware;
using System;
using System.Threading;

namespace PiezoSpeaker_Sample
{
    public class PiezoSpeakerApp : AppBase<F7Micro, PiezoSpeakerApp>
    {
        IDigitalOutputPort port;
        SoftPwmPort pwm; // TODO: get rid of this when we get hadware PWM working.
        PiezoSpeaker piezoSpeaker;

        public PiezoSpeakerApp()
        {
            port = Device.CreateDigitalOutputPort(Device.Pins.D02);
            pwm = new SoftPwmPort(port);
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
using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;

namespace Leds.APA102_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Apa102 apa102;
        int numberOfLeds = 8;
        float maxBrightness = 0.25f;

        public MeadowApp()
        {
            Initialize();

            while(true){
                SetColor(Colors.ChileanFire, maxBrightness);
                Thread.Sleep(1000);
                SetColor(Colors.PearGreen, maxBrightness);
                Thread.Sleep(1000);
                Pulse(Colors.AzureBlue, 10);
                WalkTheStrip(Colors.ChileanFire, 10);
            }
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            apa102 = new Apa102(Device.CreateSpiBus(48000), numberOfLeds, Apa102.PixelOrder.BGR);
            Console.WriteLine("Hardware initialized.");
        }

        /// <summary>
        /// Sets the entire strip to be one color.
        /// </summary>
        /// <param name="color"></param>
        void SetColor(Color color, float brightness)
        {
            Console.WriteLine($"SetColor(color:{color}");
            for (int i = 0; i < apa102.NumberOfLeds; i++) {
                apa102.SetLed(i, color, brightness);
            }
            apa102.Show();
        }

        /// <summary>
        /// pulses the entire strip up and down in brightness
        /// </summary>
        /// <param name="color"></param>
        void Pulse(Color color, int numberOfPulses)
        {
            Console.WriteLine("Pulse");

            float minimumBrightness = 0.05f;
            float brightness = minimumBrightness;
            float increment = 0.01f; // the colors don't seem to have more resolution than this.
            bool forward = true;
            int pulsesPerLoop = (int)((maxBrightness / increment) * 2);
            //Console.WriteLine($"pulses per loop: {pulsesPerLoop}");
            int totalNumberOfPulses = numberOfPulses * pulsesPerLoop;

            for (int loop = 0; loop < totalNumberOfPulses; loop++)
            {
                // set all the leds one color.
                for (int i = 0; i < apa102.NumberOfLeds; i++) {
                    apa102.SetLed(i, color, brightness);
                }

                // increment/decrement our brightness depending on which direction
                // we're going
                if (forward) { brightness += increment; }
                else { brightness -= increment; }

                // check where we're at to determine direction.
                if (brightness <= minimumBrightness) {
                    forward = true;
                }
                if (brightness >= maxBrightness) {
                    forward = false;
                }

                apa102.Show();

                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Walks a lighted LED, up and down the strip.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="numberOfTraverses"></param>
        void WalkTheStrip(Color color, int numberOfTraverses)
        {
            int last = numberOfLeds - 1;

            bool forward = true;
            int index = 0;

            for (int loop = 0; loop < numberOfTraverses * apa102.NumberOfLeds * 2; loop++)
            {
                if (last != 9999) { apa102.SetLed(last, Color.Black); }
                apa102.SetLed(index, color);
                last = index;

                if(forward) { index++; }
                else { index--; }

                apa102.Show();

                if(index == apa102.NumberOfLeds - 1) { forward = false; }
                if(index == 0) { forward = true; }

                Thread.Sleep(50);
            }

            //for (int i = 0; i < apa102.NumberOfLeds; i++)
            //{
            //    if(last != 9999) { apa102.SetLed(last, Color.Black); }
            //    apa102.SetLed(i, Color.Turquoise);
            //    last = i;
            //    apa102.Show();
            //}
        }

        void Run()
        {
            Console.WriteLine("Run...");
            apa102.Clear();
            apa102.Show();

            Thread.Sleep(2000);
            apa102.SetLed(0, Color.Red, 0.5f);
            apa102.SetLed(1, Color.White);
            apa102.SetLed(2, Color.Blue);

            Thread.Sleep(2000);
            apa102.Show();

            Thread.Sleep(2000);
            apa102.AutoWrite = true;
            apa102.SetLed(0, Color.Green);
            apa102.SetLed(1, Color.Yellow);
            apa102.SetLed(2, Color.Pink);

            Thread.Sleep(5000);
            apa102.Clear();
        }

        public static class Colors
        {
            public static Color AzureBlue = Color.FromHex("#23abe3");
            public static Color ChileanFire = Color.FromHex("#EF7D3B");
            public static Color PearGreen = Color.FromHex("#C9DB31");
        }
    }
}
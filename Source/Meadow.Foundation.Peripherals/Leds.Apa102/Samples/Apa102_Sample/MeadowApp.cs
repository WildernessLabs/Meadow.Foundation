using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leds.APA102_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Apa102 apa102;
        int numberOfLeds = 256;
        float maxBrightness = 0.25f;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");
            apa102 = new Apa102(Device.CreateSpiBus(Apa102.DefaultSpiBusSpeed), numberOfLeds, Apa102.PixelOrder.BGR);

            return base.Initialize();
        }

        public override Task Run()
        {
            apa102.Clear();

            apa102.SetLed(index: 0, color: Color.Red, brightness: 0.5f);
            apa102.SetLed(index: 1, color: Color.Purple, brightness: 0.6f);
            apa102.SetLed(index: 2, color: Color.Blue, brightness: 0.7f);
            apa102.SetLed(index: 3, color: Color.Green, brightness: 0.8f);
            apa102.SetLed(index: 4, color: Color.Yellow, brightness: 0.9f);
            apa102.SetLed(index: 5, color: Color.Orange, brightness: 1.0f);

            apa102.Show();

            Apa102Tests();

            return Task.CompletedTask;
        }

        //<!=SNOP=>

        void Apa102Tests()
        {
            while (true)
            {
                SetColor(Colors.ChileanFire, maxBrightness);
                Thread.Sleep(1000);
                SetColor(Colors.PearGreen, maxBrightness);
                Thread.Sleep(1000);
                Pulse(Colors.AzureBlue, 10);
                WalkTheStrip(Colors.ChileanFire, 10);
            }
        }

        /// <summary>
        /// Sets the entire strip to be one color.
        /// </summary>
        /// <param name="color"></param>
        void SetColor(Color color, float brightness)
        {
            Console.WriteLine($"SetColor(color:{color}");

            for (int i = 0; i < apa102.NumberOfLeds; i++) 
            {
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

            float minBrightness = 0.05f;
            float brightness = minBrightness;
            float increment = 0.01f; // the colors don't seem to have more resolution than this.
            bool forward = true;

            int pulsesPerLoop = (int)(maxBrightness / increment * 2);
            int totalNumberOfPulses = numberOfPulses * pulsesPerLoop;

            for (int loop = 0; loop < totalNumberOfPulses; loop++)
            {
                // increment/decrement our brightness depending on direction
                if (forward) { brightness += increment; }
                else { brightness -= increment; }

                if (brightness <= minBrightness)
                {
                    forward = true;
                }
                if (brightness >= maxBrightness)
                {
                    forward = false;
                }

                // set all the leds one color
                for (int i = 0; i < apa102.NumberOfLeds; i++)
                {
                    apa102.SetLed(i, color, brightness);
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
        }

        void Start()
        {
            Console.WriteLine("Run...");
            apa102.Clear();
            apa102.Show();
            Thread.Sleep(2000);

            apa102.SetLed(0, Color.Red, 0.5f);
            apa102.SetLed(1, Color.White);
            apa102.SetLed(2, Color.Blue);
            apa102.Show();
            Thread.Sleep(2000);
            
            apa102.SetLed(0, Color.Green);
            apa102.SetLed(1, Color.Yellow);
            apa102.SetLed(2, Color.Pink);
            apa102.Show();
            Thread.Sleep(5000);
            
            apa102.Clear(true);
        }

        public static class Colors
        {
            public static Color AzureBlue = Color.FromHex("#23abe3");
            public static Color ChileanFire = Color.FromHex("#EF7D3B");
            public static Color PearGreen = Color.FromHex("#C9DB31");
        }
    }
}
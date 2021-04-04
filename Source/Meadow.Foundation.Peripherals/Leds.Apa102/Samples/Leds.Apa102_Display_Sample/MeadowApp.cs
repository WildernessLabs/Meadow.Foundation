using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using System;
using System.Threading;

namespace Leds.Apa102_Display_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Apa102 display;

        GraphicsLibrary canvas;

        public MeadowApp()
        {
            Initialize();

            canvas.CurrentFont = new Font4x8();

            while (true) {
                canvas.Clear();
                canvas.DrawText(0, 1, "MEADOW", Colors.AzureBlue);
                canvas.DrawText(24, 1, "F7", Colors.ChileanFire);
                canvas.Show();

                Thread.Sleep(1000);

                canvas.Clear();
                canvas.DrawText(0, 1, "Rocks", Colors.PearGreen);
                canvas.Show();

                Thread.Sleep(1000);
            }
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            display = new Apa102(Device.CreateSpiBus(48000), Apa102.PixelOrder.BGR, false, 32, 8);
            canvas = new GraphicsLibrary(display);

            Console.WriteLine("Hardware intitialized.");
        }

        static class Colors
        {
            public static Color AzureBlue {
                get {
                    var azureBlue = Color.FromHex("#23abe3");
                    // make it way less bright
                    return Color.FromHsba(azureBlue.Hue, azureBlue.Saturation, 0.025);
                }
            }
            public static Color ChileanFire {
                get {
                    var chileanFire = Color.FromHex("#ef7d3b");
                    // make it way less bright
                    return Color.FromHsba(chileanFire.Hue, chileanFire.Saturation, 0.025);
                }
            }
            public static Color PearGreen {
                get {
                    var PearGreen = Color.FromHex("#c9db31");
                    // make it way less bright
                    return Color.FromHsba(PearGreen.Hue, PearGreen.Saturation, 0.025);
                }
            }
        }
    }
}
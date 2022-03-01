using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using System;
using System.Threading;

namespace Leds.Pca9633_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>
        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            var driver = new Pca9633(Device.CreateI2cBus());

            //set the location of R,G,B leds for color control
            driver.SetRgbLedPositions(redLed: Pca9633.LedPosition.Led2, 
                                      greenLed: Pca9633.LedPosition.Led1, 
                                      blueLed: Pca9633.LedPosition.Led0);

            //set a single color
            driver.SetColor(Color.Red);
            Thread.Sleep(1000);
            driver.SetColor(Color.Blue);
            Thread.Sleep(1000);
            driver.SetColor(Color.Yellow);
        }
        //<!—SNOP—>
    }
}
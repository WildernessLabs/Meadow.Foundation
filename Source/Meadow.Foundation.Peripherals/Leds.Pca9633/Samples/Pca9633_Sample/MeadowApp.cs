using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leds.Pca9633_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Pca9633 pca9633;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            pca9633 = new Pca9633(Device.CreateI2cBus());

            return base.Initialize();
        }

        public override Task Run()
        {
            //set the location of R,G,B leds for color control
            pca9633.SetRgbLedPositions(redLed: Pca9633.LedPosition.Led2,
                                      greenLed: Pca9633.LedPosition.Led1,
                                      blueLed: Pca9633.LedPosition.Led0);

            //set a single color
            pca9633.SetColor(Color.Red);
            Thread.Sleep(1000);
            pca9633.SetColor(Color.Blue);
            Thread.Sleep(1000);
            pca9633.SetColor(Color.Yellow);

            return base.Run();
        }

        //<!=SNOP=>
    }
}
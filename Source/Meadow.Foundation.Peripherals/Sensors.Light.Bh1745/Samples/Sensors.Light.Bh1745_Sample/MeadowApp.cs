using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Light;
using static Meadow.Peripherals.Leds.IRgbLed;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Bh1745 sensor;

        RgbPwmLed rgbLed;

        public double GetQuantizedValue(double value)
        {
            if (value < 0.3)
                return 0;
            if (value < 0.8)
                return 0.5;
            return 1;
        }

        public MeadowApp()
        {
            Init();

            Thread.Sleep((int)sensor.MeasurementTime);

            Console.WriteLine("Read color values");

            Color color;

            while (true)
            {
                color = sensor.GetColor();

                Console.WriteLine($"Color: {color.R}, {color.G}, {color.B}");

                //quantize color for RGB to make color detection more obvious
                color = new Color(GetQuantizedValue(color.R),
                                  GetQuantizedValue(color.G),
                                  GetQuantizedValue(color.B));
                
                rgbLed.SetColor(color);

                Thread.Sleep(100);
            }
        }

        public void Init()
        {
            Console.WriteLine("Init...");

            sensor = new Bh1745(Device.CreateI2cBus());

            rgbLed = new RgbPwmLed(
                Device,
                Device.Pins.OnboardLedRed,
                Device.Pins.OnboardLedGreen,
                Device.Pins.OnboardLedBlue,
                commonType: CommonType.CommonAnode);
        }
    }
}
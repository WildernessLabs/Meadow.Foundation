using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace Sensors.Light.Tsl2591_Sample
{
    public class MeadowApp
        : App<F7Micro, MeadowApp>
    {
        Tsl2591 tsl;

        public MeadowApp()
        {
            var bus = Device.CreateI2cBus();

            tsl = new Tsl2591(bus);

            tsl.PowerOn();
            tsl.ChangeThreshold = 10;
            tsl.Updated += OnLightChange;
            tsl.StartUpdating(TimeSpan.FromSeconds(1));

            while (true)
            {
                Thread.Sleep(5000);
            }
        }

        private void OnLightChange(object _, IChangeResult<(Meadow.Units.Illuminance? FullSpectrum, Meadow.Units.Illuminance? Infrared, Meadow.Units.Illuminance? VisibleLight, Meadow.Units.Illuminance? Integrated)> e)
        {
            Console.WriteLine($"Light: {e.New.Integrated?.Lux} lux");
        }

    }
}
using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Sensors.Location.MediaTek;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mt3339 gps;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
        }

    }
}
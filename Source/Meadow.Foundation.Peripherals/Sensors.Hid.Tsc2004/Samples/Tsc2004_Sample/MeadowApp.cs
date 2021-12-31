using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bbq10Keyboard_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //Snip

        Tsc2004 touchScreen;

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.Fast, 0);

            touchScreen = new Tsc2004(i2cBus)
            {
                DisplayWidth = 240,
                DisplayHeight = 320,
                XMin = 260,
                XMax = 3803,
                YMin = 195,
                YMax = 3852,
                Rotation = RotationType._90Degrees
            };

            Task.Run(() =>
            {
                Point3d pt;

                while (true)
                {
                    if (touchScreen.IsTouched())
                    {
                        pt = touchScreen.GetPoint();
                        Console.WriteLine($"Location: X:{pt.X}, Y:{pt.Y}, Z:{pt.Z}");
                    }

                    Thread.Sleep(0);
                }
            });
        }

        //Snop
    }
}
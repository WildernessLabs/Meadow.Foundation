using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Foundation.Servos;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        GraphicsLibrary graphics;
        St7789 display;
        Vl53l0x sensor;
        Servo servo;

        public MeadowApp()
        {
            Initialize();

            Draw();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            Console.WriteLine("Create Spi bus");

            var config = new SpiClockConfiguration(24000, SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Console.WriteLine("Create display driver instance");

            display = new St7789(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 135, height: 240);

            Console.WriteLine("Create graphics lib");

            graphics = new GraphicsLibrary(display);
            graphics.CurrentFont = new Font12x20();
            graphics.Rotation = GraphicsLibrary.RotationType._90Degrees;

            Console.WriteLine("Create time of flight sensor");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            sensor = new Vl53l0x(Device, i2cBus);
            sensor.StartUpdating(100);

            Console.WriteLine("Create servo");
        }

        void Draw()
        {
            int angle = 0;
            int increment = 2;

            while (true)
            {
                graphics.Clear();

                DrawRadar();

                graphics.DrawLine(120, 120, 105, (float)(angle * Math.PI / 180), Color.Yellow);
                angle += increment;
                if(angle >= 180) { increment = -2; }
                if(angle <= 0) { increment = 2; }

                graphics.DrawText(0, 0, $"{180 - angle}°", Color.Yellow);

                if (sensor?.Conditions?.Distance != null)
                {
                    graphics.DrawText(160, 0, $"{sensor.Conditions.Distance.Value}mm");
                }

                graphics.Show();

                Thread.Sleep(50);
            }
        }

        void DrawRadar ()
        {
            int xCenter = 120;
            int yCenter = 120;

            var radarColor = Color.LawnGreen;

            for (int i = 1; i < 5; i++)
            {
                graphics.DrawCircleQuadrant(xCenter, yCenter, 25 * i, 0, radarColor);
                graphics.DrawCircleQuadrant(xCenter, yCenter, 25 * i, 1, radarColor);
            }

            for(int i = 0; i < 7; i++)
            {
                graphics.DrawLine(xCenter, yCenter, 105, (float)(i * Math.PI / 6), radarColor);
            }
        }
    }
}
using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Foundation.Servos;
using Meadow.Hardware;
using Meadow.Units;
using LU = Meadow.Units.Length.UnitType;

namespace MeadowApp
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        MicroGraphics graphics;
        St7789 display;
        Vl53l0x sensor;

        float[] radarData = new float[181];

        public MeadowApp()
        {
            Initialize();

            Draw();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            Console.WriteLine("Create Spi bus");

            var config = new SpiClockConfiguration(new Frequency(24000, Frequency.UnitType.Kilohertz), SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Console.WriteLine("Create display driver instance");

            display = new St7789(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 135, height: 240);

            Console.WriteLine("Create graphics lib");

            graphics = new MicroGraphics(display);
            graphics.CurrentFont = new Font12x20();
            graphics.Rotation = RotationType._90Degrees;

            Console.WriteLine("Create time of flight sensor");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            sensor = new Vl53l0x(Device, i2cBus);
            sensor.StartUpdating(TimeSpan.FromMilliseconds(250));

            Console.WriteLine("Create servo");
        }

        void Draw()
        {
            int angle = 160;
            int increment = 4;
            int x, y = 0;

            while (true)
            {
                graphics.Clear();

                DrawRadar();

                graphics.DrawLine(120, 120, 105, (float)(angle * Math.PI / 180), Color.Yellow);
                
                if(angle >= 180) { increment = -4; }
                if(angle <= 0) { increment = 4; }

                angle += increment;

                graphics.DrawText(0, 0, $"{180 - angle}°", Color.Yellow);

                if (sensor?.Distance != null && sensor?.Distance.Value >= new Length(0, LU.Millimeters))
                {
                    graphics.DrawText(170, 0, $"{sensor.Distance?.Millimeters}mm", Color.Yellow);
                    radarData[angle] = (float)(sensor.Distance?.Millimeters / 2);
                }
                else
                {
                    Console.WriteLine("no data");
                    radarData[angle] = 0;
                } 

                for(int i = 0; i < 180; i++)
                {
                    x = 120 + (int)(radarData[i] * MathF.Cos(i * MathF.PI / 180f));
                    y = 120 - (int)(radarData[i] * MathF.Sin(i * MathF.PI / 180f));
                    //graphics.DrawPixel(x, y, Color.Yellow);
                    graphics.DrawCircle(x, y, 2,Color.Yellow, true);
                }

                graphics.Show();

                Thread.Sleep(100);
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
using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;

namespace Sensors.Distance.Vl53l0x_St7789_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        St7789 display;
        GraphicsLibrary graphics;
        Vl53l0x sensor;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            Console.WriteLine("Create Spi bus");

            var config = new SpiClockConfiguration(12000, SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Console.WriteLine("Create display driver instance");

            display = new St7789(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 135, height: 240);

            Console.WriteLine("Create graphics lib");

            graphics = new GraphicsLibrary(display);
            graphics.CurrentFont = new Font12x16();
            graphics.Clear();

            Console.WriteLine("Create time of flight sensor");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            sensor = new Vl53l0x(Device, i2cBus);

            Console.WriteLine("Start updating");
            sensor.Updated += Sensor_Updated;
            sensor.StartUpdating(100);

            Console.WriteLine("Init complete");
        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Distance.DistanceConditionChangeResult e)
        {
            if(e.New == null || e.New.Distance == null)
            {   
                return;   
            }

            Console.WriteLine($"{e.New.Distance.Value}mm");

            graphics.DrawRectangle(0, 0, 135, 33, Color.Black, true);
            graphics.DrawText(0, 0, $"{e.New.Distance.Value}mm", Color.White, GraphicsLibrary.ScaleFactor.X2);
            graphics.Show();
        }
    }
}
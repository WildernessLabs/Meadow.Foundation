using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Hid;
using Meadow.Hardware;

namespace Sensors.Distance.Mpr121_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mpr121 sensor;
        St7789 display;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Init();
        }

        public void Init()
        {
            Console.WriteLine("Init...");

            sensor = new Mpr121(Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Standard), 90, 100);
            sensor.ChannelStatusesChanged += Sensor_ChannelStatusesChanged;

            Console.WriteLine("Create Spi bus");

            var config = new SpiClockConfiguration(6000, SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            Console.WriteLine("Create display driver instance");

            display = new St7789(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 135, height: 240);

            Console.WriteLine("Create graphics lib");

            graphics = new GraphicsLibrary(display);
            graphics.Rotation = GraphicsLibrary.RotationType._90Degrees;
            graphics.CurrentFont = new Font12x16();
        }

        int rectW = 60;
        int rectH = 45;
        private void Sensor_ChannelStatusesChanged(object sender, ChannelStatusChangedEventArgs e)
        {
            string pads = string.Empty;

            graphics.Clear();

            for(int i = 0; i < e.ChannelStatus.Count; i++)
            {
                if(e.ChannelStatus[(Mpr121.Channels)i] == true)
                {
                    pads += i + ", ";

                    graphics.DrawRectangle((i % 4) * rectW, (i / 4) * rectH, rectW, rectH, Meadow.Foundation.Color.Cyan, true);
                }
            }

            if (string.IsNullOrEmpty(pads))
            {
                Console.WriteLine("none");
                graphics.DrawText(0, 0, "none", Meadow.Foundation.Color.Cyan);
            }
            else
            {
                Console.WriteLine(pads + "touched");
            }

            graphics.Show();
        }
    }
}
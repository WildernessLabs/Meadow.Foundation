using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.TextDisplayMenu;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Buttons;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        RgbPwmLed onboardLed;

        Menu menu;
        St7789 st7789;
        ITextDisplay display;

        IButton next = null;
        IButton previous = null;
        IButton select = null;

        public MeadowApp()
        {
            Initialize();
        }

        void UpdateDisplay()
        {

        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                3.3f, 3.3f, 3.3f,
                Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);

            Console.WriteLine("Create Display with SPI...");

            var config = new SpiClockConfiguration(6000, SpiClockConfiguration.Mode.Mode3);
            var spiBus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            st7789 = new St7789(device: Device, spiBus: spiBus,
                chipSelectPin: Device.Pins.D12,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                width: 240, height: 240);

            Console.WriteLine("Create GraphicsLibrary...");

            display = new GraphicsLibrary(st7789)
            {
                CurrentFont = new Font12x20(),
             //   Rotation = GraphicsLibrary.RotationType._270Degrees
            };

            var gl = display as GraphicsLibrary;
            gl.Clear();
            gl.DrawLine(0, 0, 40, 40, Meadow.Foundation.Color.Aqua);
            gl.Show();
            
            Console.WriteLine("Load menu data...");

            var menuData = LoadResource("menu.json");

            Console.WriteLine($"Data length: {menuData.Length}...");

            Console.WriteLine("Create buttons...");

            Console.WriteLine("Create menu...");

            menu = new Menu(display, menuData, false);

         //   next = new PushButton(Device, Device.Pins.D02);
          //  next.Clicked += (s, e) => { menu.OnNext(); };

          //  select = new PushButton(Device, Device.Pins.D04);
          //  select.Clicked += (s, e) => { menu.OnSelect(); };

          //  previous = new PushButton(Device, Device.Pins.D03);
         //   previous.Clicked += (s, e) => { menu.OnPrevious(); };

            Console.WriteLine("Enable menu...");

            menu.Enable();
        }

        byte[] LoadResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Displays.TextDisplayMenu_Sample.{filename}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
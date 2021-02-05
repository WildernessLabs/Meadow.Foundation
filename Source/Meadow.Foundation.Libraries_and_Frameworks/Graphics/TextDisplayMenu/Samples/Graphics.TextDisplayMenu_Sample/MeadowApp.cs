using System;
using System.IO;
using System.Reflection;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.TextDisplayMenu;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Foundation.Graphics;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        RgbPwmLed onboardLed;

        Menu menu;
    //    St7789 st7789;

        Ssd1309 ssd1309;
        ITextDisplay display;

        IButton next = null;
        IButton previous = null;
        IButton select = null;

        public MeadowApp()
        {
            Initialize();
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

            var config = new SpiClockConfiguration(12000, SpiClockConfiguration.Mode.Mode0);

            var bus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            ssd1309 = new Ssd1309
            (
                device: Device,
                spiBus: bus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00
            );

            Console.WriteLine("Create GraphicsLibrary...");

            var gl = new GraphicsLibrary(ssd1309)
            {
                CurrentFont = new Font8x12(),
            };

            gl.Clear();
            gl.DrawText(0, 0, "Loading Menu");
            gl.Show();

            display = gl as ITextDisplay;

            Console.WriteLine("Load menu data...");

            var menuData = LoadResource("menu.json");

            Console.WriteLine($"Data length: {menuData.Length}...");

            Console.WriteLine("Create buttons...");

            Console.WriteLine("Create menu...");

            menu = new Menu(display, menuData, false);

            next = new PushButton(Device, Device.Pins.D10, ResistorMode.Disabled);
            next.Clicked += (s, e) => { menu.Next(); };

            select = new PushButton(Device, Device.Pins.D11, ResistorMode.Disabled);
            select.Clicked += (s, e) => { menu.Select(); };

            previous = new PushButton(Device, Device.Pins.D12, ResistorMode.Disabled);
            previous.Clicked += (s, e) => { menu.Previous(); };

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
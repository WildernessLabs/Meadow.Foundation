using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Displays.UI;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Buttons;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace TextDisplayMenu_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        TextDisplayMenu menu;

        IButton next = null;
        IButton previous = null;
        IButton select = null;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            // SSD1309 with SPI
            var ssd1309 = new Ssd1309
            (
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00
            );

            // SSD1309 with I2C
            //var ssd1309 = new Ssd1309
            //(
            //    Device.CreateI2cBus()
            //);

            var microGraphics = new MicroGraphics(ssd1309)
            {
                CurrentFont = new Font8x12(),
            };

            microGraphics.Clear();
            microGraphics.DrawText(0, 0, "Loading Menu");
            microGraphics.Show();

            Resolver.Log.Info("Load menu data...");

            var menuData = LoadResource("menu.json");

            Resolver.Log.Info($"Data length: {menuData.Length}...");

            Resolver.Log.Info("Create buttons...");

            Resolver.Log.Info("Create menu...");

            menu = new TextDisplayMenu(microGraphics, menuData, false);

            next = new PushButton(Device.Pins.D10);
            next.Clicked += (s, e) => { menu.Next(); };

            select = new PushButton(Device.Pins.D11);
            select.Clicked += (s, e) => { menu.Select(); };

            previous = new PushButton(Device.Pins.D12);
            previous.Clicked += (s, e) => { menu.Previous(); };

            Resolver.Log.Info("Enable menu...");

            menu.Enable();

            return Task.CompletedTask;
        }

        byte[] LoadResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"TextDisplayMenu_Sample.{filename}";

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
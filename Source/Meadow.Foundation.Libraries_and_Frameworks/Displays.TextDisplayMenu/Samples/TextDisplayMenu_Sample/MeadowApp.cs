using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Displays.TextDisplayMenu;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Buttons;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        Menu menu;

        Ssd1309 ssd1309;

        IButton next = null;
        IButton previous = null;
        IButton select = null;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var bus = Device.CreateSpiBus();

            ssd1309 = new Ssd1309
            (
                spiBus: bus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00
            );

            Resolver.Log.Info("Create MicroGraphics...");

            var gl = new MicroGraphics(ssd1309)
            {
                CurrentFont = new Font8x12(),
            };

            gl.Clear();
            gl.DrawText(0, 0, "Loading Menu");
            gl.Show();

            Resolver.Log.Info("Load menu data...");

            var menuData = LoadResource("menu.json");

            Resolver.Log.Info($"Data length: {menuData.Length}...");

            Resolver.Log.Info("Create buttons...");

            Resolver.Log.Info("Create menu...");

            menu = new Menu(ssd1309 as ITextDisplay, menuData, false);

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
            var resourceName = $"Graphics.TextDisplayMenu_Sample.{filename}";

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
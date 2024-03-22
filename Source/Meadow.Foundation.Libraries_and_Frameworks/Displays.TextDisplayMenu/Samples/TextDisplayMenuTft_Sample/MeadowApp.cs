using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Displays.UI;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Buttons;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace TextDisplayMenuTft_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        TextDisplayMenu menu;

        IButton next = null;
        IButton previous = null;
        IButton select = null;
        IButton back = null;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var display = new St7789
            (
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.A03,
                dcPin: Device.Pins.A04,
                resetPin: Device.Pins.A05,
                240, 240
            );

            var microGraphics = new MicroGraphics(display)
            {
                CurrentFont = new Font12x16(),
                Rotation = RotationType._270Degrees
            };

            microGraphics.Clear();
            microGraphics.DrawText(0, 0, "Loading Menu");
            microGraphics.Show();

            Resolver.Log.Info("Load menu data...");

            var menuData = LoadResource("menu.json");

            Resolver.Log.Info($"Data length: {menuData.Length}...");

            Resolver.Log.Info("Create menu...");

            menu = new TextDisplayMenu(microGraphics, menuData, false);
            menu.ValueChanged += Menu_ValueChanged;

            Resolver.Log.Info("Create buttons...");

            next = GetPushButton(Device.Pins.D02);
            next.Clicked += (s, e) => { menu.Next(); };

            select = GetPushButton(Device.Pins.D05);
            select.Clicked += (s, e) => { menu.Select(); };

            previous = GetPushButton(Device.Pins.D15);
            previous.Clicked += (s, e) => { menu.Previous(); };

            back = GetPushButton(Device.Pins.D10);
            back.Clicked += (s, e) => { menu.Back(); };

            Resolver.Log.Info("Enable menu...");

            menu.Enable();

            return Task.CompletedTask;
        }

        private void Menu_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Resolver.Log.Info($"Value changed for {e.ItemID} to {e.Value}");
        }

        byte[] LoadResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"TextDisplayMenuTft_Sample.{filename}";

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        private IButton GetPushButton(IPin pin)
        {
            if (pin.Supports<IDigitalChannelInfo>(c => c.InterruptCapable))
            {
                return new PushButton(pin, ResistorMode.InternalPullDown);
            }
            else
            {
                return new PollingPushButton(pin, ResistorMode.InternalPullDown);
            }
        }
    }
}
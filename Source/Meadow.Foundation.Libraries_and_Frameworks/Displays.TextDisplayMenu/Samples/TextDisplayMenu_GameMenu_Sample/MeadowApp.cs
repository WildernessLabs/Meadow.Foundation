using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Displays.UI;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Buttons;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace TextDisplayMenu_GameMenu_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        TextDisplayMenu menu;

        MicroGraphics microGraphics;

        IButton up = null;
        IButton down = null;
        IButton left = null;
        IButton right = null;

        bool playGame = false;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var bus = Device.CreateSpiBus();

            // SSD1309 with SPI
            var ssd1309 = new Ssd1309
            (
                spiBus: bus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00
            );

            // SSD1309 with I2C
            //var ssd1309 = new Ssd1309
            //(
            //    Device.CreateI2cBus()
            //);

            microGraphics = new MicroGraphics(ssd1309)
            {
                CurrentFont = new Font8x12(),
            };

            microGraphics.Clear();
            microGraphics.DrawText(0, 0, "Loading Menu");
            microGraphics.Show();

            Resolver.Log.Info("Load menu data...");

            var menuData = LoadResource("menu.json");

            Resolver.Log.Info("Create menu...");

            menu = new TextDisplayMenu(microGraphics, menuData, false);

            menu.Selected += Menu_Selected;

            Resolver.Log.Info("Create buttons...");

            up = new PushButton(Device.Pins.D09);
            up.Clicked += Up_Clicked;

            left = new PushButton(Device.Pins.D11);
            left.Clicked += Left_Clicked;

            right = new PushButton(Device.Pins.D10);
            right.Clicked += Right_Clicked;

            down = new PushButton(Device.Pins.D12);
            down.Clicked += Down_Clicked;

            Resolver.Log.Info("Enable menu...");

            menu.Enable();

            return Task.CompletedTask;
        }

        private void Up_Clicked(object sender, EventArgs e)
        {
            Resolver.Log.Info("Up_Clicked");
            if (menu.IsEnabled)
            {
                menu.Previous();
            }
        }

        private void Down_Clicked(object sender, EventArgs e)
        {
            Resolver.Log.Info("Down_Clicked");
            if (menu.IsEnabled)
            {
                menu.Next();
            }
        }

        private void Left_Clicked(object sender, EventArgs e)
        {
            Resolver.Log.Info("Left_Clicked");
            if (menu.IsEnabled == false)
            {
                playGame = false;
            }
        }

        private void Right_Clicked(object sender, EventArgs e)
        {
            Resolver.Log.Info("Right_Clicked");
            if (menu.IsEnabled)
            {
                menu.Select();
            }
        }

        private void Menu_Selected(object sender, MenuSelectedEventArgs e)
        {
            Resolver.Log.Info($"******** Selected: {e.Command}");

            DisableMenu();

            _ = StartGame(e.Command);
        }

        async Task StartGame(string command)
        {
            Resolver.Log.Info($"******** {command}");

            playGame = true;
            int count = 0;
            int x = 0, y = 0;
            int xD = 1, yD = 1;

            await Task.Run(() =>
            {
                while (count < 150 && playGame == true)
                {
                    microGraphics.Clear();
                    microGraphics.DrawText(0, 0, $"{command}:");
                    microGraphics.DrawText(0, 20, $"{count++}");
                    microGraphics.DrawPixel(x += xD, y += yD);
                    if (x == microGraphics.Width || x == 0) { xD *= -1; };
                    if (y == microGraphics.Height || y == 0) { yD *= -1; };
                    microGraphics.Show();
                }
            }).ConfigureAwait(false);

            EnableMenu();
        }

        void EnableMenu()
        {
            Resolver.Log.Info("Enable menu...");

            menu?.Enable();
        }

        void DisableMenu()
        {
            menu?.Disable();
        }

        byte[] LoadResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"TextDisplayMenu_GameMenu_Sample.{filename}";

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
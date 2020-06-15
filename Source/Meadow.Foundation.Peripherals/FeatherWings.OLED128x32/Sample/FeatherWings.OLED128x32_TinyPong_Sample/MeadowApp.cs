using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace FeatherWings.OLED128x32_TinyPong_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        OLED128x32Wing oledWing;
        GraphicsLibrary graphics;

        PongGame pongGame;

        public MeadowApp()
        {
            pongGame = new PongGame(128, 32);

            Initialize();
            StartGameLoop();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            oledWing = new OLED128x32Wing(i2cBus, Device, Device.Pins.D11, Device.Pins.D10, Device.Pins.D09);
            oledWing.ButtonB.Clicked += (sender, e) => pongGame?.PlayerUp();
            oledWing.ButtonC.Clicked += (sender, e) => pongGame?.PlayerDown();

            graphics = new GraphicsLibrary(oledWing.Display);
            graphics.CurrentFont = new Font8x12();
        }

        void StartGameLoop()
        {
            while(true)
            {
                pongGame.Update();

                graphics.Clear();

                graphics.DrawCircle(pongGame.BallX, pongGame.BallY, pongGame.BallRadius, true, true);
                graphics.DrawRectangle(pongGame.PlayerX, pongGame.PlayerY, pongGame.PaddleWidth, pongGame.PaddleHeight, true, true);
                graphics.DrawRectangle(pongGame.CpuX, pongGame.CpuY, pongGame.PaddleWidth, pongGame.PaddleHeight, true, true);

                graphics.DrawText(52, 0, $"{pongGame.PlayerScore}:{pongGame.CpuScore}");

                graphics.Show();

                Thread.Sleep(10);
            }
        }
    }
}
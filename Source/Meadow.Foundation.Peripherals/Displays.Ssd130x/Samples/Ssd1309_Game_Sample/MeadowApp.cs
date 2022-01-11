using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Ssd130x;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;

namespace Displays.Ssd130x.Ssd1309_Game_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        MicroGraphics graphics;
        Ssd1309 display;

     //   PushButton btnUp, btnDown, btnLeft, btnRight;

        IDigitalInterruptPort portLeft;
        IDigitalInterruptPort portUp;
        IDigitalInterruptPort portRight;
        IDigitalInterruptPort portDown;

        BreakoutGame breakoutGame;
        SnakeGame snakeGame;

        public MeadowApp()
        {
            Initialize();

            breakoutGame = new BreakoutGame(64, 128);
            StartBreakoutLoop();

            snakeGame = new SnakeGame(64, 128);
          //  StartSnakeLoop();

        }

        void StartBreakoutLoop()
        {
            Console.WriteLine("Start breakout");

            while (true)
            {
                breakoutGame.Update();

                graphics.Clear(false);

                graphics.DrawCircle(breakoutGame.Ball.X, breakoutGame.Ball.Y, breakoutGame.Ball.Radius, true, true);
                    

                graphics.DrawRectangle(breakoutGame.Paddle.X, breakoutGame.Paddle.Y,
                    breakoutGame.Paddle.Width, breakoutGame.Paddle.Height,
                    true);

                for (int i = 0; i < breakoutGame.Blocks.Length; i++)
                {
                    if (breakoutGame.Blocks[i].IsVisible)
                    {
                        graphics.DrawRectangle(breakoutGame.Blocks[i].X, breakoutGame.Blocks[i].Y,
                            breakoutGame.Blocks[i].Width, breakoutGame.Blocks[i].Height);
                    }
                }

                graphics.Show();
            }
        }

        void StartSnakeLoop()
        {
            while(true)
            {
                graphics.Clear();

                snakeGame.Update();
                //draw food
                graphics.DrawPixel(snakeGame.FoodPosition.X, snakeGame.FoodPosition.Y);

                //draw food
                for (int i = 0; i < snakeGame.SnakePosition.Count; i++)
                {
                    var point = (Point)snakeGame.SnakePosition[i];

                    graphics.DrawPixel(point.X, point.Y);
                }

             //   if (game.PlaySound)
             //       speaker.PlayTone(440, 25);

                //show
                graphics.Show();

                Thread.Sleep(250 - snakeGame.Level * 5);
            }
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            Console.WriteLine("Create Display with SPI...");

            var config = new SpiClockConfiguration(new Frequency(12000, Frequency.UnitType.Kilohertz), SpiClockConfiguration.Mode.Mode0);

            var bus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            display = new Ssd1309
            (
                device: Device,
                spiBus: bus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00
            );

            Console.WriteLine("Create Graphics Library...");

            graphics = new MicroGraphics(display);
            graphics.Rotation = RotationType._270Degrees;
            graphics.CurrentFont = new Font8x12();

            graphics.Clear();
            graphics.DrawText(0, 0, "Hello");
            graphics.Show();

            Console.WriteLine("Create buttons...");

            portLeft = Device.CreateDigitalInputPort(Device.Pins.D12, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown);
            portUp = Device.CreateDigitalInputPort(Device.Pins.D13, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown);
            portRight = Device.CreateDigitalInputPort(Device.Pins.D07, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown);
            portDown = Device.CreateDigitalInputPort(Device.Pins.D11, InterruptMode.EdgeFalling, ResistorMode.InternalPullDown);

            portRight.Changed += PortRight_Changed;
            portLeft.Changed += PortLeft_Changed;
            portUp.Changed += PortUp_Changed;
            portDown.Changed += PortDown_Changed;

            /*     btnUp = new PushButton(Device, Device.Pins.D13);
                 btnLeft = new PushButton(Device, Device.Pins.D12);
                 btnDown = new PushButton(Device, Device.Pins.D11);
                 btnRight = new PushButton(Device, Device.Pins.D10);

                 btnUp.Clicked += BtnUp_Clicked;
                 btnLeft.Clicked += BtnLeft_Clicked;
                 btnDown.Clicked += BtnDown_Clicked;
                 btnRight.Clicked += BtnRight_Clicked;  */

            Console.WriteLine("Initialize complete");

        }

        private void PortRight_Changed(object sender, DigitalPortResult e)
        {
            /*    graphics.Clear();
                graphics.DrawText(0, 0, "R" + count++);
                graphics.Show(); */
            //   snakeGame.Direction = SnakeDirection.Right;

            breakoutGame.Right(breakoutGame.Paddle.Width);
        }

        private void PortDown_Changed(object sender, DigitalPortResult e)
        {
            /*  graphics.Clear();
              graphics.DrawText(0, 0, "D" + count++);
              graphics.Show(); */
      //      snakeGame.Direction = SnakeDirection.Down;
        }

        private void PortUp_Changed(object sender, DigitalPortResult e)
        {
          /*  graphics.Clear();
            graphics.DrawText(0, 0, "U" + count++);
            graphics.Show(); */

        //    snakeGame.Direction = SnakeDirection.Up;

        }

        private void PortLeft_Changed(object sender, DigitalPortResult e)
        {
            /* graphics.Clear();
            graphics.DrawText(0, 0, "L" + count++);
            graphics.Show(); */

            //  snakeGame.Direction = SnakeDirection.Left;

            breakoutGame.Left(breakoutGame.Paddle.Width);
        }

        int count = 0;
        private void BtnRight_Clicked(object sender, EventArgs e)
        {
            graphics.Clear();
            graphics.DrawText(0, 0, "Right" + count++);
            graphics.Show();
        }

        private void BtnDown_Clicked(object sender, EventArgs e)
        {
            graphics.Clear();
            graphics.DrawText(0, 0, "Down" + count++);
            graphics.Show();
        }

        private void BtnLeft_Clicked(object sender, EventArgs e)
        {
            graphics.Clear();
            graphics.DrawText(0, 0, "Left" + count++);
            graphics.Show();
        }

        private void BtnUp_Clicked(object sender, EventArgs e)
        {
            graphics.Clear();
            graphics.DrawText(0, 0, "Up" + count++);
            graphics.Show();
        }

        void OutputText()
        {

        }
    }
}
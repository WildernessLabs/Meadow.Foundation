using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Ssd130x;
using Meadow.Foundation.Graphics;
using Meadow.Units;

namespace Displays.Ssd130x.Ssd1309_3DCube_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        MicroGraphics graphics;
        Ssd1309 display;

        //needs cleanup - quick port from c code
        double rot, rotationX, rotationY, rotationZ;
        double rotationXX, rotationYY, rotationZZ;
        double rotationXXX, rotationYYY, rotationZZZ;

        int[,] cubeWireframe = new int[12, 3];
        int[,] cubeVertices;

        public MeadowApp()
        {
            // CreateSpiDisplay();
            CreateI2CDisplay();

            /*   display.Clear(true);

               Console.WriteLine("Create Graphics Library");
               TestDisplayGraphicsAPI();
               Thread.Sleep(500); */

            int cubeSize = 20;

            cubeVertices = new int[8, 3] {
                 { -cubeSize, -cubeSize,  cubeSize},
                 {  cubeSize, -cubeSize,  cubeSize},
                 {  cubeSize,  cubeSize,  cubeSize},
                 { -cubeSize,  cubeSize,  cubeSize},
                 { -cubeSize, -cubeSize, -cubeSize},
                 {  cubeSize, -cubeSize, -cubeSize},
                 {  cubeSize,  cubeSize, -cubeSize},
                 { -cubeSize,  cubeSize, -cubeSize},
            };

            /*  cube_vertex = new int[8, 3] {
                   { -20, -20, front_depth},
                   {  20, -20, front_depth},
                   {  20,  20, front_depth},
                   { -20,  20, front_depth},
                   { -20, -20, back_depth},
                   {  20, -20, back_depth},
                   {  20,  20, back_depth},
                   { -20,  20, back_depth}
              };  */

            graphics = new MicroGraphics(display);

            Show3dCube();

            //  Grid();

        }

        void CreateSpiDisplay()
        {
            Console.WriteLine("Create Display with SPI...");

            var config = new Meadow.Hardware.SpiClockConfiguration(new Frequency(12000, Frequency.UnitType.Kilohertz), Meadow.Hardware.SpiClockConfiguration.Mode.Mode0);

            var bus = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, config);

            display = new Ssd1309
            (
                device: Device,
                spiBus: bus,
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00
            );
        }

        void CreateI2CDisplay()
        {
            Console.WriteLine("Create Display with I2C...");

            display = new Ssd1309
            (
                i2cBus: Device.CreateI2cBus(40000),
                address: 60
            );
        }

        void Show3dCube()
        {
            int originX = (int)display.Width / 2;
            int originY = (int)display.Height / 2;

            int angle = 0;
            while (true)
            {
                Console.WriteLine("Draw 3DCube frame");
                graphics.Clear();

                angle++;
                for (int i = 0; i < 8; i++)
                {
                    rot = angle * 0.0174532; //0.0174532 = one degree
                                             //rotateY

                    rotationZ = cubeVertices[i, 2] * Math.Cos(rot) - cubeVertices[i, 0] * Math.Sin(rot);
                    rotationX = cubeVertices[i, 2] * Math.Sin(rot) + cubeVertices[i, 0] * Math.Cos(rot);
                    rotationY = cubeVertices[i, 1];

                    //rotateX
                    rotationYY = rotationY * Math.Cos(rot) - rotationZ * Math.Sin(rot);
                    rotationZZ = rotationY * Math.Sin(rot) + rotationZ * Math.Cos(rot);
                    rotationXX = rotationX;
                    //rotateZ
                    rotationXXX = rotationXX * Math.Cos(rot) - rotationYY * Math.Sin(rot);
                    rotationYYY = rotationXX * Math.Sin(rot) + rotationYY * Math.Cos(rot);
                    rotationZZZ = rotationZZ;

                    //orthographic projection
                    rotationXXX = rotationXXX + originX;
                    rotationYYY = rotationYYY + originY;

                    //store new vertices values for wireframe drawing
                    cubeWireframe[i, 0] = (int)rotationXXX;
                    cubeWireframe[i, 1] = (int)rotationYYY;
                    cubeWireframe[i, 2] = (int)rotationZZZ;

                    DrawVertices();
                }

                DrawWireframe();

                graphics.Show();
            }
        }

        void DrawVertices()
        {
            graphics.DrawPixel((int)rotationXXX, (int)rotationYYY);
        }

        void DrawWireframe()
        {
            graphics.DrawLine(cubeWireframe[0, 0], cubeWireframe[0, 1], cubeWireframe[1, 0], cubeWireframe[1, 1], true);
            graphics.DrawLine(cubeWireframe[1, 0], cubeWireframe[1, 1], cubeWireframe[2, 0], cubeWireframe[2, 1], true);
            graphics.DrawLine(cubeWireframe[2, 0], cubeWireframe[2, 1], cubeWireframe[3, 0], cubeWireframe[3, 1], true);
            graphics.DrawLine(cubeWireframe[3, 0], cubeWireframe[3, 1], cubeWireframe[0, 0], cubeWireframe[0, 1], true);

            //cross face above
            graphics.DrawLine(cubeWireframe[1, 0], cubeWireframe[1, 1], cubeWireframe[3, 0], cubeWireframe[3, 1], true);
            graphics.DrawLine(cubeWireframe[0, 0], cubeWireframe[0, 1], cubeWireframe[2, 0], cubeWireframe[2, 1], true);

            graphics.DrawLine(cubeWireframe[4, 0], cubeWireframe[4, 1], cubeWireframe[5, 0], cubeWireframe[5, 1], true);
            graphics.DrawLine(cubeWireframe[5, 0], cubeWireframe[5, 1], cubeWireframe[6, 0], cubeWireframe[6, 1], true);
            graphics.DrawLine(cubeWireframe[6, 0], cubeWireframe[6, 1], cubeWireframe[7, 0], cubeWireframe[7, 1], true);
            graphics.DrawLine(cubeWireframe[7, 0], cubeWireframe[7, 1], cubeWireframe[4, 0], cubeWireframe[4, 1], true);

            graphics.DrawLine(cubeWireframe[0, 0], cubeWireframe[0, 1], cubeWireframe[4, 0], cubeWireframe[4, 1], true);
            graphics.DrawLine(cubeWireframe[1, 0], cubeWireframe[1, 1], cubeWireframe[5, 0], cubeWireframe[5, 1], true);
            graphics.DrawLine(cubeWireframe[2, 0], cubeWireframe[2, 1], cubeWireframe[6, 0], cubeWireframe[6, 1], true);
            graphics.DrawLine(cubeWireframe[3, 0], cubeWireframe[3, 1], cubeWireframe[7, 0], cubeWireframe[7, 1], true);
        }

        void ShowGrid()
        {
            int xOffset = 0;
            int yOffset = 0;
            int spacing = 15;

            for (int t = 0; t < 2000; t++)
            {
                display.Clear();

                for (int i = xOffset; i < display.Width; i += spacing)
                {
                    graphics.DrawVerticalLine(i, 0, (int)display.Height, true);
                }

                for (int j = yOffset; j < display.Height; j += spacing)
                {
                    graphics.DrawHorizontalLine(0, j, (int)display.Width, true);
                }

                xOffset = (xOffset + 1) % spacing;
                yOffset = (yOffset + 1) % spacing;

                display.Show();
            }
        }

        void TestDisplayGraphicsAPI()
        {
            graphics = new MicroGraphics(display);

            graphics.Clear();
            graphics.CurrentFont = new Font12x16();
            graphics.DrawText(0, 0, "MeadowB3.7");
            graphics.DrawText(0, 24, "4-8x faster");
            graphics.DrawText(0, 48, "86x IO perf");

            graphics.Show();
        }
    }
}
﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        MicroGraphics graphics;
        Max7219 display;

        //needs cleanup - quick port from c code
        double rot, rotationX, rotationY, rotationZ;
        double rotationXX, rotationYY, rotationZZ;
        double rotationXXX, rotationYYY, rotationZZZ;

        readonly int[,] cubeWireframe = new int[12, 3];
        int[,] cubeVertices;

        public override Task Initialize()
        {
            int cubeSize = 5;

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

            Resolver.Log.Info("Init...");

            var spiBus = Device.CreateSpiBus();

            display = new Max7219(spiBus, Device.Pins.D01, 4, 2, Max7219.Max7219Mode.Display);

            graphics = new MicroGraphics(display);

            graphics.Rotation = RotationType._90Degrees;

            Resolver.Log.Info("Max7219 instantiated");

            return base.Initialize();
        }

        void Show3dCube()
        {
            int originY = display.Width / 2;
            int originX = display.Height / 2;

            int angle = 0;
            while (true)
            {
                Resolver.Log.Info("Draw 3DCube frame");
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

        public override Task Run()
        {
            Show3dCube();

            graphics.Clear();
            graphics.DrawRectangle(0, 0, 16, 16);
            graphics.Show();

            Thread.Sleep(1000);

            graphics.Clear();
            graphics.DrawRectangle(0, 0, 20, 10);
            graphics.Show();

            /*   while (true)
               {
                   Counter();
                   Thread.Sleep(2000);

                   DrawPixels();
                   Thread.Sleep(2000);

                   ShowText();
                   Thread.Sleep(2000);
               } */

            return base.Run();
        }
    }
}
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System;
using System.Threading.Tasks;

namespace SharpMemoryDisplay_3dCube_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        // Unit cube vertices: 8 corners at ±1 on each axis
        static readonly float[] vx = { -1, 1, 1, -1, -1, 1, 1, -1 };
        static readonly float[] vy = { -1, -1, 1, 1, -1, -1, 1, 1 };
        static readonly float[] vz = { -1, -1, -1, -1, 1, 1, 1, 1 };

        // 12 edges as index pairs
        static readonly int[] edgeA = { 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3 };
        static readonly int[] edgeB = { 1, 2, 3, 0, 5, 6, 7, 4, 4, 5, 6, 7 };

        readonly int[] px = new int[8];
        readonly int[] py = new int[8];

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            var display = new SharpMemoryDisplay(
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D00,
                width: 144,
                height: 168);

            graphics = new MicroGraphics(display)
            {
                Stroke = 1,
                PenColor = Color.Black,
                CurrentFont = new Font4x8()
            };

            return base.Initialize();
        }

        public override Task Run()
        {
            int cx = graphics.Width / 2;
            int cy = graphics.Height / 2;
            const float scale = 165f;
            const float fov = 7f;
            float angleX = 0f;
            float angleY = 0f;

            int frameCount = 0;
            int lastTick = Environment.TickCount;
            string fpsText = "FPS:--";

            while (true)
            {
                float cosX = (float)Math.Cos(angleX);
                float sinX = (float)Math.Sin(angleX);
                float cosY = (float)Math.Cos(angleY);
                float sinY = (float)Math.Sin(angleY);

                for (int i = 0; i < 8; i++)
                {
                    float rx = vx[i] * cosY + vz[i] * sinY;
                    float rz = -vx[i] * sinY + vz[i] * cosY;
                    float ry = vy[i] * cosX - rz * sinX;
                    float rzf = vy[i] * sinX + rz * cosX;

                    float depth = fov + rzf;
                    px[i] = cx + (int)(rx * scale / depth);
                    py[i] = cy + (int)(ry * scale / depth);
                }

                graphics.Clear();

                for (int e = 0; e < 12; e++)
                {
                    graphics.DrawLine(px[edgeA[e]], py[edgeA[e]], px[edgeB[e]], py[edgeB[e]]);
                }

                graphics.DrawText(0, graphics.Height - 8, fpsText);

                graphics.Show();

                frameCount++;
                int now = Environment.TickCount;
                if (now - lastTick >= 1000)
                {
                    fpsText = $"FPS:{frameCount}";
                    frameCount = 0;
                    lastTick = now;
                }

                angleX += 0.04f;
                angleY += 0.07f;
            }
        }

        //<!=SNOP=>
    }
}

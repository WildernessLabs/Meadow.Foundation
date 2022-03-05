/// <summary>
/// Demos for Neopixels on Adafruit Seesaw
/// Author: Frederick M Meyer
/// Date: /// 2022-03-03
///
/// Copyright: 2022 (c) Frederick M Meyer for Wilderness Labs
/// License: MIT
/// </summary>
/// <remarks>
/// For hardware, this works with either Seesaw device:
/// Adafruit ATSAMD09 Breakout with seesaw <see href="https://www.adafruit.com/product/3657"</see>
/// or
/// Adafruit ATtiny8x7 Breakout with seesaw - STEMMA QT / Qwiic <see href="https://www.adafruit.com/product/5233"</see>
/// </remarks>

using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.ICs.IOExpanders.Seesaw;
using Meadow.Hardware;
using System;
using System.Linq;
using System.Threading;

namespace Neopixel_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
         public MeadowApp()
         {
            int np = 16;   // Number of Pixels

            Seesaw seesaw = new Seesaw(Device.CreateI2cBus(I2cBusSpeed.FastPlus));  // Define I2C bus

            Neopixel neopixel = new Neopixel(seesaw, 9, np, brightness: 0.3);  // Initialize seesaw-attached neopixel environment
            
            Neopixel.PixelArray px = neopixel.PixelArrayInstance;  // Get address of the pixel array created by Neopixel() constructor

            Console.WriteLine("Neopixel Sample");

            Console.WriteLine($"Seesaw Address: 0x{seesaw.SeesawBoardAddr:X}, Board Id: 0x{seesaw.ChipId:X}, Type: {Enum.GetName(typeof(HwidCodes), seesaw.ChipId)}");

            const uint red   = 0x40000000;
            const uint green = 0x00400000;
            const uint blue  = 0x00004000;
            const uint white = 0x00000060;

            int demoNumber = -1;

            while (true)
            {
                demoNumber = demoNumber < 7 ? ++demoNumber : 0;
                Console.WriteLine($"Starting Demo: {demoNumber}");

                switch (demoNumber)
                {
                    case 0:
                        //Red White Blue White using ValueTuple
                        for (int i = 0; i < px.Length; i++)
                            px[i] = i % 2 == 1 ? (0, 0, 0, 64) : i % 4 == 0 ? (0x40, 0, 0, 0) : (0, 0, 64, 0);

                        neopixel.MoveToDisplay();
                        neopixel.Show();
                        Thread.Sleep(15 * 1000);
                        break;

                    case 1:
                        px.Fill(0);

                        px[0] = 0xFF000000;
                        px[2] = 0x00FF0000;
                        px[4] = 0x0000FF00;
                        px[6] = 0x000000FF;

                        neopixel.MoveToDisplay();
                        neopixel.Show();

                        Thread.Sleep(5000);

                        px[1] = 0xFF000000;
                        px[3] = 0x00FF0000;
                        px[5] = 0x0000FF00;
                        px[7] = 0x000000FF;

                        neopixel.MoveToDisplay();
                        neopixel.Show();

                        if (np == 16)
                        {
                            Thread.Sleep(5000);

                            px[8] = px[0];
                            px[9] = px[1];
                            px[10] = px[2];
                            px[11] = px[3];
                            px[12] = px[4];
                            px[13] = px[5];
                            px[14] = px[6];
                            px[15] = px[7];

                            neopixel.MoveToDisplay();
                            neopixel.Show();
                        }

                            Thread.Sleep(15 * 1000);
                        break;

                    case 2:
                        //Red White Blue White using uiunt
                        for (int i = 0; i < px.Length; i++)
                            px[i] = i % 2 == 1 ? white : i % 4 == 0 ? red : blue;

                        neopixel.MoveToDisplay();
                        neopixel.Show();
                        Thread.Sleep(15 * 1000);
                        break;

                    case 3:
                        px[0] = (uint)0x40000000; // Red
                        px[1] = (uint)0x50200000; // Orange
                        px[2] = (uint)0x40400000; // Yellow
                        px[3] = (uint)0x00400000; // Green
                        px[4] = (uint)0x00004000; // Blue
                        px[5] = (uint)0x24004100; // Indigo
                        px[6] = (uint)0x4F2B4F00; // Violet
                        px[7] = (uint)0x00000040; // White

                        if (np == 16)
                        {
                            px[8] = (uint)0x40000000; // Red
                            px[9] = (uint)0x55320000; // Orange
                            px[10] = (uint)0x40400000; // Yellow
                            px[11] = (uint)0x00400000; // Green
                            px[12] = (uint)0x00004000; // Blue
                            px[13] = (uint)0x24004100; // Indigo
                            px[14] = (uint)0x27152700; // Violet
                            px[15] = (uint)0x00000020; // White
                        }

                        double holdBrightness = neopixel.Brightness;
                        neopixel.Brightness = 0.8;
                        
                        neopixel.MoveToDisplay();
                        neopixel.Show();
                        Thread.Sleep(5000);

                        neopixel.Brightness = holdBrightness;

                        neopixel.MoveToDisplay();
                        neopixel.Show();
                        Thread.Sleep(15 * 1000);
                        break;

                    case 4:
                        // px.fill(green | blue, Enumerable.Range(0, np).Where(p => p % 2 != 0));
                        // px.fill(white,        Enumerable.Range(0, np).Where(p => p % 2 == 0));
                        // -or-
                        px.Fill(white, Enumerable.Range(0, np).Where(p => p % 2 == 0));
                        px.Fill(red, Enumerable.Range(0, np).Where(p => p % 4 == 1));
                        px.Fill(blue, Enumerable.Range(0, np).Where(p => p % 4 == 3));

                        neopixel.MoveToDisplay();
                        neopixel.Show();
                        Thread.Sleep(15 * 1000);
                        break;

                    case 5:
                    case 6:
                        // =====================================
                        // Use colorwheel for demo
                        //
                        int coloroffset = 0;  // Start at red
                        for (int j = 0; j < 100; j++)
                        {
                            // Cycle through all colors along the ring
                            for (int i = 0; i < np; i++)
                            {
                                int rcindex = (i * 256 / np) + coloroffset;

                                px[i] = colorwheel(rcindex % 256);

                                coloroffset++;
                            }

                            neopixel.ReversePixelOrder = demoNumber == 5;  // <== Reverses the pixel positions when moving to display (non-destructive);
                                                                           //     01234567 becomes 76543210 
                            neopixel.MoveToDisplay();
                            neopixel.Show();

                            Thread.Sleep(100);
                        }
                        neopixel.ReversePixelOrder = false;
                        break;
                    //
                    // =====================================

                    case 7:
                        px.Fill((0, 255, 255, 0));

                        neopixel.MoveToDisplay();
                        neopixel.Show();
                        Thread.Sleep(15 * 1000);
                        break;
                }
            }
        }

        uint colorwheel(int colorvalue)
        {
            // A colorwheel. ``0`` and ``255`` are red, ``85`` is green, and ``170`` is blue, with the values
            // between being the rest of the rainbow.

            // param int colorvalue: 0-255 of color value to return
            // return: uint of RGB values

            byte r; byte g; byte b;

            if (colorvalue < 0 || colorvalue > 255)
            {
                r = 0;
                g = 0;
                b = 0;
            }
            else if (colorvalue < 85)
            {
                r = (byte)(255 - colorvalue / 3);
                g = (byte)(colorvalue * 3);
                b = 0;
            }
            else if (colorvalue < 170)
            {
                colorvalue -= 85;
                r = 0;
                g = (byte)(255 - colorvalue * 3);
                b = (byte)(colorvalue * 3);
            }
            else
            {
                colorvalue -= 170;
                r = (byte)(colorvalue * 3);
                g = 0;
                b = (byte)(255 - colorvalue * 3);
            }
            return BitConverter.ToUInt32(new byte[] {0, (byte)(b), (byte)(g), (byte)(r)});
        }
    }
}

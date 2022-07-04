using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;

namespace ICs.IOExpanders.Is31fl3731_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Is31fl3731 iS31FL3731;
        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            iS31FL3731 = new Is31fl3731(Device.CreateI2cBus());
            iS31FL3731.Initialize();

            iS31FL3731.ClearAllFrames();
            iS31FL3731.SetFrame(frame: 0);
            iS31FL3731.DisplayFrame(frame: 0);

            //Turn on all LEDs in frame
            for (byte i = 0; i <= 144; i++)
            {
                iS31FL3731.SetLedPwm(
                    frame: 0, 
                    ledIndex: i, 
                    brightness: 128);

                Thread.Sleep(50);
            }
        }

        //<!=SNOP=>

        void IS31FL3731Tests()
        { 
            DarkToBright();
            Thread.Sleep(1000);

            iS31FL3731.Clear();
            RunExplicit();
            Thread.Sleep(1000);

            iS31FL3731.Clear();
            RunImplicit();
            Thread.Sleep(5000);

            iS31FL3731.Clear();
        }


        void DarkToBright()
        {
            byte brightness = 0;
            byte led = 0;

            for (byte y = 0; y < 144; y++)
            {
                if (brightness >= 255)
                {
                    brightness = 0;
                }

                iS31FL3731.SetLedPwm(led, brightness);
                led++;
                brightness += 2;
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Set the LEDs explicitly for each function call. 
        /// </summary>
        void RunExplicit()
        {
            Console.WriteLine("Run...");
            Console.WriteLine("Display frame 0");
            iS31FL3731.DisplayFrame(0);
            iS31FL3731.SetLedPwm(0, 1, 128);

            iS31FL3731.SetLedPwm(1, 2, 128);

            iS31FL3731.SetLedPwm(2, 3, 128);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 1");
            iS31FL3731.DisplayFrame(1);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 2");
            iS31FL3731.DisplayFrame(2);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 0");
            iS31FL3731.DisplayFrame(0);

            //Turn on all LEDs
            for(byte i = 0; i <= 144; i++)
            {
                iS31FL3731.SetLedPwm(0, i, 128);
                iS31FL3731.SetLedPwm(1, i, 70);
                Thread.Sleep(50);
            }

            Console.WriteLine("Frame switching blinking");
            //Switch between Frame 0 and 1. Blinking them
            for (byte i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                iS31FL3731.DisplayFrame(1);

                Thread.Sleep(1000);
                iS31FL3731.DisplayFrame(0);
            }

            //Switch between Frame 0 and 1. Blinking them
            
            Thread.Sleep(1000);
            Console.WriteLine("Frame 0 blink mode on");
            iS31FL3731.SetBlinkMode(true, 0x05);
            iS31FL3731.SetBlinkFunctionOnAllLeds(0, true);

            Thread.Sleep(10000);
            Console.WriteLine("Turn off blink mode");
            iS31FL3731.SetBlinkFunctionOnAllLeds(0, false);
            iS31FL3731.SetBlinkMode(false, 0x05);

            iS31FL3731.Clear(0);
        }

        /// <summary>
        /// Set the LEDs implicitly for each function call.
        /// </summary>
        void RunImplicit()
        {
            Console.WriteLine("Run implicit...");
            Console.WriteLine("Display frame 0");
            iS31FL3731.DisplayFrame(0);
            iS31FL3731.SetFrame(0);
            iS31FL3731.SetLedPwm(1, 128);

            iS31FL3731.SetFrame(1);
            iS31FL3731.SetLedPwm(1, 2, 128);

            iS31FL3731.SetFrame(2);
            iS31FL3731.SetLedPwm(2, 3, 128);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 1");
            iS31FL3731.DisplayFrame(1);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 2");
            iS31FL3731.DisplayFrame(2);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 0");
            iS31FL3731.DisplayFrame(0);

            //Turn on All the LED
            for (byte i = 0; i <= 144; i++)
            {
                iS31FL3731.SetFrame(0);
                iS31FL3731.SetLedPwm(i, 128);

                iS31FL3731.SetFrame(1);
                iS31FL3731.SetLedPwm(1, i, 70);
                Thread.Sleep(50);
            }

            Console.WriteLine("Frame switching blinking");
            //Switch between Frame 0 and 1. Blinking them
            for (byte i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                iS31FL3731.DisplayFrame(1);

                Thread.Sleep(1000);
                iS31FL3731.DisplayFrame(0);
            }

            //Switch between Frame 0 and 1. Blinking them

            Thread.Sleep(1000);
            iS31FL3731.SetFrame(0);
            Console.WriteLine("Frame 0 blink mode on");
            iS31FL3731.SetBlinkMode(true, 0x05);
            iS31FL3731.SetBlinkFunctionOnAllLeds(true);

            Thread.Sleep(10000);
            Console.WriteLine("Turn off blink mode");
            iS31FL3731.SetBlinkFunctionOnAllLeds(false);
            iS31FL3731.SetBlinkMode(false, 0x05);
            iS31FL3731.Clear();
        }
    }
}

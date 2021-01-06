using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;

namespace ICs.IOExpanders.IS31FL3731_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        II2cBus _i2cBus;
        Is31fl3731 _iS31FL3731;
        public MeadowApp()
        {
            Initialize();
            Thread.Sleep(1000);

            Brightness();
            Thread.Sleep(1000);

            _iS31FL3731.Clear();
            RunExplicit();
            Thread.Sleep(1000);

            _iS31FL3731.Clear();
            RunImplicit();
            Thread.Sleep(5000);

            _iS31FL3731.Clear();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            
            _i2cBus= Device.CreateI2cBus();
            _iS31FL3731 = new Is31fl3731(_i2cBus, 0x74);
            
            _iS31FL3731.Initialize();

            Console.WriteLine("Clear all frames...");
            for (byte i = 0; i <= 7; i++)
            {
                _iS31FL3731.SetLedState(i,true);
                _iS31FL3731.Clear(i);
            }

            _iS31FL3731.SetFrame(0);
            _iS31FL3731.DisplayFrame(0);
        }

        void Brightness()
        {
            byte brightness = 0;
            byte led = 0;

            for (byte y = 0; y < 144; y++)
            {
                if (brightness >= 255)
                    brightness = 0;

                _iS31FL3731.SetLedPwm(led, brightness);
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
            _iS31FL3731.DisplayFrame(0);
            _iS31FL3731.SetLedPwm(0, 1, 128);

            _iS31FL3731.SetLedPwm(1, 2, 128);

            _iS31FL3731.SetLedPwm(2, 3, 128);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 1");
            _iS31FL3731.DisplayFrame(1);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 2");
            _iS31FL3731.DisplayFrame(2);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 0");
            _iS31FL3731.DisplayFrame(0);

            //Turn on All the LED
            for(byte i = 0; i <= 144; i++)
            {
                _iS31FL3731.SetLedPwm(0, i, 128);
                _iS31FL3731.SetLedPwm(1, i, 70);
                Thread.Sleep(50);
            }

            Console.WriteLine("Frame switching blinking");
            //Switch between Frame 0 and 1. Blinking them
            for (byte i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                _iS31FL3731.DisplayFrame(1);

                Thread.Sleep(1000);
                _iS31FL3731.DisplayFrame(0);
            }

            //Switch between Frame 0 and 1. Blinking them
            
            Thread.Sleep(1000);
            Console.WriteLine("Frame 0 blink mode on");
            _iS31FL3731.SetBlinkMode(true, 0x05);
            _iS31FL3731.SetBlinkFunctionOnAllLeds(0, true);

            Thread.Sleep(10000);
            Console.WriteLine("Turn off blink mode");
            _iS31FL3731.SetBlinkFunctionOnAllLeds(0, false);
            _iS31FL3731.SetBlinkMode(false, 0x05);

            _iS31FL3731.Clear(0);
        }

        /// <summary>
        /// Set the LEDs implicitly for each function call.
        /// </summary>
        void RunImplicit()
        {
            Console.WriteLine("Run implicit...");
            Console.WriteLine("Display frame 0");
            _iS31FL3731.DisplayFrame(0);
            _iS31FL3731.SetFrame(0);
            _iS31FL3731.SetLedPwm(1, 128);

            _iS31FL3731.SetFrame(1);
            _iS31FL3731.SetLedPwm(1, 2, 128);

            _iS31FL3731.SetFrame(2);
            _iS31FL3731.SetLedPwm(2, 3, 128);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 1");
            _iS31FL3731.DisplayFrame(1);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 2");
            _iS31FL3731.DisplayFrame(2);

            Thread.Sleep(1000);
            Console.WriteLine("Display frame 0");
            _iS31FL3731.DisplayFrame(0);

            //Turn on All the LED
            for (byte i = 0; i <= 144; i++)
            {
                _iS31FL3731.SetFrame(0);
                _iS31FL3731.SetLedPwm(i, 128);

                _iS31FL3731.SetFrame(1);
                _iS31FL3731.SetLedPwm(1, i, 70);
                Thread.Sleep(50);
            }

            Console.WriteLine("Frame switching blinking");
            //Switch between Frame 0 and 1. Blinking them
            for (byte i = 0; i < 10; i++)
            {
                Thread.Sleep(1000);
                _iS31FL3731.DisplayFrame(1);

                Thread.Sleep(1000);
                _iS31FL3731.DisplayFrame(0);
            }

            //Switch between Frame 0 and 1. Blinking them

            Thread.Sleep(1000);
            _iS31FL3731.SetFrame(0);
            Console.WriteLine("Frame 0 blink mode on");
            _iS31FL3731.SetBlinkMode(true, 0x05);
            _iS31FL3731.SetBlinkFunctionOnAllLeds(true);

            Thread.Sleep(10000);
            Console.WriteLine("Turn off blink mode");
            _iS31FL3731.SetBlinkFunctionOnAllLeds(false);
            _iS31FL3731.SetBlinkMode(false, 0x05);
            _iS31FL3731.Clear();
        }
    }
}

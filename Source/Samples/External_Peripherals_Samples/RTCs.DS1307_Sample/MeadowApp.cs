using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.RTCs;

namespace RTCs.DS1307_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected DS1307 rtc;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            rtc = new DS1307(Device.CreateI2cBus());

            DS1307Test();
        }

        protected void DS1307Test()
        {
            Console.WriteLine("DS1307Test...");

            var now = new DateTime();
            var running = rtc.IsRunning;

            Console.WriteLine($"{(running ? "is running" : "is not running")}");

            if (!running)
            {
                Console.WriteLine(" Starting RTC...");
                rtc.IsRunning = true;
            }

            while (true)
            {
                for (int i = 0; i < 3; i++)
                {
                    now = rtc.GetTime();
                    Console.WriteLine($" RTC current time is: {now.ToString("MM/dd/yy HH:mm:ss")}");
                    Thread.Sleep(1000);
                }

                var rand = new Random();

                if (now.Year < 2019)
                    now = DateTime.Now;
                else
                    now = now.AddSeconds(rand.Next(1, 30));

                Console.WriteLine($" Setting RTC to : {now.ToString("MM/dd/yy HH:mm:ss")}");

                var data = new byte[56];

                for (int i = 0; i < 56; i++)
                {
                    data[i] = (byte)rand.Next(256);
                }

                Console.WriteLine($" Writing to RTC RAM   : {BitConverter.ToString(data)}");
                rtc.WriteRAM(0, data);
                Console.Write($" Reading from RTC RAM : ");
                data = rtc.ReadRAM(0, 56);
                Console.WriteLine(BitConverter.ToString(data));

                Thread.Sleep(rand.Next(1, 5));
            }
        }
    }
}
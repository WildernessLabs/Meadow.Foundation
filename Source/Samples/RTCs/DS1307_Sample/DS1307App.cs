using Meadow;
using Meadow.Devices;
using Meadow.Foundation.RTCs;
using Meadow.Hardware;
using System;
using System.Threading;

namespace DS1307_Sample
{
    public class DS1307App : App<F7Micro, DS1307App>
    {
        public DS1307App()
        {
            Console.WriteLine("+DS1307App");

            var i2c = Device.CreateI2cBus();

            DS1307Test(i2c);
        }

        private void DS1307Test(II2cBus i2c)
        {
            Console.WriteLine("+DS1307 Test");

            var rtc = new DS1307(i2c);

            Console.Write(" Checking IsRunning...");
            var running = rtc.IsRunning;
            Console.WriteLine($"{(running ? "is running" : "is not running")}");

            if (!running)
            {
                Console.WriteLine(" Starting RTC...");
                rtc.IsRunning = true;
            }

            DateTime now = new DateTime();

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
                {
                    now = DateTime.Now;
                }
                else
                {
                    now = now.AddSeconds(rand.Next(1, 30));
                }

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

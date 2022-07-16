using Meadow;
using Meadow.Devices;
using Meadow.Foundation.RTCs;
using System;
using System.Threading.Tasks;

namespace RTCs.DS1307_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ds1307 rtc;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing...");

            rtc = new Ds1307(Device.CreateI2cBus());

            return base.Initialize();
        }

        public override Task Run()
        {
            var dateTime = new DateTime();
            var running = rtc.IsRunning;

            Console.WriteLine($"{(running ? "is running" : "is not running")}");

            if (!running)
            {
                Console.WriteLine(" Starting RTC...");
                rtc.IsRunning = true;
            }

            dateTime = rtc.GetTime();
            Console.WriteLine($" RTC current time is: {dateTime.ToString("MM/dd/yy HH:mm:ss")}");

            Console.WriteLine($" Setting RTC to : {dateTime.ToString("MM/dd/yy HH:mm:ss")}");
            dateTime = new DateTime(2030, 2, 15);
            rtc.SetTime(dateTime);

            dateTime = rtc.GetTime();
            Console.WriteLine($" RTC current time is: {dateTime.ToString("MM/dd/yy HH:mm:ss")}");

            var rand = new Random();

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

            return base.Run();
        }

        //<!=SNOP=>
    }
}
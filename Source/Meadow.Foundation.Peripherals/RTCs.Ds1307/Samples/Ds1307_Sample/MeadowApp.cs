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

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initializing...");

            rtc = new Ds1307(Device.CreateI2cBus());

            return base.Initialize(args);
        }

        public override Task Run()
        {
            var dateTime = new DateTime();
            var running = rtc.IsRunning;

            Resolver.Log.Info($"{(running ? "is running" : "is not running")}");

            if (!running)
            {
                Resolver.Log.Info(" Starting RTC...");
                rtc.IsRunning = true;
            }

            dateTime = rtc.GetTime();
            Resolver.Log.Info($" RTC current time is: {dateTime.ToString("MM/dd/yy HH:mm:ss")}");

            Resolver.Log.Info($" Setting RTC to : {dateTime.ToString("MM/dd/yy HH:mm:ss")}");
            dateTime = new DateTime(2030, 2, 15);
            rtc.SetTime(dateTime);

            dateTime = rtc.GetTime();
            Resolver.Log.Info($" RTC current time is: {dateTime.ToString("MM/dd/yy HH:mm:ss")}");

            var rand = new Random();

            var data = new byte[56];

            for (int i = 0; i < 56; i++)
            {
                data[i] = (byte)rand.Next(256);
            }

            Resolver.Log.Info($" Writing to RTC RAM   : {BitConverter.ToString(data)}");
            rtc.WriteRAM(0, data);
            Resolver.Log.Info($" Reading from RTC RAM : ");
            data = rtc.ReadRAM(0, 56);
            Resolver.Log.Info(BitConverter.ToString(data));

            return base.Run();
        }

        //<!=SNOP=>
    }
}
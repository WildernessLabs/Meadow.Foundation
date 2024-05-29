using Meadow;
using Meadow.Devices;
using Meadow.Foundation.RTCs;
using System;
using System.Threading.Tasks;

namespace RTCs.Pcf8523_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Pcf8523 rtc;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            rtc = new Pcf8523(Device.CreateI2cBus());

            return base.Initialize();
        }

        public override Task Run()
        {
            var dateTime = new DateTimeOffset();
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

            return base.Run();
        }

        //<!=SNOP=>
    }
}
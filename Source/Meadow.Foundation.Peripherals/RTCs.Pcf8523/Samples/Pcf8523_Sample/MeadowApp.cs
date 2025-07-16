using Meadow;
using Meadow.Devices;
using Meadow.Foundation.RTCs;
using System;
using System.Threading.Tasks;

namespace Pcf8523_Sample
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

        public override async Task Run()
        {
            DateTimeOffset dateTime;
            var running = rtc.IsRunning;

            Resolver.Log.Info($"{(running ? "is running" : "is not running")}");

            if (!running)
            {
                Resolver.Log.Info(" Starting RTC...");
                rtc.IsRunning = true;
            }

            dateTime = rtc.GetTime();
            Resolver.Log.Info($" RTC current time is: {dateTime:MM/dd/yy HH:mm:ss}");

            dateTime = new DateTime(2030, 2, 15);
            Resolver.Log.Info($" Setting RTC to : {dateTime:MM/dd/yy HH:mm:ss}");
            rtc.SetTime(dateTime);

            dateTime = rtc.GetTime();
            Resolver.Log.Info($" RTC current time is: {dateTime:MM/dd/yy HH:mm:ss}");

            // Test Timer A
            Resolver.Log.Info("Setting Timer A for 5 seconds...");
            rtc.SetTimerA(5, DelayTimeUnit.Seconds);

            // Test Timer B
            Resolver.Log.Info("Setting Timer B for 2 seconds...");
            rtc.SetTimerB(2, DelayTimeUnit.Seconds);

            await Task.Delay(2000);

            if (rtc.HasTimerAInterruptTriggered)
            {
                Resolver.Log.Info("Timer A SUCCESS");
            }
            else
            {
                Resolver.Log.Info("Timer A FAILED");
            }

            if (rtc.HasTimerBInterruptTriggered)
            {
                Resolver.Log.Info("Timer B SUCCESS");
            }
            else
            {
                Resolver.Log.Info("Timer B FAILED");
            }
        }

        //<!=SNOP=>
    }
}
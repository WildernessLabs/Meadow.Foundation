using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using System;
using System.Threading.Tasks;

namespace DeepSleep_Sample;

public class MeadowApp : App<F7CoreComputeV2>
{
    //<!=SNIP=>
    private DeepSleep deepSleep;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        var i2cBus = Device.CreateI2cBus();

        deepSleep = new DeepSleep(i2cBus);

        return Task.CompletedTask;
    }

    public override Task Run()
    {
        Resolver.Log.Info("Run...");

        var currentTime = deepSleep.GetTime();
        Resolver.Log.Info($"DeepSleep current time: {currentTime:MM/dd/yy HH:mm:ss}");

        var testTime = new DateTime(2025, 6, 15, 14, 30, 0);
        Resolver.Log.Info($"Setting time to: {testTime:MM/dd/yy HH:mm:ss}");
        deepSleep.SetTime(testTime);

        currentTime = deepSleep.GetTime();
        Resolver.Log.Info($"RTC time after setting: {currentTime:MM/dd/yy HH:mm:ss}");

        DateTimeOffset wakeTime = deepSleep.GetTime().AddSeconds(30);

        Resolver.Log.Info($"Setting DeepSleep to sleep in 10 seconds and wake at: {wakeTime:MM/dd/yy HH:mm:ss}");
        deepSleep.SetDeepSleep(10, wakeTime);

        return Task.CompletedTask;
    }

    //<!=SNOP=>
}
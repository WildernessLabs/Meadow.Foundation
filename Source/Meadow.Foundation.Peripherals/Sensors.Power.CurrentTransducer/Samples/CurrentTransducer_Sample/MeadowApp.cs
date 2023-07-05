using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Power;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace MeadowApp;

public class MeadowApp : App<F7FeatherV2>
{
    //<!=SNIP=>

    private CurrentTransducer transducer = default!;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        var bus = Device.CreateI2cBus();
        transducer = new CurrentTransducer(
            Device.Pins.A00.CreateAnalogInputPort(),
            new Voltage(3.3, Voltage.UnitType.Volts), // a reading of 3.3V
            new Current(10, Current.UnitType.Amps)    // equals 10 amps of current
            );

        Resolver.Log.Info($"-- Current Transducer Sample App ---");
        transducer.Updated += (s, v) =>
        {
            Resolver.Log.Info($"Current is now {v.New.Amps}A");
        };

        return Task.CompletedTask;
    }

    public override Task Run()
    {
        transducer.StartUpdating(TimeSpan.FromSeconds(2));
        return Task.CompletedTask;
    }

    //<!=SNOP=>
}
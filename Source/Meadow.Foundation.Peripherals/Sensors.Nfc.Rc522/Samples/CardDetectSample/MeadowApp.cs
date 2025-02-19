using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Radio.Rfid;
using Meadow.Units;
using System.Threading.Tasks;

namespace MeadowApp;

public class MeadowApp : App<F7FeatherV2>
{
    //<!=SNIP=>
    private Mfrc522 rfid;

    public override async Task Initialize()
    {
        Resolver.Log.LogLevel = Meadow.Logging.LogLevel.Trace;

        Resolver.Log.Info("Initialize...");

        var bus = Device.CreateSpiBus(500_000.Hertz());
        rfid = new Mfrc522(
            bus,
            Device.Pins.D03.CreateDigitalOutputPort(),
            Device.Pins.D04.CreateDigitalOutputPort());

        //        var version = rfid.();
        //        Resolver.Log.Info($"Version: 0x{version:X2}");
    }

    public override async Task Run()
    {
        Resolver.Log.Info("Run...");

        var result = rfid.SelfTest();

        Resolver.Log.Info($"self-test: {result}");

        while (true)
        {
            Resolver.Log.Info("check");

            var result2 = rfid.PICC_IsNewCardPresent();
            Resolver.Log.Info($"present: {result2}");

            await Task.Delay(1000);
        }
    }

    //<!=SNOP=>
}
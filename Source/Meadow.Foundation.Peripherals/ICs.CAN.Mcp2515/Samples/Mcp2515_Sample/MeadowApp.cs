using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.CAN;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace MeadowApp;

public class F7FeatherV1App : MeadowApp<F7FeatherV1> { }
public class F7FeatherV2App : MeadowApp<F7FeatherV2> { }

public class MeadowApp<T> : App<T>
    where T : F7FeatherBase
{
    private Mcp2515 expander;

    //<!=SNIP=>

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        expander = new Mcp2515(
            Device.CreateSpiBus(),
            Device.Pins.D05,
            Mcp2515.CanOscillator.Osc_8MHz,
            Device.Pins.D06);


        return base.Initialize();
    }

    public override async Task Run()
    {
        var bus = expander.CreateCanBus(CanBitrate.Can_250kbps);

        Console.WriteLine($"Listening for CAN data...");

        var tick = 0;

        while (true)
        {
            var frame = bus.ReadFrame();
            if (frame != null)
            {
                if (frame is StandardDataFrame sdf)
                {
                    Console.WriteLine($"Standard Frame: {sdf.ID:X3} {BitConverter.ToString(sdf.Payload)}");
                }
                else if (frame is ExtendedDataFrame edf)
                {
                    Console.WriteLine($"Extended Frame: {edf.ID:X8} {BitConverter.ToString(edf.Payload)}");
                }
            }
            else
            {
                await Task.Delay(100);
            }

            if (tick++ % 50 == 0)
            {
                Console.WriteLine($"Sending Standard Frame...");

                bus.WriteFrame(new StandardDataFrame
                {
                    ID = 0x700,
                    Payload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, (byte)(tick & 0xff) }
                });
            }
        }
    }

    //<!=SNOP=>
}
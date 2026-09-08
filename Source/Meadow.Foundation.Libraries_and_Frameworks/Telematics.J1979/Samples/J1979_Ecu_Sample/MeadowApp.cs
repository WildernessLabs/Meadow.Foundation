using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.CAN;
using Meadow.Foundation.Telematics.OBD2;
using System;
using System.Threading.Tasks;

namespace Obd2.EcuSample;

public class MeadowApp : App<F7FeatherV2>
{
    private Ecu ecu;
    private Mcp2515 mcp;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");
        mcp = new Mcp2515(
            Device.CreateSpiBus(),
            Device.Pins.D04.CreateDigitalOutputPort());
        var canBus = mcp.CreateCanBus(Meadow.Hardware.CanBitrate.Can_250kbps);
        ecu = new Ecu(canBus);

        // these PIDs are reported only on request
        ecu.PidRequestHandlers.Add(Pid.EngineCoolantTemperature, OnCoolantTempRequested);

        return Task.CompletedTask;
    }

    private byte[] OnCoolantTempRequested(ushort pid)
    {
        return new byte[] { 0x00, 0x01 };
    }

    private Obd2Frame GetFuelLevel()
    {
        throw new NotImplementedException();
    }

    public override async Task Run()
    {

        await Task.Delay(1000);

    }
}
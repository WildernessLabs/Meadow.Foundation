using Meadow;
using Meadow.Foundation;
using Meadow.Hardware;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DualCan_Sample;

public class MeadowApp : App<RaspberryPi>
{
    private WaveshareDualCanHat? _dualCanHat;
    private const int InterMessageDelay = 1000;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initializing...");

        _dualCanHat = new WaveshareDualCanHat(Device);

        return Task.CompletedTask;
    }

    public override async Task Run()
    {
        Resolver.Log.Info("Running...");

        _dualCanHat!.CAN0.FrameReceived += CAN0_FrameReceived;
        _dualCanHat!.CAN1.FrameReceived += CAN1_FrameReceived;
        _dualCanHat!.CAN0.BusError += OnBusError;
        _dualCanHat!.CAN1.BusError += OnBusError;

        var seedFrame = new StandardDataFrame
        {
            ID = 0x100,
            Payload = Encoding.UTF8.GetBytes($"f:0")
        };

        Resolver.Log.Info("Sending seed frame on CAN0");
        _dualCanHat.CAN0.WriteFrame(seedFrame);

        Resolver.Log.Info("Starting loopback test");

        while (true)
        {
            await (Task.Delay(1000));
        }
    }

    private void OnBusError(object? sender, CanErrorInfo e)
    {
        Resolver.Log.Info($"CAN error count: {e.ReceiveErrorCount}");
    }

    private void CAN0_FrameReceived(object? sender, ICanFrame e)
    {
        Resolver.Log.Info("CAN0 frame received");

        if (e is StandardDataFrame rx)
        {
            var payload = Encoding.UTF8.GetString(rx.Payload);
            Resolver.Log.Info($"CAN0 Received from {rx.ID}: {payload}");
            var count = int.Parse(payload.Substring(2));
            count++;

            // wait a bit to not flood the bus
            Thread.Sleep(InterMessageDelay);
            Resolver.Log.Info($"CAN0 responsing with {count}");
            var response = new StandardDataFrame
            {
                ID = 0x100,
                Payload = Encoding.UTF8.GetBytes($"f:{count}")
            };
            _dualCanHat!.CAN0.WriteFrame(response);
        }
    }

    private void CAN1_FrameReceived(object? sender, ICanFrame e)
    {
        Resolver.Log.Info("CAN1 frame received");

        if (e is StandardDataFrame rx)
        {
            var payload = Encoding.UTF8.GetString(rx.Payload);
            Resolver.Log.Info($"CAN1 Received from {rx.ID}: {payload}");
            var count = int.Parse(payload.Substring(2));
            count++;

            // wait a bit to not flood the bus
            Thread.Sleep(InterMessageDelay);
            Resolver.Log.Info($"CAN1 responsing with {count}");
            var response = new StandardDataFrame
            {
                ID = 0x101,
                Payload = Encoding.UTF8.GetBytes($"f:{count}")
            };
            _dualCanHat!.CAN1.WriteFrame(response);
        }
    }
}
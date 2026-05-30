using Meadow.Hardware;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Meadow.Foundation.Telematics.J1979;

internal class CanBusMonitor
{
    public event EventHandler<J1979QueryFrame>? QueryReceived;

    public ICanBus Bus { get; }

    // Physical request address for this module: moduleAddress - 8
    // e.g. PCM at 0x7E8 listens on 0x7E0; TCU at 0x7E9 listens on 0x7E1
    private readonly short _physicalRequestId;
    private readonly int _responseDelayMs;

    public CanBusMonitor(ICanBus bus, short moduleAddress)
    {
        Bus = bus;
        _physicalRequestId = (short)(moduleAddress - 8);
        // Stagger responses by address offset so lower-addressed modules always respond first,
        // matching real CAN bus arbitration behavior (lower ID wins). Without this, Task.Run
        // scheduling is non-deterministic and scan tools that assume ascending-address order break.
        _responseDelayMs = (moduleAddress - 0x7E8) * 5;
        bus.FrameReceived += OnFrameReceived;
    }

    private void OnFrameReceived(object? sender, ICanFrame frame)
    {
        if (frame is not StandardDataFrame sdf) return;
        if (sdf.ID != J1979Frame.J1979RequestID && sdf.ID != _physicalRequestId) return;
        // Only single-frame ISO-TP packets are J1979 queries (upper nibble == 0).
        // Upper nibble 0x1=FirstFrame, 0x2=Consecutive, 0x3=FlowControl — all ignored.
        if (sdf.Payload.Length == 0 || (sdf.Payload[0] & 0xF0) != 0) return;

        J1979QueryFrame query;
        try
        {
            if (J1979Frame.FromCanFrame(sdf) is not J1979QueryFrame q) return;
            query = q;
        }
        catch
        {
            // not a valid J1979 frame - ignore
            return;
        }

        // Dispatch off the receive thread to avoid re-entrancy issues with the CAN driver
        _ = Task.Run(async () =>
        {
            try
            {
                if (_responseDelayMs > 0)
                    await Task.Delay(_responseDelayMs);
                QueryReceived?.Invoke(this, query);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[J1979] Exception handling query: {ex}");
            }
        });
    }
}

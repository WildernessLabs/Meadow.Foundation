using Meadow.Foundation.Telematics.J1979;
using Meadow.Hardware;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Sends an ISO-TP response from an ECU address: a single frame when it fits, otherwise a first
/// frame, a wait for the tester's flow control, then the consecutive frames.
/// </summary>
/// <remarks>
/// Mirrors <c>ControllerBase.SendIsoTpResponse</c> in the J1979 driver, which is protected and so
/// not reachable from here.
/// </remarks>
public static class IsoTpResponder
{
    /// <summary>How long to wait for the tester's flow control frame before giving up.</summary>
    public static TimeSpan FlowControlTimeout { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Sends <paramref name="data"/> as an ISO-TP message from <paramref name="ecuAddress"/>.</summary>
    public static async Task SendAsync(ICanBus bus, short ecuAddress, short testerAddress, byte[] data)
    {
        var frames = IsoTp.Encode(data);
        if (frames.Length == 0) return;

        if (frames.Length == 1)
        {
            if (frames[0] is StandardDataFrame single)
            {
                single.ID = ecuAddress;
                bus.WriteFrame(single);
            }
            return;
        }

        if (frames[0] is not StandardDataFrame firstFrame) return;

        // The tester answers a first frame with flow control sent to its own request address.
        // Subscribe before transmitting — a tester that replies immediately would otherwise beat
        // the subscription and strand the message until the timeout.
        var tcs = new TaskCompletionSource<bool>();
        EventHandler<ICanFrame>? fcHandler = null;
        fcHandler = (_, frame) =>
        {
            if (frame is StandardDataFrame sdf &&
                sdf.ID == testerAddress &&
                sdf.Payload.Length > 0 &&
                (sdf.Payload[0] & 0xF0) == 0x30)
            {
                bus.FrameReceived -= fcHandler;
                tcs.TrySetResult(true);
            }
        };
        bus.FrameReceived += fcHandler;

        firstFrame.ID = ecuAddress;
        bus.WriteFrame(firstFrame);

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(FlowControlTimeout));
        if (completed != tcs.Task)
        {
            bus.FrameReceived -= fcHandler;
            Debug.WriteLine($"[UDS 0x{ecuAddress:X3}] ISO-TP: timeout waiting for flow control");
            return;
        }

        for (int i = 1; i < frames.Length; i++)
        {
            if (frames[i] is StandardDataFrame cf)
            {
                cf.ID = ecuAddress;
                bus.WriteFrame(cf);
                await Task.Delay(1);
            }
        }
    }
}

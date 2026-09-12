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
    public static Task SendAsync(ICanBus bus, short ecuAddress, short testerAddress, byte[] data)
        => SendAsync(bus, (uint)ecuAddress, (uint)testerAddress, data, extended: false);

    /// <summary>
    /// Sends an ISO-TP message at either addressing width. A module on a 29-bit normal-fixed
    /// address answers with 29-bit frames, so the encoder's standard frames have to be rebuilt as
    /// extended ones — assigning a 29-bit value to a <see cref="StandardDataFrame"/> would truncate
    /// it and the tester would never see the reply.
    /// </summary>
    public static async Task SendAsync(
        ICanBus bus, uint ecuAddress, uint testerAddress, byte[] data, bool extended)
    {
        var frames = IsoTp.Encode(data);
        if (frames.Length == 0) return;

        if (frames.Length == 1)
        {
            if (TryRetarget(frames[0], ecuAddress, extended) is { } single) bus.WriteFrame(single);
            return;
        }

        if (TryRetarget(frames[0], ecuAddress, extended) is not { } firstFrame) return;

        // The tester answers a first frame with flow control sent to its own request address.
        // Subscribe before transmitting — a tester that replies immediately would otherwise beat
        // the subscription and strand the message until the timeout.
        var tcs = new TaskCompletionSource<bool>();
        EventHandler<ICanFrame>? fcHandler = null;
        fcHandler = (_, frame) =>
        {
            if (!TryReadId(frame, out var id, out var payload)) return;

            if (id == testerAddress && payload.Length > 0 && (payload[0] & 0xF0) == 0x30)
            {
                bus.FrameReceived -= fcHandler;
                tcs.TrySetResult(true);
            }
        };
        bus.FrameReceived += fcHandler;

        bus.WriteFrame(firstFrame);

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(FlowControlTimeout));
        if (completed != tcs.Task)
        {
            bus.FrameReceived -= fcHandler;
            Debug.WriteLine($"[UDS 0x{ecuAddress:X}] ISO-TP: timeout waiting for flow control");
            return;
        }

        for (int i = 1; i < frames.Length; i++)
        {
            if (TryRetarget(frames[i], ecuAddress, extended) is { } cf)
            {
                bus.WriteFrame(cf);
                await Task.Delay(1);
            }
        }
    }

    /// <summary>Rewrites an encoded frame to the given identifier at the given width.</summary>
    private static ICanFrame? TryRetarget(ICanFrame frame, uint id, bool extended)
    {
        if (frame is not StandardDataFrame sdf) return null;

        if (!extended)
        {
            sdf.ID = (short)id;
            return sdf;
        }

        return new ExtendedDataFrame { ID = (int)id, Payload = sdf.Payload };
    }

    private static bool TryReadId(ICanFrame frame, out uint id, out byte[] payload)
    {
        switch (frame)
        {
            case ExtendedDataFrame edf:
                id = (uint)edf.ID;
                payload = edf.Payload ?? [];
                return true;
            case StandardDataFrame sdf:
                id = (uint)sdf.ID;
                payload = sdf.Payload ?? [];
                return true;
            default:
                id = 0;
                payload = [];
                return false;
        }
    }
}

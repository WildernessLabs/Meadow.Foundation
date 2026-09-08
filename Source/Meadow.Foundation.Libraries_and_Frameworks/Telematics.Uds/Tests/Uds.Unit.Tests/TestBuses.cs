using Meadow.Hardware;

namespace Uds.Unit.Tests;

/// <summary>Records what a module transmits and lets a test inject requests.</summary>
public class FakeCanBus : ICanBus
{
    public event EventHandler<ICanFrame>? FrameReceived;
    public event EventHandler<CanErrorInfo>? BusError;

    public CanAcceptanceFilterCollection AcceptanceFilters { get; } = new(0);
    public CanBitrate BitRate { get; set; } = CanBitrate.Can_500kbps;

    public List<StandardDataFrame> SentFrames { get; } = [];

    public void WriteFrame(ICanFrame frame)
    {
        if (frame is StandardDataFrame sdf)
        {
            lock (SentFrames) SentFrames.Add(sdf);
        }
    }

    public void InjectFrame(ICanFrame frame) => FrameReceived?.Invoke(this, frame);

    /// <summary>Sends an ISO-TP single frame request, the way a tester would.</summary>
    public void InjectRequest(short id, params byte[] data)
    {
        var payload = new byte[8];
        payload[0] = (byte)data.Length;
        Array.Copy(data, 0, payload, 1, data.Length);
        InjectFrame(new StandardDataFrame { ID = id, Payload = payload });
    }

    /// <summary>Waits for the module to transmit, or gives up. Returns what was sent.</summary>
    public async Task<List<StandardDataFrame>> WaitForFrames(int count, int timeoutMs = 1000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            lock (SentFrames)
            {
                if (SentFrames.Count >= count) return [.. SentFrames];
            }
            await Task.Delay(5);
        }

        lock (SentFrames) return [.. SentFrames];
    }

    /// <summary>Gives any pending background response time to land, then returns what was sent.</summary>
    public async Task<List<StandardDataFrame>> Settle(int delayMs = 250)
    {
        await Task.Delay(delayMs);
        lock (SentFrames) return [.. SentFrames];
    }

    public void ClearReceiveBuffers()
    {
        lock (SentFrames) SentFrames.Clear();
    }

    public bool IsFrameAvailable() => false;
    public ICanFrame? ReadFrame() => null;

    /// <summary>Raises <see cref="BusError"/>; present so the event is used.</summary>
    public void ReportError(CanErrorInfo info) => BusError?.Invoke(this, info);
}

/// <summary>
/// A bus that hands every transmitted frame straight back to all subscribers, so a client and a
/// server attached to it talk to each other exactly as they would over a wire.
/// </summary>
public class LoopbackCanBus : ICanBus
{
    public event EventHandler<ICanFrame>? FrameReceived;
    public event EventHandler<CanErrorInfo>? BusError;

    public CanAcceptanceFilterCollection AcceptanceFilters { get; } = new(0);
    public CanBitrate BitRate { get; set; } = CanBitrate.Can_500kbps;

    public List<StandardDataFrame> Traffic { get; } = [];

    public void WriteFrame(ICanFrame frame)
    {
        if (frame is StandardDataFrame sdf)
        {
            lock (Traffic) Traffic.Add(sdf);
        }

        FrameReceived?.Invoke(this, frame);
    }

    public void ClearReceiveBuffers()
    {
        lock (Traffic) Traffic.Clear();
    }

    public bool IsFrameAvailable() => false;
    public ICanFrame? ReadFrame() => null;

    /// <summary>Raises <see cref="BusError"/>; present so the event is used.</summary>
    public void ReportError(CanErrorInfo info) => BusError?.Invoke(this, info);
}

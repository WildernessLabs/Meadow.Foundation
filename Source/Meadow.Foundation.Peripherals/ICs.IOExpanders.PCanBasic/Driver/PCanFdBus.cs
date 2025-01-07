using Meadow.Hardware;

namespace ICs.IOExpanders.PCanBasic;

public class PCanFdBus : ICanBus
{
    /// <inheritdoc/>
    public event EventHandler<ICanFrame>? FrameReceived;
    /// <inheritdoc/>
    public event EventHandler<CanErrorInfo>? BusError;

    internal PCanFdBus(PCanConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public CanBitrate BitRate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public CanAcceptanceFilterCollection AcceptanceFilters => throw new NotImplementedException();

    public void ClearReceiveBuffers()
    {
        throw new NotImplementedException();
    }

    public bool IsFrameAvailable()
    {
        throw new NotImplementedException();
    }

    public ICanFrame? ReadFrame()
    {
        throw new NotImplementedException();
    }

    public void SetFilter(int filter)
    {
        throw new NotImplementedException();
    }

    public void SetMask(int filter)
    {
        throw new NotImplementedException();
    }

    public void WriteFrame(ICanFrame frame)
    {
        throw new NotImplementedException();
    }
}

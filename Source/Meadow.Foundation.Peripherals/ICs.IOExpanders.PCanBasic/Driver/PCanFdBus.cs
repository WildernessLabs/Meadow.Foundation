using Meadow.Hardware;

namespace ICs.IOExpanders.PCanBasic;

public class PCanFdBus : ICanBus
{
    internal PCanFdBus(PCanConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public event EventHandler<ICanFrame>? FrameReceived;

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

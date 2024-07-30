using Meadow.Hardware;
using Peak.Can.Basic.BackwardCompatibility;

namespace ICs.IOExpanders.PCanBasic;

/// <summary>
/// Represents a PCAN Basic 
/// </summary>
public class PCanBus : ICanBus
{
    /// <inheritdoc/>
    public event EventHandler<ICanFrame>? FrameReceived;

    private PCanConfiguration configuration;

    internal PCanBus(PCanConfiguration configuration)
    {
        var result = PCANBasic.Initialize(
            configuration.BusHandle,
            configuration.Bitrate.ToPCANBaudrate());

        if (result != TPCANStatus.PCAN_ERROR_OK)
        {
            throw new Exception($"{result}");
        }

        this.configuration = configuration;
    }

    /// <inheritdoc/>
    public bool IsFrameAvailable()
    {
        return false;
    }

    private void WriteStandard(StandardDataFrame frame)
    {
        var msgCanMessage = new TPCANMsg
        {
            DATA = new byte[frame.Payload.Length],
            ID = (uint)frame.ID,
            LEN = (byte)frame.Payload.Length,
            MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD
        };
        Array.Copy(frame.Payload, msgCanMessage.DATA, frame.Payload.Length);
        var result = PCANBasic.Write(configuration.BusHandle, ref msgCanMessage);
        if (result != TPCANStatus.PCAN_ERROR_OK)
        {
            throw new Exception($"{result}");
        }
    }

    private void WriteExtended(ExtendedDataFrame frame)
    {
        var msgCanMessage = new TPCANMsg
        {
            DATA = new byte[frame.Payload.Length],
            ID = (uint)frame.ID,
            LEN = (byte)frame.Payload.Length,
            MSGTYPE = TPCANMessageType.PCAN_MESSAGE_EXTENDED
        };
        Array.Copy(frame.Payload, msgCanMessage.DATA, frame.Payload.Length);
        var result = PCANBasic.Write(configuration.BusHandle, ref msgCanMessage);
        if (result != TPCANStatus.PCAN_ERROR_OK)
        {
            throw new Exception($"{result}");
        }
    }

    /// <inheritdoc/>
    public void WriteFrame(ICanFrame frame)
    {
        if (frame is ExtendedDataFrame edf)
        {
            WriteExtended(edf);
        }
        else if (frame is StandardDataFrame sdf)
        {
            WriteStandard(sdf);
        }
        else
        {
            throw new Exception($"Frame type {frame.GetType().Name} is not supported.");
        }
    }

    /// <inheritdoc/>
    public ICanFrame? ReadFrame()
    {
        var result = PCANBasic.Read(
            configuration.BusHandle,
            out TPCANMsg message,
            out TPCANTimestamp timeStamp);

        if (result != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
        {
            DataFrame frame;

            switch (message.MSGTYPE)
            {
                case TPCANMessageType.PCAN_MESSAGE_STANDARD:
                    frame = new StandardDataFrame
                    {
                        ID = (short)message.ID,
                        Payload = new byte[message.LEN]
                    };
                    Array.Copy(message.DATA, 0, frame.Payload, 0, message.LEN);
                    return frame;
                case TPCANMessageType.PCAN_MESSAGE_EXTENDED:
                    frame = new ExtendedDataFrame
                    {
                        ID = (int)message.ID,
                        Payload = new byte[message.LEN]
                    };
                    Array.Copy(message.DATA, 0, frame.Payload, 0, message.LEN);
                    return frame;
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public void SetFilter(int filter)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void SetMask(int filter)
    {
        throw new NotImplementedException();
    }
}

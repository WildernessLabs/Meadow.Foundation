using Meadow.Hardware;
using Peak.Can.Basic.BackwardCompatibility;

namespace ICs.IOExpanders.PCanBasic;

public class PCanBus : ICanBus
{
    private PCanConfiguration configuration;

    internal PCanBus(PCanConfiguration configuration)
    {
        if (configuration.IsFD)
        {
            throw new ArgumentException();
        }

        var result = PCANBasic.Initialize(
            configuration.BusHandle,
            configuration.Bitrate.ToPCANBaudrate());

        if (result != TPCANStatus.PCAN_ERROR_OK)
        {
            throw new Exception($"{result}");
        }

        this.configuration = configuration;
    }

    public void Write(short id, byte[] data)
    {
        Write(new StandardCanFrame(id, data));
    }

    public void WriteExtended(int id, byte[] data)
    {
        WriteExtended(new ExtendedCanFrame(id, data));
    }

    public void Write(StandardCanFrame frame)
    {
        var msgCanMessage = new TPCANMsg
        {
            DATA = new byte[frame.Data.Length],
            ID = (uint)frame.ID,
            LEN = (byte)frame.Data.Length,
            MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD
        };
        Array.Copy(frame.Data, msgCanMessage.DATA, frame.Data.Length);
        var result = PCANBasic.Write(configuration.BusHandle, ref msgCanMessage);
        if (result != TPCANStatus.PCAN_ERROR_OK)
        {
            throw new Exception($"{result}");
        }
    }

    public void WriteExtended(ExtendedCanFrame frame)
    {
        var msgCanMessage = new TPCANMsg
        {
            DATA = new byte[frame.Data.Length],
            ID = (uint)frame.ID,
            LEN = (byte)frame.Data.Length,
            MSGTYPE = TPCANMessageType.PCAN_MESSAGE_EXTENDED
        };
        Array.Copy(frame.Data, msgCanMessage.DATA, frame.Data.Length);
        var result = PCANBasic.Write(configuration.BusHandle, ref msgCanMessage);
        if (result != TPCANStatus.PCAN_ERROR_OK)
        {
            throw new Exception($"{result}");
        }
    }

    public CanFrame? Read()
    {
        var result = PCANBasic.Read(
            configuration.BusHandle,
            out TPCANMsg message,
            out TPCANTimestamp timeStamp);

        if (result != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
        {
            switch (message.MSGTYPE)
            {
                case TPCANMessageType.PCAN_MESSAGE_STANDARD:
                    return new StandardCanFrame(
                        (short)message.ID,
                        message.DATA,
                        0,
                        message.LEN);

                case TPCANMessageType.PCAN_MESSAGE_EXTENDED:
                    return new ExtendedCanFrame(
                        (int)message.ID,
                        message.DATA,
                        0,
                        message.LEN);
            }
        }
        return null;
    }
}

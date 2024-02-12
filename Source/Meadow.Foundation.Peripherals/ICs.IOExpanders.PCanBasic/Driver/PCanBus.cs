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
                case TPCANMessageType.PCAN_MESSAGE_EXTENDED:
                    var frame = new CanFrame(
                        message.ID,
                        message.DATA,
                        0,
                        message.LEN);

                    return frame;
            }
        }
        return null;
    }
}

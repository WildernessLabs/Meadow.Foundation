using Meadow.Hardware;
using Peak.Can.Basic.BackwardCompatibility;

namespace Meadow.Foundation.ICs.CAN;

/// <summary>
/// Represents a PCAN Basic 
/// </summary>
public class PCanBasic : ICanBus
{
    /// <inheritdoc/>
    public event EventHandler<ICanFrame>? FrameReceived;
    /// <inheritdoc/>
    public event EventHandler<CanErrorInfo>? BusError;

    /// <inheritdoc/>
    public CanAcceptanceFilterCollection AcceptanceFilters { get; } = new(5);

    private PCanConfiguration configuration;

    internal PCanBasic(PCanConfiguration configuration)
    {
        var result = PCANBasic.Initialize(
            configuration.BusHandle,
            configuration.Bitrate.ToPCANBaudrate());

        if (result != TPCANStatus.PCAN_ERROR_OK)
        {
            throw new Exception($"{result}");
        }

        this.configuration = configuration;
        AcceptanceFilters.CollectionChanged += OnAcceptanceFiltersChanged;
    }

    private void OnAcceptanceFiltersChanged(object? sender, (System.ComponentModel.CollectionChangeAction Action, CanAcceptanceFilter Filter) e)
    {
        if (e.Action == System.ComponentModel.CollectionChangeAction.Add)
        {
            if (e.Filter is CanStandardExactAcceptanceFilter sefa)
            {
                PCANBasic.FilterMessages(configuration.BusHandle, (uint)sefa.AcceptID, (uint)sefa.AcceptID, TPCANMode.PCAN_MODE_STANDARD);
            }
            else if (e.Filter is CanExtendedExactAcceptanceFilter eef)
            {
                PCANBasic.FilterMessages(configuration.BusHandle, (uint)eef.AcceptID, (uint)eef.AcceptID, TPCANMode.PCAN_MODE_EXTENDED);
            }
            else if (e.Filter is CanStandardRangeAcceptanceFilter srf)
            {
                PCANBasic.FilterMessages(configuration.BusHandle, (uint)srf.FirstAcceptID, (uint)srf.LastAcceptID, TPCANMode.PCAN_MODE_STANDARD);
            }
            else if (e.Filter is CanExtendedRangeAcceptanceFilter erf)
            {
                PCANBasic.FilterMessages(configuration.BusHandle, (uint)erf.FirstAcceptID, (uint)erf.LastAcceptID, TPCANMode.PCAN_MODE_EXTENDED);
            }
        }
    }

    /// <inheritdoc/>
    public CanBitrate BitRate
    {
        get => configuration.Bitrate;
        set
        {
            PCANBasic.Uninitialize(configuration.BusHandle);

            configuration.Bitrate = value;

            var result = PCANBasic.Initialize(
                configuration.BusHandle,
                configuration.Bitrate.ToPCANBaudrate());

            if (result != TPCANStatus.PCAN_ERROR_OK)
            {
                throw new Exception($"{result}");
            }
        }
    }

    /// <inheritdoc/>
    public void ClearReceiveBuffers()
    {
        PCANBasic.Reset(configuration.BusHandle);
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

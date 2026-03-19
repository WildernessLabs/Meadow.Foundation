using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Telematics.J1979;

public class SaeStandardQueryFrame : J1979QueryFrame
{
    public Pid Pid
    {
        get => (Pid)Payload[2];
    }

    private SaeStandardQueryFrame()
    {
        ID = J1979RequestID;
        Payload = new byte[8];
        Payload[0] = 2;
    }

    public SaeStandardQueryFrame(Service service, byte pid)
        : this()
    {
        Payload[1] = (byte)service;
        Payload[2] = pid;
    }

    internal SaeStandardQueryFrame(StandardDataFrame canFrame)
        : this()
    {
        if (canFrame.Payload[0] != 2)
        {
            throw new ArgumentException("CAN frame is not a valid SAE standard request frame");
        }
        this.ID = canFrame.ID;
        this.Payload = canFrame.Payload;
    }
}

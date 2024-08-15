using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Telematics.OBD2;

public class SaeStandardQueryFrame : Obd2QueryFrame
{
    public Pid Pid { get; }

    internal SaeStandardQueryFrame(StandardDataFrame canFrame)
    {
        if (canFrame.Payload[0] != 2)
        {
            throw new ArgumentException("CAN frame is not a valid SAE standard request frame");
        }

        Service = canFrame.Payload[1];
        Pid = (Pid)canFrame.Payload[2];
    }
}

using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Telematics.J1979;

public class VehicleSpecificQueryFrame : J1979QueryFrame
{
    public Pid Pid => (Pid)Payload[2];
    public byte FrameNumber => Payload[3];

    internal VehicleSpecificQueryFrame(StandardDataFrame canFrame)
    {
        if (canFrame.Payload[0] != 3)
        {
            throw new ArgumentException("CAN frame is not a valid vehicle-specific request frame");
        }

        ID = canFrame.ID;
        Payload = canFrame.Payload;
    }
}

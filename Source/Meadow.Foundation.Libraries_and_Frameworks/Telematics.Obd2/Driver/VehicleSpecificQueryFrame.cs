using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Telematics.OBD2;

public class VehicleSpecificQueryFrame : Obd2QueryFrame
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

using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Telematics.OBD2;

public class VehicleSpecificQueryFrame : Obd2QueryFrame
{
    public ushort Pid { get; }

    internal VehicleSpecificQueryFrame(StandardDataFrame canFrame)
    {
        if (canFrame.Payload[0] != 3)
        {
            throw new ArgumentException("CAN frame is not a valid vehicle-specific request frame");
        }

        Service = canFrame.Payload[1];
        Pid = (ushort)(canFrame.Payload[2] << 8 | canFrame.Payload[3]);
    }
}

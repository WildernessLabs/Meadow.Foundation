using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Telematics.OBD2;

public class ServiceOnlyQueryFrame : Obd2QueryFrame
{
    internal ServiceOnlyQueryFrame(StandardDataFrame canFrame)
    {
        if (canFrame.Payload[0] != 1)
            throw new ArgumentException("Not a service-only query frame");
        ID = canFrame.ID;
        Payload = canFrame.Payload;
    }
}

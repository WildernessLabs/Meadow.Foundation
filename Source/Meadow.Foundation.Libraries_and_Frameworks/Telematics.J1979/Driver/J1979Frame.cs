using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Telematics.J1979;

public abstract class J1979Frame : StandardDataFrame
{
    public const short J1979RequestID = 0x7df;

    public static J1979Frame FromCanFrame(StandardDataFrame dataFrame)
    {
        switch ((J1979FrameType)dataFrame.Payload[0])
        {
            case J1979FrameType.ServiceOnly: return new ServiceOnlyQueryFrame(dataFrame);
            case J1979FrameType.Standard: return new SaeStandardQueryFrame(dataFrame);
            case J1979FrameType.VehicleSpecific: return new VehicleSpecificQueryFrame(dataFrame);
        }

        throw new ArgumentException("data frame is not a valid J1979 frame");
    }

    public virtual StandardDataFrame AsCanFrame(J1979FrameType frameType, short ecuID)
    {

        throw new NotImplementedException();
    }
}

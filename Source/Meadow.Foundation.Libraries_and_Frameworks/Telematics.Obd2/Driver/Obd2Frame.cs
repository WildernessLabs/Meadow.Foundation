using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Telematics.OBD2;

public abstract class Obd2Frame : StandardDataFrame
{
    public const short Obd2RequestID = 0x7df;

    public static Obd2Frame FromCanFrame(StandardDataFrame dataFrame)
    {
        if (dataFrame.ID == Obd2RequestID)
        {
            switch ((Obd2FrameType)dataFrame.Payload[0])
            {
                case Obd2FrameType.Standard: // SAE STANDARD
                    return new SaeStandardQueryFrame(dataFrame);
                case Obd2FrameType.VehicleSpecific: // VEHICLE SPECIFIC
                    return new VehicleSpecificQueryFrame(dataFrame);
            }
        }

        throw new ArgumentException("data frame is not a valid ODB2 frame");
    }

    public virtual StandardDataFrame AsCanFrame(Obd2FrameType frameType, short ecuID)
    {

        throw new NotImplementedException();
    }
}

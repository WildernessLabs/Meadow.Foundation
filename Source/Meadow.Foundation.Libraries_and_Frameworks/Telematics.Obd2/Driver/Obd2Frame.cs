using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Telematics.OBD2;

public abstract class Obd2Frame
{
    public const short Obd2RequestID = 0x7df;

    public static Obd2Frame FromCanFrame(StandardDataFrame dataFrame)
    {
        if (dataFrame.ID == Obd2RequestID)
        {
            switch (dataFrame.Payload[0])
            {
                case 2: // SAE STANDARD
                    return new SaeStandardQueryFrame(dataFrame);
                case 3: // VEHICLE SPECIFIC
                    return new VehicleSpecificQueryFrame(dataFrame);
            }
        }

        throw new ArgumentException("data frame is not a valid ODB2 frame");
    }
}

using Meadow.Hardware;

namespace Meadow.Foundation.Telematics.OBD2;

public class Obd2ResponseFrame : Obd2Frame
{
    public Service Service => (Service)(Payload[1] - 0x40);
    public Pid Pid => (Pid)Payload[2];

    internal Obd2ResponseFrame(StandardDataFrame dataFrame)
    {
        ID = dataFrame.ID;
        Payload = dataFrame.Payload;
    }
}

public abstract class Obd2QueryFrame : Obd2Frame
{
    public Service Service => (Service)Payload[1];
}

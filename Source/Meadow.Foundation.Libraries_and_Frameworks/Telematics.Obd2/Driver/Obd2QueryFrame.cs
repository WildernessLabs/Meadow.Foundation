namespace Meadow.Foundation.Telematics.OBD2;

public abstract class Obd2QueryFrame : Obd2Frame
{
    public byte Service { get; protected set; }
}

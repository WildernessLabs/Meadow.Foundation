namespace Meadow.Foundation.Telematics.OBD2;

public abstract class Obd2QueryFrame : Obd2Frame
{
    public Service Service
    {
        get => (Service)Payload[1];
        set => Payload[1] = (byte)value;
    }
}

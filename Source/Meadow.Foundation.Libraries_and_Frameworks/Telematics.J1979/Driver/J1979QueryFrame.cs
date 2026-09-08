namespace Meadow.Foundation.Telematics.J1979;

public abstract class J1979QueryFrame : J1979Frame
{
    public Service Service
    {
        get => (Service)Payload[1];
        set => Payload[1] = (byte)value;
    }
}

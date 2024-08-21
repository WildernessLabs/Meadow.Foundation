using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Telematics.OBD2;

public class Obd2ResponseFrame : Obd2Frame
{
    public Service Service
    {
        get => (Service)(Payload[1] - 0x40);
        set => Payload[1] = (byte)((byte)value | 0x40);
    }

    public Pid Pid => (Pid)Payload[2];

    internal Obd2ResponseFrame(StandardDataFrame dataFrame)
    {
        ID = dataFrame.ID;
        Payload = dataFrame.Payload;
    }

    internal Obd2ResponseFrame(Service requestService, Pid pid, byte[] data, short ecuAddress)
    {
        ID = ecuAddress;
        Payload = new byte[8];

        if (data.Length > 4) throw new ArgumentOutOfRangeException();

        Payload[0] = (byte)(data.Length + 2);
        Payload[1] = (byte)((byte)requestService | 0x40);
        Payload[2] = (byte)pid;

        for (var i = 0; i < 5; i++)
        {
            if (data.Length - i > 0)
            {
                Payload[3 + i] = data[i];
            }
            else
            {
                Payload[3 + i] = 0x55; // empty bytes are stuffed with 0x55, not 0x00
            }
        }
    }
}

public abstract class Obd2QueryFrame : Obd2Frame
{
    public Service Service
    {
        get => (Service)Payload[1];
        set => Payload[1] = (byte)value;
    }
}

using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Telematics.OBD2;

public class Ecu
{
    private readonly ICanBus _bus;
    public PidRequestHandlerCollection PidRequestHandlers { get; } = new();
    public short Address { get; }

    public Ecu(ICanBus bus, short ecuAddress = 0x7e8)
    {
        _bus = bus;
        Address = ecuAddress;
        bus.FrameReceived += OnCanFrameReceived;
    }

    private void OnCanFrameReceived(object sender, ICanFrame e)
    {
        if (e is StandardDataFrame sdf)
        {
            var obdFrame = Obd2Frame.FromCanFrame(sdf);

            PidRequestHandler? handler = null;
            ushort pid = 0;
            byte service = 0;
            bool standard = true;

            if (obdFrame is SaeStandardQueryFrame sqf)
            {
                handler = PidRequestHandlers.GetHandler(sqf.Pid);
                pid = (ushort)sqf.Pid;
                service = sqf.Service;
            }
            else if (obdFrame is VehicleSpecificQueryFrame vqf)
            {
                handler = PidRequestHandlers.GetHandler(vqf.Pid);
                pid = vqf.Pid;
                service = vqf.Service;
                standard = false;
            }

            if (handler != null)
            {
                var data = handler.Invoke(pid);
                // data cannot be > 4 bytes long
                if (data.Length > 4)
                {
                    throw new Exception("Handler data exceeded 4 bytes in length");
                }

                var payload = new byte[8];
                payload[0] = (byte)(2 + data.Length);
                payload[1] = (byte)(service + 0x40);

                if (standard)
                {
                    payload[2] = (byte)pid;
                    data.CopyTo(payload, 3);
                }
                else
                {
                    payload[2] = (byte)(pid >> 8);
                    payload[3] = (byte)(pid | 0xff);
                    data.CopyTo(payload, 4);
                }

                // create a response frame
                var response = new StandardDataFrame
                {
                    ID = Address,
                    Payload = payload
                };

                _bus.WriteFrame(response);
            }
        }
    }
}

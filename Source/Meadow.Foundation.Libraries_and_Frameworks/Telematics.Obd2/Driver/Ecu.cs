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

    private void HandleSupportedPidsRequest(SaeStandardQueryFrame queryFrame)
    {
        Resolver.Log.Info($"handling {queryFrame.Pid} response frame");

        switch (queryFrame.Pid)
        {
            case Pid.SupportedPids_01_20:
            case Pid.SupportedPids_21_40:
                uint supportBits = 0;

                for (var i = 0; i < 32; i++)
                {
                    if (PidRequestHandlers.GetHandler((ushort)(i + (int)queryFrame.Pid + 1)) != null)
                    {
                        supportBits |= (uint)(i << (31 - i));
                    }
                }
                Resolver.Log.Info($"support: {supportBits:x8}");
                var payload = new byte[4];
                payload[0] = (byte)((supportBits & 0xff000000) >> 24);
                payload[1] = (byte)((supportBits & 0x00ff0000) >> 16);
                payload[2] = (byte)((supportBits & 0x0000ff00) >> 8);
                payload[3] = (byte)((supportBits & 0x000000ff) >> 0);

                var supportedFrame = new Obd2ResponseFrame(queryFrame.Service, queryFrame.Pid, payload, Address);

                Resolver.Log.Info($"Sending {queryFrame.Pid} response frame");

                _bus.WriteFrame(supportedFrame);

                break;
        }
    }

    private void HandleRequestCurrentData(SaeStandardQueryFrame queryFrame)
    {
        switch (queryFrame.Pid)
        {
            case Pid.SupportedPids_01_20:
            case Pid.SupportedPids_21_40:
                HandleSupportedPidsRequest(queryFrame);
                return;
        }

        var handler = PidRequestHandlers.GetHandler(queryFrame.Pid);
        var pid = (ushort)queryFrame.Pid;
        var service = queryFrame.Service;

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
            payload[2] = (byte)pid;
            data.CopyTo(payload, 3);

            // create a response frame
            var response = new Obd2ResponseFrame(queryFrame.Service, queryFrame.Pid, payload, Address);
            _bus.WriteFrame(response);
        }
    }

    private void OnCanFrameReceived(object sender, ICanFrame e)
    {
        if (e is StandardDataFrame sdf)
        {
            Obd2Frame? obdFrame = null;

            try
            {
                obdFrame = Obd2Frame.FromCanFrame(sdf);
            }
            catch (Exception ex)
            {
                Resolver.Log.Warn($"Unable to convert CAN frame to OBD2: {ex.Message}");
                return;
            }

            PidRequestHandler? handler = null;
            ushort pid = 0;
            Service service = 0;

            if (obdFrame is SaeStandardQueryFrame sqf)
            {
                switch (sqf.Service)
                {
                    case Service.Current:
                        HandleRequestCurrentData(sqf);
                        break;
                    case Service.FreezeFrame:
                        Resolver.Log.Info("Request for Freeze frame");
                        break;
                    case Service.StoredDtcs:
                        Resolver.Log.Info("Request for stored DTCs");
                        break;
                    case Service.PendingDtcs:
                        Resolver.Log.Info("Request for pending DTCs");
                        break;
                    case Service.ControlOperations:
                        Resolver.Log.Info("Request for control ops");
                        break;
                    case Service.PermanentDtcs:
                        Resolver.Log.Info("Request for permanent DTCs");
                        break;
                    case Service.TestResultsOther:
                        Resolver.Log.Info("Request for test results");
                        break;
                    case Service.VehicleInfo:
                        Resolver.Log.Info("Request for vehicle info");
                        break;
                }

                Resolver.Log.Warn($"Standard OBD2 Frame Received for Service:PID {sqf.Service}:{sqf.Pid}");
            }
            else if (obdFrame is VehicleSpecificQueryFrame vqf)
            {
                handler = PidRequestHandlers.GetHandler(vqf.Pid);
                pid = vqf.Pid;
                service = vqf.Service;
            }
            else
            {
                Resolver.Log.Warn($"OBD2 Frame Received for Service:PID {service}:{pid}");
            }
        }
    }
}

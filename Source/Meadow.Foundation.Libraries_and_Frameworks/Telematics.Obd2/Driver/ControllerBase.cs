using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Telematics.OBD2;

public interface IController
{
    Pid[] SupportedPids { get; }
    short ModuleAddress { get; }
}

public abstract class ControllerBase : IController
{
    public const short TesterAddress = 0x7E0;

    private readonly List<CanBusMonitor> _busMonitors = new();
    private byte[]? _supportedPidMask;
    private readonly Dictionary<Pid, Func<byte[]?>> _pidHandlers = new();
    private readonly IControlModuleStore _store;

    public abstract string Vin { get; }
    public virtual Pid[] SupportedPids => _pidHandlers.Keys.ToArray();
    public short ModuleAddress { get; }

    private byte[] SupportedPidMask
    {
        get
        {
            if (_supportedPidMask is null)
            {
                uint mask = 0;
                foreach (var pid in SupportedPids)
                {
                    byte p = (byte)pid;
                    if (p >= 0x01 && p <= 0x1F)
                        mask |= 1u << (32 - p);
                }
                // Advertise the 0x21-0x40 range extension if we have PIDs there
                if (SupportedPids.Any(p => (byte)p >= 0x21 && (byte)p <= 0x3F))
                    mask |= 1u; // bit 0 = PID 0x20 (SupportedPids_21_40) supported
                var maskBytes = BitConverter.GetBytes(mask);
                if (BitConverter.IsLittleEndian) Array.Reverse(maskBytes);
                _supportedPidMask = maskBytes;
            }
            return _supportedPidMask;
        }
    }

    protected ControllerBase(ICanBus[] canBuses, short moduleAddress, IControlModuleStore? store = null)
    {
        ModuleAddress = moduleAddress;
        _store = store ?? new InMemoryControlModuleStore();
        RegisterPids();
        foreach (var canBus in canBuses)
        {
            var monitor = new CanBusMonitor(canBus);
            monitor.QueryReceived += (_, query) => OnQueryReceived(monitor.Bus, query);
            _busMonitors.Add(monitor);
        }
    }

    protected void RegisterPid(Pid pid, Func<byte[]?> liveData)
    {
        _pidHandlers[pid] = liveData;
        _supportedPidMask = null;
    }

    protected virtual void RegisterPids()
    {
        RegisterPid(Pid.MonitorStatus,
            () => GetEmissionsReadiness().ToBytes(GetStoredDtcs().Count > 0, (byte)GetStoredDtcs().Count));

        RegisterPid(Pid.EngineCoolantTemperature,
            () => { var t = GetEngineCoolantTemperature(); return t.HasValue ? [(byte)(t.Value.Celsius + 40)] : null; });

        RegisterPid(Pid.EngineRpm,
            () => { var r = GetEngineRpm(); if (!r.HasValue) return null; var raw = (ushort)(r.Value * 4); return [(byte)(raw >> 8), (byte)(raw & 0xFF)]; });

        RegisterPid(Pid.VehicleSpeed,
            () => { var s = GetVehicleSpeed(); return s.HasValue ? [(byte)s.Value.KilometersPerHour] : null; });

        RegisterPid(Pid.ThrottlePosition,
            () => { var t = GetThrottlePosition(); return t.HasValue ? [(byte)(t.Value * 255f / 100f)] : null; });

        RegisterPid(Pid.EngineOilTemperature,
            () => { var t = GetTransFluidTemp(); return t.HasValue ? [(byte)(t.Value.Celsius + 40)] : null; });
    }

    private void OnQueryReceived(ICanBus sourceBus, Obd2QueryFrame queryFrame)
    {
        // TODO: raise an event
        Debug.WriteLine($"[PCM] Query received: Service=0x{(byte)queryFrame.Service:X2}");

        if (queryFrame is SaeStandardQueryFrame saeQuery)
        {
            HandleSaeQuery(sourceBus, saeQuery);
        }
        else if (queryFrame is ServiceOnlyQueryFrame serviceOnlyQuery)
        {
            HandleServiceOnlyQuery(sourceBus, serviceOnlyQuery);
        }
        else if (queryFrame is VehicleSpecificQueryFrame vehicleQuery)
        {
            HandleVehicleSpecificQuery(sourceBus, vehicleQuery);
        }
    }

    private void HandleSaeQuery(ICanBus bus, SaeStandardQueryFrame query)
    {
        Debug.WriteLine($"[PCM]   SAE: Service=0x{(byte)query.Service:X2} PID=0x{(byte)query.Pid:X2}");

        switch (query.Service)
        {
            case Service.Current:
                HandleService01(bus, query.Pid);
                break;
            case Service.VehicleInfo:
                HandleService09(bus, query.Pid);
                break;
        }
    }

    private void HandleVehicleSpecificQuery(ICanBus bus, VehicleSpecificQueryFrame query)
    {
        Debug.WriteLine($"[PCM]   Vehicle-specific: Service=0x{(byte)query.Service:X2} PID=0x{(byte)query.Pid:X2} Frame={query.FrameNumber}");

        switch (query.Service)
        {
            case Service.FreezeFrame:
                HandleService02(bus, query.Pid, query.FrameNumber);
                break;
        }
    }

    private void HandleService01(ICanBus bus, Pid pid)
    {
        if (pid == Pid.SupportedPids_01_20)
        {
            SendResponse(bus, new Obd2ResponseFrame(Service.Current, pid, SupportedPidMask, ModuleAddress));
            return;
        }

        if (_pidHandlers.TryGetValue(pid, out var handler))
        {
            var data = handler();
            if (data != null)
                SendResponse(bus, new Obd2ResponseFrame(Service.Current, pid, data, ModuleAddress));
        }
    }

    private void HandleService02(ICanBus bus, Pid pid, byte frameNumber)
    {
        var ff = _store.FreezeFrame;
        if (ff is null) return;

        if (pid == Pid.SupportedPids_01_20)
        {
            var ffMask = BuildFreezeFrameSupportedPidMask(ff);
            SendResponse(bus, new Obd2ResponseFrame(Service.FreezeFrame, pid, Prepend(frameNumber, ffMask), ModuleAddress));
            return;
        }

        if (pid == Pid.FreezeDtc)
        {
            var dtcBytes = ff.TriggeringDtc.ToBytes();
            SendResponse(bus, new Obd2ResponseFrame(Service.FreezeFrame, pid, [frameNumber, dtcBytes[0], dtcBytes[1]], ModuleAddress));
            return;
        }

        if (ff.Data.TryGetValue(pid, out var data))
            SendResponse(bus, new Obd2ResponseFrame(Service.FreezeFrame, pid, Prepend(frameNumber, data), ModuleAddress));
    }

    private static byte[] BuildFreezeFrameSupportedPidMask(FreezeFrameSnapshot ff)
    {
        uint mask = 0;
        // PID 0x02 FreezeDtc - always present
        mask |= 1u << (32 - 0x02);

        foreach (var pid in ff.Data.Keys)
        {
            byte p = (byte)pid;
            if (p >= 0x01 && p <= 0x1F)
                mask |= 1u << (32 - p);
        }

        var bytes = BitConverter.GetBytes(mask);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    private static byte[] Prepend(byte prefix, byte[] data)
    {
        var result = new byte[data.Length + 1];
        result[0] = prefix;
        Array.Copy(data, 0, result, 1, data.Length);
        return result;
    }

    private void HandleServiceOnlyQuery(ICanBus bus, ServiceOnlyQueryFrame query)
    {
        Debug.WriteLine($"[PCM]   SAE: Service=0x{(byte)query.Service:X2} (no PID)");

        switch (query.Service)
        {
            case Service.StoredDtcs:
                HandleDtcResponse(bus, Service.StoredDtcs, GetStoredDtcs());
                break;
            case Service.ClearDtcs:
                HandleService04(bus);
                break;
            case Service.PendingDtcs:
                HandleDtcResponse(bus, Service.PendingDtcs, GetPendingDtcs());
                break;
            case Service.PermanentDtcs:
                HandleDtcResponse(bus, Service.PermanentDtcs, GetPermanentDtcs());
                break;
        }
    }

    private void HandleDtcResponse(ICanBus bus, Service service, IReadOnlyList<Dtc> dtcs)
    {
        var payload = new byte[2 + dtcs.Count * 2];
        payload[0] = (byte)((byte)service | 0x40);
        payload[1] = (byte)dtcs.Count;
        for (int i = 0; i < dtcs.Count; i++)
        {
            var bytes = dtcs[i].ToBytes();
            payload[2 + i * 2] = bytes[0];
            payload[2 + i * 2 + 1] = bytes[1];
        }
        _ = SendIsoTpResponse(bus, ModuleAddress, TesterAddress, payload);
    }

    private void HandleService04(ICanBus bus)
    {
        ClearAllDtcs();
        OnDtcsCleared();
        _ = SendIsoTpResponse(bus, ModuleAddress, TesterAddress, new byte[] { 0x44 });
    }

    protected virtual void OnDtcsCleared() { }

    private void HandleService09(ICanBus bus, Pid pid)
    {
        switch (pid)
        {
            case Pid.SupportedPids_01_20:
                // Service 09 supported PIDs bitmask: bit 30 = PID 02 (VIN)
                uint mask = 0;
                mask |= 1u << (32 - 0x02); // VIN
                var maskBytes = BitConverter.GetBytes(mask);
                if (BitConverter.IsLittleEndian) Array.Reverse(maskBytes);
                SendResponse(bus, new Obd2ResponseFrame(Service.VehicleInfo, pid, maskBytes, ModuleAddress));
                break;

            case (Pid)0x02: // VIN
                // Payload: [0x49, 0x02, 0x01, VIN bytes (17)]
                var vinBytes = Encoding.ASCII.GetBytes(Vin.PadRight(17).Substring(0, 17));
                var payload = new byte[3 + vinBytes.Length];
                payload[0] = 0x49; // Service 09 response (0x09 | 0x40)
                payload[1] = 0x02; // PID
                payload[2] = 0x01; // message count
                Array.Copy(vinBytes, 0, payload, 3, vinBytes.Length);
                _ = SendIsoTpResponse(bus, ModuleAddress, TesterAddress, payload);
                break;
        }
    }

    public void SetDtc(Dtc dtc)
    {
        _store.AddStoredDtc(dtc);

        if (_store.FreezeFrame is null)
        {
            var data = new Dictionary<Pid, byte[]>();
            foreach (var (pid, handler) in _pidHandlers)
            {
                var bytes = handler();
                if (bytes != null) data[pid] = bytes;
            }
            _store.SetFreezeFrame(new FreezeFrameSnapshot { TriggeringDtc = dtc, Data = data });
        }
    }

    public void ClearDtc(Dtc dtc) => _store.RemoveStoredDtc(dtc);

    public void SetPendingDtc(Dtc dtc) => _store.AddPendingDtc(dtc);
    public void ClearPendingDtc(Dtc dtc) => _store.RemovePendingDtc(dtc);

    public void SetPermanentDtc(Dtc dtc) => _store.AddPermanentDtc(dtc);
    public void ClearPermanentDtc(Dtc dtc) => _store.RemovePermanentDtc(dtc);

    public void ClearAllDtcs() => _store.ClearAllDtcs();

    protected virtual IReadOnlyList<Dtc> GetStoredDtcs() => _store.StoredDtcs;
    protected virtual IReadOnlyList<Dtc> GetPendingDtcs() => _store.PendingDtcs;
    protected virtual IReadOnlyList<Dtc> GetPermanentDtcs() => _store.PermanentDtcs;

    protected virtual EmissionsReadinessStatus GetEmissionsReadiness() => new EmissionsReadinessStatus();
    protected virtual Temperature? GetEngineCoolantTemperature() => null;
    protected virtual float? GetEngineRpm() => null;
    protected virtual Speed? GetVehicleSpeed() => null;
    protected virtual float? GetThrottlePosition() => null; // percent 0-100
    protected virtual Temperature? GetTransFluidTemp() => null;

    protected void SendResponse(ICanBus bus, Obd2ResponseFrame response)
    {
        bus.WriteFrame(response);
    }

    protected async Task SendIsoTpResponse(ICanBus bus, short ecuAddress, short testerAddress, byte[] data)
    {
        var frames = IsoTp.Encode(data);

        if (frames.Length == 1)
        {
            // Single frame — set address and send
            if (frames[0] is StandardDataFrame sf)
            {
                sf.ID = ecuAddress;
                bus.WriteFrame(sf);
            }
            return;
        }

        // Multi-frame: send First Frame, wait for Flow Control, send Consecutive Frames
        if (frames[0] is not StandardDataFrame firstFrame) return;
        firstFrame.ID = ecuAddress;
        bus.WriteFrame(firstFrame);

        // Wait for Flow Control (scanner sends to tester address, e.g. 0x7E0)
        var tcs = new TaskCompletionSource<bool>();
        EventHandler<ICanFrame>? fcHandler = null;
        fcHandler = (_, frame) =>
        {
            if (frame is StandardDataFrame sdf &&
                sdf.ID == testerAddress &&
                (sdf.Payload[0] & 0xF0) == 0x30) // Flow Control frame type
            {
                bus.FrameReceived -= fcHandler;
                tcs.TrySetResult(true);
            }
        };
        bus.FrameReceived += fcHandler;

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(1000));
        if (completed != tcs.Task)
        {
            bus.FrameReceived -= fcHandler;
            Debug.WriteLine("[PCM] ISO-TP: timeout waiting for flow control");
            return;
        }

        // Send Consecutive Frames
        for (int i = 1; i < frames.Length; i++)
        {
            if (frames[i] is StandardDataFrame cf)
            {
                cf.ID = ecuAddress;
                bus.WriteFrame(cf);
                await Task.Delay(1);
            }
        }
    }
}

internal class CanBusMonitor
{
    public event EventHandler<Obd2QueryFrame>? QueryReceived;

    public ICanBus Bus { get; }

    public CanBusMonitor(ICanBus bus)
    {
        Bus = bus;
        bus.FrameReceived += OnFrameReceived;
    }

    private void OnFrameReceived(object? sender, ICanFrame frame)
    {
        if (frame is not StandardDataFrame sdf) return;
        if (sdf.ID != Obd2Frame.Obd2RequestID) return;

        Obd2QueryFrame query;
        try
        {
            if (Obd2Frame.FromCanFrame(sdf) is not Obd2QueryFrame q) return;
            query = q;
        }
        catch
        {
            // not a valid OBD2 frame - ignore
            return;
        }

        // Dispatch off the receive thread to avoid re-entrancy issues with the CAN driver
        _ = Task.Run(() =>
        {
            try
            {
                QueryReceived?.Invoke(this, query);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OBD2] Exception handling query: {ex}");
            }
        });
    }
}

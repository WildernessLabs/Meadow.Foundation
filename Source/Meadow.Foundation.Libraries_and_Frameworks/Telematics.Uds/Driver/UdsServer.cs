using Meadow.Foundation.Telematics.J1979;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// The ECU side of ISO 14229: listens for UDS requests on a module's physical address and on the
/// functional broadcast address, and answers them from an <see cref="IUdsDataSource"/>.
/// </summary>
/// <remarks>
/// This subscribes to the bus independently of the J1979 <c>ControllerBase</c>, so a module can
/// serve both protocols at once. A UDS request parses as a J1979 frame whose service byte matches
/// nothing, so the J1979 side stays silent and there is no double response.
/// </remarks>
public class UdsServer : IDisposable
{
    /// <summary>Where a tester's flow control comes from — the module's own request address.</summary>
    public short RequestAddress { get; }

    /// <summary>The address this module answers from, e.g. 0x7E8.</summary>
    public short ResponseAddress { get; }

    /// <summary>The session/keep-alive state, exposed so a host can serve DID $F186.</summary>
    public UdsSessionState Session { get; } = new();

    private readonly ICanBus[] _buses;
    private readonly IUdsDataSource _source;
    private readonly int _responseDelayMs;
    private readonly EventHandler<ICanFrame> _handler;
    private bool _disposed;

    /// <summary>
    /// Starts serving UDS for one module.
    /// </summary>
    /// <param name="canBuses">Buses to listen on.</param>
    /// <param name="responseAddress">The module's response ID (e.g. 0x7E8); requests arrive at that minus 8.</param>
    /// <param name="source">Supplies the DTCs and DIDs this module reports.</param>
    public UdsServer(ICanBus[] canBuses, short responseAddress, IUdsDataSource source)
    {
        _buses = canBuses;
        _source = source;
        ResponseAddress = responseAddress;
        RequestAddress = (short)(responseAddress - Obd2Addresses.EcuPhysicalOffset);

        // Stagger by address the way CanBusMonitor does, so lower-addressed modules answer a
        // functional request first and scan tools that assume ascending order keep working.
        _responseDelayMs = (responseAddress - Obd2Addresses.EcuResponseBase) * 5;

        _handler = OnFrameReceived;
        foreach (var bus in _buses)
        {
            bus.FrameReceived += _handler;
        }
    }

    private void OnFrameReceived(object? sender, ICanFrame frame)
    {
        if (frame is not StandardDataFrame sdf) return;

        bool functional = sdf.ID == Obd2Addresses.FunctionalRequest;
        if (!functional && sdf.ID != RequestAddress) return;

        var payload = sdf.Payload;
        if (payload == null || payload.Length == 0) return;

        // UDS requests reach us as ISO-TP single frames: upper nibble 0, lower nibble = length.
        if ((payload[0] & 0xF0) != 0) return;

        int length = payload[0] & 0x0F;
        if (length == 0 || length > payload.Length - 1) return;

        var request = new byte[length];
        Array.Copy(payload, 1, request, 0, length);

        var bus = sender as ICanBus ?? _buses[0];

        _ = Task.Run(async () =>
        {
            try
            {
                if (_responseDelayMs > 0) await Task.Delay(_responseDelayMs);
                await HandleRequestAsync(bus, request, functional);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UDS 0x{ResponseAddress:X3}] Exception handling request: {ex}");
            }
        });
    }

    private async Task HandleRequestAsync(ICanBus bus, byte[] request, bool functional)
    {
        Session.KeepAlive();

        var service = (UdsService)request[0];
        Debug.WriteLine($"[UDS 0x{ResponseAddress:X3}] Service=0x{request[0]:X2} len={request.Length}");

        switch (service)
        {
            case UdsService.DiagnosticSessionControl:
                await HandleSessionControl(bus, request, functional);
                break;
            case UdsService.ClearDiagnosticInformation:
                await HandleClearDtcs(bus, request, functional);
                break;
            case UdsService.ReadDtcInformation:
                await HandleReadDtcInformation(bus, request, functional);
                break;
            case UdsService.ReadDataByIdentifier:
                await HandleReadDataByIdentifier(bus, request, functional);
                break;
            case UdsService.TesterPresent:
                await HandleTesterPresent(bus, request, functional);
                break;
            default:
                await SendNegative(bus, request[0], UdsNrc.ServiceNotSupported, functional);
                break;
        }
    }

    private async Task HandleSessionControl(ICanBus bus, byte[] request, bool functional)
    {
        if (request.Length < 2)
        {
            await SendNegative(bus, request[0], UdsNrc.IncorrectMessageLengthOrInvalidFormat, functional);
            return;
        }

        var requested = (UdsSessionType)(request[1] & 0x7F);
        if (!Enum.IsDefined(typeof(UdsSessionType), requested))
        {
            await SendNegative(bus, request[0], UdsNrc.SubFunctionNotSupported, functional);
            return;
        }

        Session.Set(requested);

        // 50 <session> P2 = 0x0032 (50 ms), P2* = 0x01F4 (5 s, in 10 ms units)
        await Send(bus, [PositiveResponse(request[0]), (byte)requested, 0x00, 0x32, 0x01, 0xF4]);
    }

    private async Task HandleClearDtcs(ICanBus bus, byte[] request, bool functional)
    {
        // 14 <groupHi> <groupMid> <groupLo>; 0xFFFFFF means "all groups"
        if (request.Length < 4)
        {
            await SendNegative(bus, request[0], UdsNrc.IncorrectMessageLengthOrInvalidFormat, functional);
            return;
        }

        _source.ClearDtcs();
        await Send(bus, [PositiveResponse(request[0])]);
    }

    private async Task HandleReadDtcInformation(ICanBus bus, byte[] request, bool functional)
    {
        if (request.Length < 2)
        {
            await SendNegative(bus, request[0], UdsNrc.IncorrectMessageLengthOrInvalidFormat, functional);
            return;
        }

        var subFunction = (UdsDtcSubFunction)request[1];
        byte availability = (byte)_source.AvailabilityMask;

        switch (subFunction)
        {
            case UdsDtcSubFunction.ReportNumberOfDtcByStatusMask:
                if (request.Length < 3)
                {
                    await SendNegative(bus, request[0], UdsNrc.IncorrectMessageLengthOrInvalidFormat, functional);
                    return;
                }
                else
                {
                    var count = MatchingDtcs(request[2]).Count;
                    // 59 01 <availability> <format 0x01 = ISO 14229-1 3-byte DTC> <countHi> <countLo>
                    await Send(bus, [PositiveResponse(request[0]), request[1], availability, 0x01,
                        (byte)(count >> 8), (byte)(count & 0xFF)]);
                }
                return;

            case UdsDtcSubFunction.ReportDtcByStatusMask:
                if (request.Length < 3)
                {
                    await SendNegative(bus, request[0], UdsNrc.IncorrectMessageLengthOrInvalidFormat, functional);
                    return;
                }
                await SendDtcList(bus, request[0], request[1], availability, MatchingDtcs(request[2]));
                return;

            case UdsDtcSubFunction.ReportSupportedDtcs:
                await SendDtcList(bus, request[0], request[1], availability, _source.GetDtcs());
                return;

            default:
                await SendNegative(bus, request[0], UdsNrc.SubFunctionNotSupported, functional);
                return;
        }
    }

    private IReadOnlyList<UdsDtcRecord> MatchingDtcs(byte statusMask)
        => _source.GetDtcs().Where(d => ((byte)d.Status & statusMask) != 0).ToList();

    private async Task SendDtcList(ICanBus bus, byte requestService, byte subFunction, byte availability,
        IReadOnlyList<UdsDtcRecord> dtcs)
    {
        // 59 <sub> <availability> then a 4-byte record per DTC
        var payload = new byte[3 + dtcs.Count * 4];
        payload[0] = PositiveResponse(requestService);
        payload[1] = subFunction;
        payload[2] = availability;

        for (int i = 0; i < dtcs.Count; i++)
        {
            var bytes = dtcs[i].ToBytes();
            Array.Copy(bytes, 0, payload, 3 + i * 4, 4);
        }

        await Send(bus, payload);
    }

    private async Task HandleReadDataByIdentifier(ICanBus bus, byte[] request, bool functional)
    {
        if (request.Length < 3 || (request.Length - 1) % 2 != 0)
        {
            await SendNegative(bus, request[0], UdsNrc.IncorrectMessageLengthOrInvalidFormat, functional);
            return;
        }

        var response = new List<byte> { PositiveResponse(request[0]) };
        bool any = false;

        // 22 <did>[<did>...] — a request may ask for several identifiers at once.
        for (int i = 1; i + 1 < request.Length; i += 2)
        {
            ushort did = (ushort)((request[i] << 8) | request[i + 1]);
            if (!_source.TryGetDid(did, out var data)) continue;

            any = true;
            response.Add((byte)(did >> 8));
            response.Add((byte)(did & 0xFF));
            response.AddRange(data);
        }

        if (!any)
        {
            await SendNegative(bus, request[0], UdsNrc.RequestOutOfRange, functional);
            return;
        }

        await Send(bus, [.. response]);
    }

    private async Task HandleTesterPresent(ICanBus bus, byte[] request, bool functional)
    {
        if (request.Length < 2)
        {
            await SendNegative(bus, request[0], UdsNrc.IncorrectMessageLengthOrInvalidFormat, functional);
            return;
        }

        // Bit 7 of the sub-function is suppressPosRspMsgIndicationBit.
        if ((request[1] & 0x80) != 0) return;

        await Send(bus, [PositiveResponse(request[0]), (byte)(request[1] & 0x7F)]);
    }

    private static byte PositiveResponse(byte requestService)
        => (byte)(requestService + Obd2Addresses.ResponseOffset);

    private Task Send(ICanBus bus, byte[] payload)
        => IsoTpResponder.SendAsync(bus, ResponseAddress, RequestAddress, payload);

    private Task SendNegative(ICanBus bus, byte requestService, UdsNrc nrc, bool functional)
    {
        // ISO 14229-1: a functionally addressed request that would be rejected is answered with
        // silence, so a tester scanning 0x7DF does not mistake refusals for present modules.
        if (functional) return Task.CompletedTask;

        return Send(bus, UdsProtocol.BuildNegativeResponse(requestService, nrc));
    }

    /// <summary>Stops listening on every bus.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var bus in _buses)
        {
            bus.FrameReceived -= _handler;
        }

        GC.SuppressFinalize(this);
    }
}

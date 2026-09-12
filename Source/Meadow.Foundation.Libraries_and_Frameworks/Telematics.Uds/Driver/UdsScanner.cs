using Meadow;
using Meadow.Foundation.Telematics.J1979;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// UDS (ISO 14229) diagnostic client communicating over an <see cref="ICanBus"/> using ISO-TP.
/// </summary>
public class UdsScanner : IUdsScanner
{
    private static readonly TimeSpan PhysicalTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan BroadcastCollectTimeout = TimeSpan.FromMilliseconds(1200);

    /// <summary>
    /// How long a burst of probe requests is left to collect answers.
    /// <para>
    /// Discovery cannot wait out a timeout per address. A three-tier sweep visits about 500
    /// addresses, and at the 2 s physical timeout that is over a quarter of an hour — so the sweep
    /// fires a whole batch and listens once, which is what makes it finish in seconds.
    /// </para>
    /// </summary>
    private static readonly TimeSpan ProbeCollectTimeout = TimeSpan.FromMilliseconds(400);

    /// <summary>
    /// Probe requests sent before pausing to collect. Small enough not to overrun a module's
    /// receive buffer or the bus itself, large enough that the collect window is amortised.
    /// </summary>
    private const int ProbeBatchSize = 16;

    /// <summary>Gap between frames in a burst, so a slow ECU is not talked over.</summary>
    private const int ProbeFrameGapMs = 2;

    private readonly ICanBus _bus;
    private readonly IUdsDescriptionProvider _descriptions;

    /// <summary>
    /// Address bands the sweep walks. Defaults to the ISO-generic three-tier plan; a host can
    /// replace it with one loaded from configuration to add manufacturer address maps without a
    /// code change.
    /// </summary>
    public UdsDiscoveryPlan Plan { get; set; } = UdsDiscoveryPlan.Default();

    /// <summary>
    /// Creates a scanner on the given bus.
    /// </summary>
    /// <param name="bus">The CAN bus to talk to.</param>
    /// <param name="descriptions">
    /// Supplies DID names, DTC text and NRC descriptions. Defaults to <see cref="NullUdsDescriptions"/>,
    /// which decodes correctly but describes nothing.
    /// </param>
    public UdsScanner(ICanBus bus, IUdsDescriptionProvider? descriptions = null)
    {
        _bus = bus;
        _descriptions = descriptions ?? NullUdsDescriptions.Instance;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<UdsModuleInfo>> DiscoverModulesAsync(CancellationToken ct = default)
        => DiscoverModulesAsync(Plan, null, ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UdsModuleInfo>> DiscoverModulesAsync(
        UdsDiscoveryPlan plan,
        IProgress<UdsDiscoveryProgress>? progress,
        CancellationToken ct = default)
    {
        Resolver.Log?.Info($"UdsScanner: sweeping {plan.TotalAddresses} addresses across {plan.EnabledTiers.Count()} tier(s)...");

        var found = new Dictionary<UdsAddress, byte[]?>();
        var tiers = plan.EnabledTiers.ToList();
        var total = plan.TotalAddresses;
        var probed = 0;

        // The functional broadcast is still worth one shot first: on a vehicle whose modules all
        // answer 0x7DF it finds them in a single exchange, and it costs one request.
        var broadcast = await SendAndCollectAll(
            (ushort)Obd2Addresses.FunctionalRequest,
            [(byte)UdsService.ReadDtcInformation, (byte)UdsDtcSubFunction.ReportDtcByStatusMask, 0xFF],
            (byte)((byte)UdsService.ReadDtcInformation + Obd2Addresses.ResponseOffset),
            ct);

        foreach (var (rxId, payload) in broadcast)
        {
            found[UdsAddress.Standard((uint)(rxId - Obd2Addresses.EcuPhysicalOffset), rxId)] = payload;
        }

        for (var tierIndex = 0; tierIndex < tiers.Count; tierIndex++)
        {
            if (ct.IsCancellationRequested) break;

            var tier = tiers[tierIndex];
            var addresses = tier.Addresses().ToList();

            foreach (var batch in Chunk(addresses, ProbeBatchSize))
            {
                if (ct.IsCancellationRequested) break;

                var responders = await ProbeBatchAsync(batch, ct);

                foreach (var address in responders)
                {
                    // Already known from the broadcast — keep the richer payload we have.
                    if (!found.ContainsKey(address)) found[address] = null;
                }

                probed += batch.Count;
                progress?.Report(new UdsDiscoveryProgress(
                    tier.Name, tierIndex, tiers.Count, probed, total, found.Count));
            }
        }

        // Interrogating every responder is the slow part, so it happens once, after the sweep, and
        // only for addresses that actually answered.
        var modules = new List<UdsModuleInfo>();

        foreach (var address in found.Keys.OrderBy(a => a.IsExtended).ThenBy(a => a.TxId))
        {
            if (ct.IsCancellationRequested) break;
            modules.Add(await InterrogateAsync(address, found[address], plan, ct));
        }

        Resolver.Log?.Info($"UdsScanner: found {modules.Count} module(s).");
        return modules;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UdsModuleInfo>> ProbeAddressesAsync(
        IEnumerable<UdsAddress> addresses,
        IProgress<UdsDiscoveryProgress>? progress = null,
        CancellationToken ct = default)
    {
        // The targeted probe a remembered vehicle uses: the same burst-and-collect machinery, given
        // an explicit list instead of a range. Dozens of addresses rather than five hundred, which
        // is the whole point of remembering them.
        var list = addresses.Distinct().ToList();
        var modules = new List<UdsModuleInfo>();
        var probed = 0;

        foreach (var batch in Chunk(list, ProbeBatchSize))
        {
            if (ct.IsCancellationRequested) break;

            var responders = await ProbeBatchAsync(batch, ct);

            foreach (var address in responders)
            {
                if (ct.IsCancellationRequested) break;
                modules.Add(await InterrogateAsync(address, null, Plan, ct));
            }

            probed += batch.Count;
            progress?.Report(new UdsDiscoveryProgress(
                "Remembered addresses", 0, 1, probed, list.Count, modules.Count));
        }

        return modules;
    }

    /// <summary>
    /// Fires a batch of probe requests, then listens once for anything that answers.
    /// </summary>
    /// <remarks>
    /// The probe is TesterPresent with the positive response *not* suppressed, and a negative
    /// response counts as a hit: an ECU that refuses the request has still proved it is there, and
    /// treating an NRC as silence would hide every module in a session state that declines $3E.
    /// </remarks>
    private async Task<List<UdsAddress>> ProbeBatchAsync(List<UdsAddress> batch, CancellationToken ct)
    {
        var byRxId = batch.ToDictionary(a => a.RxId, a => a);
        var responders = new HashSet<UdsAddress>();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(ProbeCollectTimeout);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        EventHandler<ICanFrame>? handler = null;
        handler = (_, frame) =>
        {
            if (!TryReadFrame(frame, out var id, out var payload)) return;
            if (!byRxId.TryGetValue(id, out var address)) return;
            if (!IsTesterPresentReply(payload)) return;

            responders.Add(address);
        };

        cts.Token.Register(() =>
        {
            _bus.FrameReceived -= handler;
            tcs.TrySetResult(true);
        });

        _bus.FrameReceived += handler;

        foreach (var address in batch)
        {
            if (ct.IsCancellationRequested) break;

            // Sub-function 0x00: do NOT suppress the positive response. A suppressed response is
            // exactly what a probe must not ask for — silence would be indistinguishable from an
            // absent module.
            SendRequest(address, [(byte)UdsService.TesterPresent, 0x00]);
            if (ProbeFrameGapMs > 0) await Task.Delay(ProbeFrameGapMs, ct).ConfigureAwait(false);
        }

        await tcs.Task;
        return [.. responders];
    }

    /// <summary>
    /// Whether an ISO-TP single frame is a module answering the TesterPresent probe — either the
    /// positive response <c>$7E</c> or a negative response naming service <c>$3E</c>.
    /// </summary>
    /// <remarks>
    /// A negative response counts: an ECU that refuses $3E in its current session has still proved
    /// it is there, and discarding NRCs would hide every such module.
    /// <para>
    /// Accepting *any* frame on the address would be wrong even though it looks more permissive.
    /// Tier request and response ranges overlap — a request at <c>0x708</c> shares an identifier
    /// with the response address of <c>0x700</c> — so unrelated traffic, or a bus that echoes
    /// transmissions, registers as a module. Each phantom then costs a full interrogation: a DTC
    /// read plus five DID reads, every one waiting out the physical timeout.
    /// </para>
    /// </remarks>
    private static bool IsTesterPresentReply(byte[] payload)
    {
        if (payload.Length < 2) return false;

        // Single frame: upper nibble zero, lower nibble the length.
        if ((payload[0] & 0xF0) != 0) return false;

        int length = payload[0] & 0x0F;
        if (length == 0 || length > payload.Length - 1) return false;

        var service = payload[1];

        if (service == (byte)UdsService.TesterPresent + Obd2Addresses.ResponseOffset) return true;

        return service == (byte)UdsService.NegativeResponse
               && length >= 2
               && payload[2] == (byte)UdsService.TesterPresent;
    }

    /// <summary>Asks a responding address who it is and what is wrong with it.</summary>
    private async Task<UdsModuleInfo> InterrogateAsync(
        UdsAddress address, byte[]? knownDtcPayload, UdsDiscoveryPlan plan, CancellationToken ct)
    {
        IReadOnlyList<UdsDtc> dtcs = knownDtcPayload != null
            ? UdsProtocol.ParseDtcResponse(knownDtcPayload, _descriptions)
            : await ReadModuleDtcsAsync(address, ct);

        var vinDid = await ReadDidAsync(address, 0xF190, ct);
        var partDid = await ReadDidAsync(address, 0xF187, ct);
        var swDid = await ReadDidAsync(address, 0xF189, ct);
        var hwDid = await ReadDidAsync(address, 0xF191, ct);
        var sysDid = await ReadDidAsync(address, 0xF197, ct);

        var name = sysDid?.DisplayValue
                   ?? plan.NameFor(address)
                   ?? GetDefaultModuleName(address);

        return new UdsModuleInfo(
            address,
            name,
            sysDid?.DisplayValue,
            partDid?.DisplayValue,
            swDid?.DisplayValue,
            hwDid?.DisplayValue,
            vinDid?.DisplayValue,
            dtcs);
    }

    private static IEnumerable<List<T>> Chunk<T>(List<T> source, int size)
    {
        for (var i = 0; i < source.Count; i += size)
        {
            yield return source.GetRange(i, Math.Min(size, source.Count - i));
        }
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<UdsDtc>> ReadModuleDtcsAsync(ushort txId, ushort rxId, CancellationToken ct = default)
        => ReadModuleDtcsAsync(UdsAddress.Standard(txId, rxId), ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UdsDtc>> ReadModuleDtcsAsync(UdsAddress address, CancellationToken ct = default)
    {
        var response = await SendAndReceivePhysical(
            address,
            [(byte)UdsService.ReadDtcInformation, (byte)UdsDtcSubFunction.ReportDtcByStatusMask, 0xFF],
            (byte)((byte)UdsService.ReadDtcInformation + Obd2Addresses.ResponseOffset),
            ct);

        return response != null ? UdsProtocol.ParseDtcResponse(response, _descriptions) : [];
    }

    /// <inheritdoc/>
    public Task<bool> ClearModuleDtcsAsync(ushort txId, ushort rxId, CancellationToken ct = default)
        => ClearModuleDtcsAsync(UdsAddress.Standard(txId, rxId), ct);

    /// <inheritdoc/>
    public async Task<bool> ClearModuleDtcsAsync(UdsAddress address, CancellationToken ct = default)
    {
        var response = await SendAndReceivePhysical(
            address,
            [(byte)UdsService.ClearDiagnosticInformation, 0xFF, 0xFF, 0xFF],
            (byte)((byte)UdsService.ClearDiagnosticInformation + Obd2Addresses.ResponseOffset),
            ct);

        return response != null && response.Length > 0 &&
               response[0] == (byte)((byte)UdsService.ClearDiagnosticInformation + Obd2Addresses.ResponseOffset);
    }

    /// <inheritdoc/>
    public async Task<bool> ClearAllDtcsAsync(CancellationToken ct = default)
    {
        SendRequest(UdsAddress.Standard((uint)Obd2Addresses.FunctionalRequest),
            [(byte)UdsService.ClearDiagnosticInformation, 0xFF, 0xFF, 0xFF]);
        await Task.Delay(500, ct);
        return true;
    }

    /// <inheritdoc/>
    public Task<UdsDidValue?> ReadDidAsync(ushort txId, ushort rxId, ushort did, CancellationToken ct = default)
        => ReadDidAsync(UdsAddress.Standard(txId, rxId), did, ct);

    /// <inheritdoc/>
    public async Task<UdsDidValue?> ReadDidAsync(UdsAddress address, ushort did, CancellationToken ct = default)
    {
        byte didHi = (byte)(did >> 8);
        byte didLo = (byte)(did & 0xFF);

        var response = await SendAndReceivePhysical(
            address,
            [(byte)UdsService.ReadDataByIdentifier, didHi, didLo],
            (byte)((byte)UdsService.ReadDataByIdentifier + Obd2Addresses.ResponseOffset),
            ct);

        return response != null ? UdsProtocol.ParseDidResponse(did, response, _descriptions) : null;
    }

    /// <inheritdoc/>
    public Task<bool> SetDiagnosticSessionAsync(ushort txId, ushort rxId, UdsSessionType session, CancellationToken ct = default)
        => SetDiagnosticSessionAsync(UdsAddress.Standard(txId, rxId), session, ct);

    /// <inheritdoc/>
    public async Task<bool> SetDiagnosticSessionAsync(UdsAddress address, UdsSessionType session, CancellationToken ct = default)
    {
        var response = await SendAndReceivePhysical(
            address,
            [(byte)UdsService.DiagnosticSessionControl, (byte)session],
            (byte)((byte)UdsService.DiagnosticSessionControl + Obd2Addresses.ResponseOffset),
            ct);

        return response != null && response.Length > 0 &&
               response[0] == (byte)((byte)UdsService.DiagnosticSessionControl + Obd2Addresses.ResponseOffset);
    }

    /// <inheritdoc/>
    public Task SendTesterPresentAsync(ushort txId, bool suppressResponse = true, CancellationToken ct = default)
        => SendTesterPresentAsync(UdsAddress.Standard(txId), suppressResponse, ct);

    /// <inheritdoc/>
    public Task SendTesterPresentAsync(UdsAddress address, bool suppressResponse = true, CancellationToken ct = default)
    {
        byte subFunc = (byte)(suppressResponse ? 0x80 : 0x00);
        SendRequest(address, [(byte)UdsService.TesterPresent, subFunc]);
        return Task.CompletedTask;
    }

    private static string GetDefaultModuleName(UdsAddress address)
    {
        if (address.IsExtended) return $"ECU 0x{address.EcuAddress:X2} ({address.TxIdHex})";

        return address.RxId switch
        {
            0x7E8 => "PCM (Powertrain)",
            0x7E9 => "TCU (Transmission)",
            0x7EA => "BCM (Body)",
            0x7EB => "HVAC (Climate)",
            0x7EC => "ABS (Brakes)",
            0x7ED => "SRS (Airbag)",
            0x7EE => "IC (Instrument Cluster)",
            0x7EF => "GW (Gateway)",
            _ => $"ECU ({address.TxIdHex})"
        };
    }

    // ── Frame plumbing ───────────────────────────────────────────────────────

    /// <summary>
    /// Reads a CAN frame's identifier and payload regardless of whether it is 11-bit or 29-bit.
    /// Everything here used to assume <see cref="StandardDataFrame"/>, which silently dropped every
    /// 29-bit response — the modules the sweep most wants to find.
    /// </summary>
    private static bool TryReadFrame(ICanFrame frame, out uint id, out byte[] payload)
    {
        switch (frame)
        {
            case ExtendedDataFrame edf:
                id = (uint)edf.ID;
                payload = edf.Payload ?? [];
                return true;
            case StandardDataFrame sdf:
                id = (uint)sdf.ID;
                payload = sdf.Payload ?? [];
                return true;
            default:
                id = 0;
                payload = [];
                return false;
        }
    }

    private ICanFrame MakeFrame(UdsAddress address, uint id, byte[] payload)
        => address.IsExtended
            ? new ExtendedDataFrame { ID = (int)id, Payload = payload }
            : new StandardDataFrame { ID = (short)id, Payload = payload };

    private void SendRequest(UdsAddress address, byte[] payloadData)
    {
        var payload = new byte[8];
        payload[0] = (byte)payloadData.Length;
        Array.Copy(payloadData, 0, payload, 1, Math.Min(7, payloadData.Length));
        _bus.WriteFrame(MakeFrame(address, address.TxId, payload));
    }

    private void SendRequest(ushort targetId, byte[] payloadData)
        => SendRequest(UdsAddress.Standard(targetId), payloadData);

    private void SendFlowControl(UdsAddress address)
    {
        byte fcByte = (byte)((byte)IsoTpFrameType.FlowControl << 4);
        var payload = new byte[] { fcByte, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        _bus.WriteFrame(MakeFrame(address, address.FlowControlId, payload));
    }

    private async Task<byte[]?> SendAndReceivePhysical(
        UdsAddress address, byte[] payloadData, byte expectedResponseService, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(PhysicalTimeout);

        var tcs = new TaskCompletionSource<byte[]?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var assembler = new MultiFrameAssembler();

        EventHandler<ICanFrame>? handler = null;
        handler = (_, frame) =>
        {
            if (!TryReadFrame(frame, out var id, out var p)) return;
            if (id != address.RxId) return;
            if (p.Length == 0) return;

            byte frameTypeByte = p[0];
            var frameType = (IsoTpFrameType)(frameTypeByte >> 4);

            if (frameType == IsoTpFrameType.Single)
            {
                int len = frameTypeByte & 0x0F;
                if (len == 0 || len > p.Length - 1) return;
                var data = new byte[len];
                Array.Copy(p, 1, data, 0, len);

                if (data.Length > 0 && data[0] == expectedResponseService)
                {
                    _bus.FrameReceived -= handler;
                    tcs.TrySetResult(data);
                }
                else if (data.Length > 0 && data[0] == (byte)UdsService.NegativeResponse)
                {
                    _bus.FrameReceived -= handler;
                    Resolver.Log?.Trace($"UDS RX {address.RxIdHex}: NRC for service 0x{expectedResponseService:X2}");
                    tcs.TrySetResult(null);
                }
            }
            else if (frameType == IsoTpFrameType.First)
            {
                int totalLen = ((frameTypeByte & 0x0F) << 8) | p[1];
                int firstBytes = Math.Min(6, p.Length - 2);
                var initial = new byte[firstBytes];
                Array.Copy(p, 2, initial, 0, firstBytes);
                assembler.Start(totalLen, initial);
                SendFlowControl(address);
            }
            else if (frameType == IsoTpFrameType.Consecutive)
            {
                int available = p.Length - 1;
                if (available <= 0) return;
                var chunk = new byte[available];
                Array.Copy(p, 1, chunk, 0, available);
                assembler.Append(chunk);

                if (assembler.IsComplete)
                {
                    var data = assembler.GetData();
                    _bus.FrameReceived -= handler;
                    tcs.TrySetResult(data.Length > 0 && data[0] == expectedResponseService ? data : null);
                }
            }
        };

        cts.Token.Register(() =>
        {
            _bus.FrameReceived -= handler;
            tcs.TrySetResult(null);
        });

        _bus.FrameReceived += handler;
        SendRequest(address, payloadData);

        return await tcs.Task;
    }

    private async Task<Dictionary<ushort, byte[]>> SendAndCollectAll(
        ushort functionalTxId, byte[] payloadData, byte expectedResponseService, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(BroadcastCollectTimeout);

        var results = new Dictionary<ushort, byte[]>();
        var assemblers = new Dictionary<ushort, MultiFrameAssembler>();
        var tcs = new TaskCompletionSource<Dictionary<ushort, byte[]>>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        EventHandler<ICanFrame>? handler = null;
        handler = (_, frame) =>
        {
            if (frame is not StandardDataFrame sdf) return;
            if (sdf.ID < Obd2Addresses.EcuResponseBase || sdf.ID > Obd2Addresses.EcuResponseMax) return;

            var p = sdf.Payload;
            if (p == null || p.Length == 0) return;

            byte frameTypeByte = p[0];
            var frameType = (IsoTpFrameType)(frameTypeByte >> 4);

            if (frameType == IsoTpFrameType.Single)
            {
                int len = frameTypeByte & 0x0F;
                if (len == 0 || len > p.Length - 1) return;
                var data = new byte[len];
                Array.Copy(p, 1, data, 0, len);
                if (data.Length > 0 && data[0] == expectedResponseService)
                    results[(ushort)sdf.ID] = data;
            }
            else if (frameType == IsoTpFrameType.First)
            {
                int totalLen = ((frameTypeByte & 0x0F) << 8) | p[1];
                int firstBytes = Math.Min(6, p.Length - 2);
                var initial = new byte[firstBytes];
                Array.Copy(p, 2, initial, 0, firstBytes);
                var asm = new MultiFrameAssembler();
                assemblers[(ushort)sdf.ID] = asm;
                asm.Start(totalLen, initial);
                SendFlowControl(UdsAddress.Standard((uint)(sdf.ID - Obd2Addresses.EcuPhysicalOffset)));
            }
            else if (frameType == IsoTpFrameType.Consecutive)
            {
                if (!assemblers.TryGetValue((ushort)sdf.ID, out var asm)) return;
                int available = p.Length - 1;
                if (available <= 0) return;
                var chunk = new byte[available];
                Array.Copy(p, 1, chunk, 0, available);
                asm.Append(chunk);
                if (asm.IsComplete)
                {
                    var data = asm.GetData();
                    if (data.Length > 0 && data[0] == expectedResponseService)
                        results[(ushort)sdf.ID] = data;
                }
            }
        };

        cts.Token.Register(() =>
        {
            _bus.FrameReceived -= handler;
            tcs.TrySetResult(results);
        });

        _bus.FrameReceived += handler;
        SendRequest(functionalTxId, payloadData);

        return await tcs.Task;
    }

    private sealed class MultiFrameAssembler
    {
        private int _totalLen;
        private readonly List<byte> _bytes = [];

        public bool IsComplete => _bytes.Count >= _totalLen;

        public void Start(int totalLen, byte[] initial)
        {
            _totalLen = totalLen;
            _bytes.Clear();
            _bytes.AddRange(initial);
        }

        public void Append(byte[] data)
        {
            int remaining = _totalLen - _bytes.Count;
            if (remaining <= 0) return;
            int take = Math.Min(remaining, data.Length);
            for (int i = 0; i < take; i++)
                _bytes.Add(data[i]);
        }

        public byte[] GetData() => [.. _bytes];
    }
}

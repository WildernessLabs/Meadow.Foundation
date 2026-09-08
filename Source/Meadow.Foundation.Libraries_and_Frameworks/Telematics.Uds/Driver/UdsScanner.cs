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

    private readonly ICanBus _bus;
    private readonly IUdsDescriptionProvider _descriptions;

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
    public async Task<IReadOnlyList<UdsModuleInfo>> DiscoverModulesAsync(CancellationToken ct = default)
    {
        Resolver.Log?.Info("UdsScanner.DiscoverModulesAsync: Scanning for UDS modules on CAN bus...");

        // 1. Broadcast probe using Service $19 $02 0xFF (Read DTCs by Status Mask) to 0x7DF
        var broadcastResp = await SendAndCollectAll(
            (ushort)Obd2Addresses.FunctionalRequest,
            [(byte)UdsService.ReadDtcInformation, (byte)UdsDtcSubFunction.ReportDtcByStatusMask, 0xFF],
            (byte)((byte)UdsService.ReadDtcInformation + Obd2Addresses.ResponseOffset),
            ct);

        var discoveredRxIds = new HashSet<ushort>(broadcastResp.Keys);

        // 2. Also probe standard physical addresses 0x7E0..0x7E7 directly with TesterPresent/Session query if no broadcast responses
        if (discoveredRxIds.Count == 0)
        {
            for (ushort txId = 0x7E0; txId <= 0x7E7; txId++)
            {
                if (ct.IsCancellationRequested) break;
                ushort rxId = (ushort)(txId + Obd2Addresses.EcuPhysicalOffset);
                var dtcData = await SendAndReceivePhysical(
                    txId, rxId,
                    [(byte)UdsService.ReadDtcInformation, (byte)UdsDtcSubFunction.ReportDtcByStatusMask, 0xFF],
                    (byte)((byte)UdsService.ReadDtcInformation + Obd2Addresses.ResponseOffset),
                    ct);

                if (dtcData != null)
                {
                    broadcastResp[rxId] = dtcData;
                    discoveredRxIds.Add(rxId);
                }
            }
        }

        var modules = new List<UdsModuleInfo>();

        foreach (var rxId in discoveredRxIds.OrderBy(id => id))
        {
            if (ct.IsCancellationRequested) break;
            ushort txId = (ushort)(rxId - Obd2Addresses.EcuPhysicalOffset);

            // Parse initial DTCs from broadcast response if available
            IReadOnlyList<UdsDtc> dtcs = [];
            if (broadcastResp.TryGetValue(rxId, out var dtcPayload))
            {
                dtcs = UdsProtocol.ParseDtcResponse(dtcPayload, _descriptions);
            }
            else
            {
                dtcs = await ReadModuleDtcsAsync(txId, rxId, ct);
            }

            // Probe standard identification DIDs
            var vinDid = await ReadDidAsync(txId, rxId, 0xF190, ct);
            var partDid = await ReadDidAsync(txId, rxId, 0xF187, ct);
            var swDid = await ReadDidAsync(txId, rxId, 0xF189, ct);
            var hwDid = await ReadDidAsync(txId, rxId, 0xF191, ct);
            var sysDid = await ReadDidAsync(txId, rxId, 0xF197, ct);

            string name = GetDefaultModuleName(rxId, sysDid?.DisplayValue);

            modules.Add(new UdsModuleInfo(
                txId,
                rxId,
                name,
                sysDid?.DisplayValue,
                partDid?.DisplayValue,
                swDid?.DisplayValue,
                hwDid?.DisplayValue,
                vinDid?.DisplayValue,
                dtcs));
        }

        Resolver.Log?.Info($"UdsScanner.DiscoverModulesAsync: Found {modules.Count} module(s).");
        return modules;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UdsDtc>> ReadModuleDtcsAsync(ushort txId, ushort rxId, CancellationToken ct = default)
    {
        var response = await SendAndReceivePhysical(
            txId, rxId,
            [(byte)UdsService.ReadDtcInformation, (byte)UdsDtcSubFunction.ReportDtcByStatusMask, 0xFF],
            (byte)((byte)UdsService.ReadDtcInformation + Obd2Addresses.ResponseOffset),
            ct);

        return response != null ? UdsProtocol.ParseDtcResponse(response, _descriptions) : [];
    }

    /// <inheritdoc/>
    public async Task<bool> ClearModuleDtcsAsync(ushort txId, ushort rxId, CancellationToken ct = default)
    {
        var response = await SendAndReceivePhysical(
            txId, rxId,
            [(byte)UdsService.ClearDiagnosticInformation, 0xFF, 0xFF, 0xFF],
            (byte)((byte)UdsService.ClearDiagnosticInformation + Obd2Addresses.ResponseOffset),
            ct);

        return response != null && response.Length > 0 &&
               response[0] == (byte)((byte)UdsService.ClearDiagnosticInformation + Obd2Addresses.ResponseOffset);
    }

    /// <inheritdoc/>
    public async Task<bool> ClearAllDtcsAsync(CancellationToken ct = default)
    {
        SendRequest((ushort)Obd2Addresses.FunctionalRequest, [(byte)UdsService.ClearDiagnosticInformation, 0xFF, 0xFF, 0xFF]);
        await Task.Delay(500, ct);
        return true;
    }

    /// <inheritdoc/>
    public async Task<UdsDidValue?> ReadDidAsync(ushort txId, ushort rxId, ushort did, CancellationToken ct = default)
    {
        byte didHi = (byte)(did >> 8);
        byte didLo = (byte)(did & 0xFF);

        var response = await SendAndReceivePhysical(
            txId, rxId,
            [(byte)UdsService.ReadDataByIdentifier, didHi, didLo],
            (byte)((byte)UdsService.ReadDataByIdentifier + Obd2Addresses.ResponseOffset),
            ct);

        return response != null ? UdsProtocol.ParseDidResponse(did, response, _descriptions) : null;
    }

    /// <inheritdoc/>
    public async Task<bool> SetDiagnosticSessionAsync(ushort txId, ushort rxId, UdsSessionType session, CancellationToken ct = default)
    {
        var response = await SendAndReceivePhysical(
            txId, rxId,
            [(byte)UdsService.DiagnosticSessionControl, (byte)session],
            (byte)((byte)UdsService.DiagnosticSessionControl + Obd2Addresses.ResponseOffset),
            ct);

        return response != null && response.Length > 0 &&
               response[0] == (byte)((byte)UdsService.DiagnosticSessionControl + Obd2Addresses.ResponseOffset);
    }

    /// <inheritdoc/>
    public Task SendTesterPresentAsync(ushort txId, bool suppressResponse = true, CancellationToken ct = default)
    {
        byte subFunc = (byte)(suppressResponse ? 0x80 : 0x00);
        SendRequest(txId, [(byte)UdsService.TesterPresent, subFunc]);
        return Task.CompletedTask;
    }

    private static string GetDefaultModuleName(ushort rxId, string? sysName)
    {
        if (!string.IsNullOrWhiteSpace(sysName)) return sysName;
        return rxId switch
        {
            0x7E8 => "PCM (Powertrain)",
            0x7E9 => "TCU (Transmission)",
            0x7EA => "BCM (Body)",
            0x7EB => "HVAC (Climate)",
            0x7EC => "ABS (Brakes)",
            0x7ED => "SRS (Airbag)",
            0x7EE => "IC (Instrument Cluster)",
            0x7EF => "GW (Gateway)",
            _ => $"ECU (0x{rxId:X3})"
        };
    }

    private void SendRequest(ushort targetId, byte[] payloadData)
    {
        var payload = new byte[8];
        payload[0] = (byte)payloadData.Length;
        Array.Copy(payloadData, 0, payload, 1, Math.Min(7, payloadData.Length));
        Resolver.Log?.Info($"UDS TX 0x{targetId:X3}: [{string.Join(" ", payload.Select(b => $"{b:X2}"))}]");
        _bus.WriteFrame(new StandardDataFrame { ID = (short)targetId, Payload = payload });
    }

    private void SendFlowControl(ushort targetId)
    {
        byte fcByte = (byte)((byte)IsoTpFrameType.FlowControl << 4);
        var payload = new byte[] { fcByte, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        _bus.WriteFrame(new StandardDataFrame { ID = (short)targetId, Payload = payload });
    }

    private async Task<byte[]?> SendAndReceivePhysical(
        ushort txId, ushort rxId, byte[] payloadData, byte expectedResponseService, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(PhysicalTimeout);

        var tcs = new TaskCompletionSource<byte[]?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var assembler = new MultiFrameAssembler();

        EventHandler<ICanFrame>? handler = null;
        handler = (_, frame) =>
        {
            if (frame is not StandardDataFrame sdf) return;
            if (sdf.ID != (short)rxId) return;

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
                {
                    _bus.FrameReceived -= handler;
                    tcs.TrySetResult(data);
                }
                else if (data.Length > 0 && data[0] == (byte)UdsService.NegativeResponse)
                {
                    _bus.FrameReceived -= handler;
                    Resolver.Log?.Warn($"UDS RX 0x{rxId:X3}: NRC response for service 0x{expectedResponseService:X2}");
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
                SendFlowControl(txId);
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
                    if (data.Length > 0 && data[0] == expectedResponseService)
                    {
                        tcs.TrySetResult(data);
                    }
                    else
                    {
                        tcs.TrySetResult(null);
                    }
                }
            }
        };

        cts.Token.Register(() =>
        {
            _bus.FrameReceived -= handler;
            tcs.TrySetResult(null);
        });

        _bus.FrameReceived += handler;
        SendRequest(txId, payloadData);

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
                SendFlowControl((ushort)(sdf.ID - Obd2Addresses.EcuPhysicalOffset));
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

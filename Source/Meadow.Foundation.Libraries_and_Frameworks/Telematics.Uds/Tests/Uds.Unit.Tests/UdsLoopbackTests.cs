using Meadow.Foundation.Telematics.Uds;

namespace Uds.Unit.Tests;

/// <summary>
/// Client and server on one bus. These are the tests that prove both halves of ISO-TP agree —
/// framing, flow control and addressing — without any hardware.
/// </summary>
public class UdsLoopbackTests
{
    private static (LoopbackCanBus bus, UdsScanner scanner, List<UdsServer> servers, List<FakeUdsDataSource> sources)
        BuildBus(params ushort[] responseIds)
    {
        var bus = new LoopbackCanBus();
        var servers = new List<UdsServer>();
        var sources = new List<FakeUdsDataSource>();

        foreach (var id in responseIds)
        {
            var source = new FakeUdsDataSource();
            source.WithText(0xF197, $"ECU {id:X3}");
            sources.Add(source);
            servers.Add(new UdsServer([bus], (short)id, source));
        }

        return (bus, new UdsScanner(bus), servers, sources);
    }

    [Fact]
    public async Task DiscoverModules_FindsEveryServerOnTheBus()
    {
        var (_, scanner, servers, sources) = BuildBus(0x7E8, 0x7E9, 0x7EA);
        using var d1 = servers[0];
        using var d2 = servers[1];
        using var d3 = servers[2];

        foreach (var source in sources)
        {
            source.Dtcs.Add(new UdsDtcRecord(0x01, 0x00, 0x00, UdsDtcStatusMask.ConfirmedDtc));
        }

        var modules = await scanner.DiscoverModulesAsync();

        Assert.Equal(3, modules.Count);
        Assert.Equal([0x7E8, 0x7E9, 0x7EA], modules.Select(m => m.RxId).ToArray());
        Assert.Equal([0x7E0, 0x7E1, 0x7E2], modules.Select(m => m.TxId).ToArray());
        Assert.Equal("ECU 7E8", modules[0].Name);
        Assert.All(modules, m => Assert.Single(m.Dtcs));
    }

    [Fact]
    public async Task ReadDid_RoundTripsThroughIsoTp()
    {
        var (_, scanner, servers, sources) = BuildBus(0x7E8);
        using var d = servers[0];

        sources[0].WithText(0xF190, "1HGCR2F83HA000000");

        var value = await scanner.ReadDidAsync(0x7E0, 0x7E8, 0xF190);

        Assert.NotNull(value);
        // 17 ASCII bytes plus the 3-byte header does not fit one CAN frame, so this is the
        // multi-frame path with the client's flow control driving the server.
        Assert.Equal("1HGCR2F83HA000000", System.Text.Encoding.ASCII.GetString(value.RawBytes));
    }

    [Fact]
    public async Task ReadDid_UnknownIdentifier_ComesBackNull()
    {
        var (_, scanner, servers, _) = BuildBus(0x7E8);
        using var d = servers[0];

        Assert.Null(await scanner.ReadDidAsync(0x7E0, 0x7E8, 0x1234));
    }

    [Fact]
    public async Task ReadModuleDtcs_ReturnsEveryStoredFault()
    {
        var (_, scanner, servers, sources) = BuildBus(0x7E8);
        using var d = servers[0];

        sources[0].Dtcs.Add(new UdsDtcRecord(0x01, 0x00, 0x11, UdsDtcStatusMask.ConfirmedDtc | UdsDtcStatusMask.TestFailed));
        sources[0].Dtcs.Add(new UdsDtcRecord(0xC1, 0x00, 0x87, UdsDtcStatusMask.PendingDtc));

        var dtcs = await scanner.ReadModuleDtcsAsync(0x7E0, 0x7E8);

        Assert.Equal(2, dtcs.Count);
        Assert.Equal("P0100-11", dtcs[0].FullCode);
        Assert.True(dtcs[0].IsConfirmed);
        Assert.Equal("U0100-87", dtcs[1].FullCode);
        Assert.True(dtcs[1].IsPending);
    }

    [Fact]
    public async Task ClearModuleDtcs_EmptiesTheModule()
    {
        var (_, scanner, servers, sources) = BuildBus(0x7E8);
        using var d = servers[0];

        sources[0].Dtcs.Add(new UdsDtcRecord(0x01, 0x00, 0x00, UdsDtcStatusMask.ConfirmedDtc));

        Assert.True(await scanner.ClearModuleDtcsAsync(0x7E0, 0x7E8));
        Assert.Empty(await scanner.ReadModuleDtcsAsync(0x7E0, 0x7E8));
    }

    [Fact]
    public async Task ClearAllDtcs_OverFunctionalAddress_ReachesEveryModule()
    {
        var (_, scanner, servers, sources) = BuildBus(0x7E8, 0x7E9);
        using var d1 = servers[0];
        using var d2 = servers[1];

        foreach (var source in sources)
        {
            source.Dtcs.Add(new UdsDtcRecord(0x01, 0x00, 0x00, UdsDtcStatusMask.ConfirmedDtc));
        }

        await scanner.ClearAllDtcsAsync();

        Assert.All(sources, s => Assert.Empty(s.Dtcs));
    }

    [Fact]
    public async Task SetDiagnosticSession_IsAcceptedAndRemembered()
    {
        var (_, scanner, servers, _) = BuildBus(0x7E8);
        using var d = servers[0];

        Assert.True(await scanner.SetDiagnosticSessionAsync(0x7E0, 0x7E8, UdsSessionType.ExtendedDiagnosticSession));
        Assert.Equal(UdsSessionType.ExtendedDiagnosticSession, servers[0].Session.Current);
    }

    [Fact]
    public async Task TesterPresent_KeepsTheSessionAlive()
    {
        var (_, scanner, servers, _) = BuildBus(0x7E8);
        using var d = servers[0];

        servers[0].Session.Timeout = TimeSpan.FromMilliseconds(120);
        await scanner.SetDiagnosticSessionAsync(0x7E0, 0x7E8, UdsSessionType.ExtendedDiagnosticSession);

        for (int i = 0; i < 4; i++)
        {
            await Task.Delay(40);
            await scanner.SendTesterPresentAsync(0x7E0);
        }

        Assert.Equal(UdsSessionType.ExtendedDiagnosticSession, servers[0].Session.Current);
    }
}

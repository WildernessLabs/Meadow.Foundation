using Meadow.Foundation.Telematics.Uds;
using System.Text;

namespace Uds.Unit.Tests;

public class UdsServerTests
{
    private const short Pcm = 0x7E8;
    private const short PcmRequest = 0x7E0;
    private const short Functional = 0x7DF;

    private static (FakeCanBus bus, FakeUdsDataSource source, UdsServer server) BuildPcm()
    {
        var bus = new FakeCanBus();
        var source = new FakeUdsDataSource();
        var server = new UdsServer([bus], Pcm, source);
        return (bus, source, server);
    }

    private static byte[] SingleFrameData(Meadow.Hardware.StandardDataFrame frame)
    {
        int length = frame.Payload[0] & 0x0F;
        return frame.Payload.Skip(1).Take(length).ToArray();
    }

    [Fact]
    public async Task ReadDtcByStatusMask_SingleDtc_AnswersWithOneFrame()
    {
        var (bus, source, server) = BuildPcm();
        using var _ = server;

        // P0100-11, confirmed and currently failing
        source.Dtcs.Add(new UdsDtcRecord(0x01, 0x00, 0x11, UdsDtcStatusMask.ConfirmedDtc | UdsDtcStatusMask.TestFailed));

        bus.InjectRequest(PcmRequest, 0x19, 0x02, 0xFF);
        var frames = await bus.WaitForFrames(1);

        var response = SingleFrameData(Assert.Single(frames));
        Assert.Equal([0x59, 0x02, (byte)source.AvailabilityMask, 0x01, 0x00, 0x11, 0x09], response);
        Assert.Equal(Pcm, frames[0].ID);
    }

    [Fact]
    public async Task ReadDtcByStatusMask_ManyDtcs_UsesMultiFrameAndWaitsForFlowControl()
    {
        var (bus, source, server) = BuildPcm();
        using var _ = server;

        for (byte i = 1; i <= 4; i++)
        {
            source.Dtcs.Add(new UdsDtcRecord(0x00, i, 0x00, UdsDtcStatusMask.ConfirmedDtc));
        }

        bus.InjectRequest(PcmRequest, 0x19, 0x02, 0xFF);

        // Only the first frame goes out until the tester grants flow control.
        var first = await bus.WaitForFrames(1);
        Assert.Single(first);
        Assert.Equal(0x10, first[0].Payload[0] & 0xF0);
        Assert.Equal(3 + 4 * 4, ((first[0].Payload[0] & 0x0F) << 8) | first[0].Payload[1]);

        bus.InjectFrame(new Meadow.Hardware.StandardDataFrame
        {
            ID = PcmRequest,
            Payload = [0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]
        });

        var all = await bus.WaitForFrames(4);
        Assert.True(all.Count >= 3, $"expected consecutive frames, got {all.Count}");
        Assert.All(all.Skip(1), f => Assert.Equal(0x20, f.Payload[0] & 0xF0));
    }

    [Fact]
    public async Task ReadDtcByStatusMask_FiltersOnRequestedStatusBits()
    {
        var (bus, source, server) = BuildPcm();
        using var _ = server;

        source.Dtcs.Add(new UdsDtcRecord(0x01, 0x00, 0x00, UdsDtcStatusMask.ConfirmedDtc));
        source.Dtcs.Add(new UdsDtcRecord(0x02, 0x00, 0x00, UdsDtcStatusMask.PendingDtc));

        // Ask for pending only.
        bus.InjectRequest(PcmRequest, 0x19, 0x02, (byte)UdsDtcStatusMask.PendingDtc);
        var frames = await bus.WaitForFrames(1);

        var response = SingleFrameData(frames[0]);
        Assert.Equal(7, response.Length);
        Assert.Equal(0x02, response[3]); // only the pending record survived
    }

    [Fact]
    public async Task ReadDataByIdentifier_KnownDid_ReturnsValue()
    {
        var (bus, source, server) = BuildPcm();
        using var _ = server;

        source.WithText(0xF197, "PCM");

        bus.InjectRequest(PcmRequest, 0x22, 0xF1, 0x97);
        var frames = await bus.WaitForFrames(1);

        var response = SingleFrameData(frames[0]);
        Assert.Equal(0x62, response[0]);
        Assert.Equal(0xF1, response[1]);
        Assert.Equal(0x97, response[2]);
        Assert.Equal("PCM", Encoding.ASCII.GetString(response, 3, response.Length - 3));
    }

    [Fact]
    public async Task ReadDataByIdentifier_UnknownDid_ReturnsRequestOutOfRange()
    {
        var (bus, _, server) = BuildPcm();
        using var _s = server;

        bus.InjectRequest(PcmRequest, 0x22, 0x12, 0x34);
        var frames = await bus.WaitForFrames(1);

        Assert.Equal([0x7F, 0x22, (byte)UdsNrc.RequestOutOfRange], SingleFrameData(frames[0]));
    }

    [Fact]
    public async Task UnsupportedService_ReturnsServiceNotSupported()
    {
        var (bus, _, server) = BuildPcm();
        using var _s = server;

        bus.InjectRequest(PcmRequest, 0x27, 0x01); // SecurityAccess — deliberately not implemented
        var frames = await bus.WaitForFrames(1);

        Assert.Equal([0x7F, 0x27, (byte)UdsNrc.ServiceNotSupported], SingleFrameData(frames[0]));
    }

    [Fact]
    public async Task FunctionalRequest_ThatWouldBeRejected_StaysSilent()
    {
        var (bus, _, server) = BuildPcm();
        using var _s = server;

        // A tester sweeping 0x7DF must not collect refusals as if they were modules.
        bus.InjectRequest(Functional, 0x22, 0x12, 0x34);
        Assert.Empty(await bus.Settle());
    }

    [Fact]
    public async Task FunctionalRequest_ThatSucceeds_IsAnswered()
    {
        var (bus, source, server) = BuildPcm();
        using var _ = server;

        source.Dtcs.Add(new UdsDtcRecord(0x01, 0x00, 0x00, UdsDtcStatusMask.ConfirmedDtc));

        bus.InjectRequest(Functional, 0x19, 0x02, 0xFF);
        var frames = await bus.WaitForFrames(1);

        Assert.Equal(Pcm, frames[0].ID);
        Assert.Equal(0x59, SingleFrameData(frames[0])[0]);
    }

    [Fact]
    public async Task ClearDiagnosticInformation_ClearsAndAcknowledges()
    {
        var (bus, source, server) = BuildPcm();
        using var _ = server;

        source.Dtcs.Add(new UdsDtcRecord(0x01, 0x00, 0x00, UdsDtcStatusMask.ConfirmedDtc));

        bus.InjectRequest(PcmRequest, 0x14, 0xFF, 0xFF, 0xFF);
        var frames = await bus.WaitForFrames(1);

        Assert.Equal([0x54], SingleFrameData(frames[0]));
        Assert.Equal(1, source.ClearCount);
        Assert.Empty(source.Dtcs);
    }

    [Fact]
    public async Task TesterPresent_WithSuppressBit_SendsNothing()
    {
        var (bus, _, server) = BuildPcm();
        using var _s = server;

        bus.InjectRequest(PcmRequest, 0x3E, 0x80);
        Assert.Empty(await bus.Settle());
    }

    [Fact]
    public async Task TesterPresent_WithoutSuppressBit_IsAcknowledged()
    {
        var (bus, _, server) = BuildPcm();
        using var _s = server;

        bus.InjectRequest(PcmRequest, 0x3E, 0x00);
        var frames = await bus.WaitForFrames(1);

        Assert.Equal([0x7E, 0x00], SingleFrameData(frames[0]));
    }

    [Fact]
    public async Task DiagnosticSessionControl_SwitchesSessionAndReportsTiming()
    {
        var (bus, _, server) = BuildPcm();
        using var _s = server;

        bus.InjectRequest(PcmRequest, 0x10, 0x03);
        var frames = await bus.WaitForFrames(1);

        Assert.Equal([0x50, 0x03, 0x00, 0x32, 0x01, 0xF4], SingleFrameData(frames[0]));
        Assert.Equal(UdsSessionType.ExtendedDiagnosticSession, server.Session.Current);
    }

    [Fact]
    public async Task DiagnosticSessionControl_UnknownSession_ReturnsSubFunctionNotSupported()
    {
        var (bus, _, server) = BuildPcm();
        using var _s = server;

        bus.InjectRequest(PcmRequest, 0x10, 0x77);
        var frames = await bus.WaitForFrames(1);

        Assert.Equal([0x7F, 0x10, (byte)UdsNrc.SubFunctionNotSupported], SingleFrameData(frames[0]));
    }

    [Fact]
    public async Task RequestForAnotherModulesAddress_IsIgnored()
    {
        var (bus, _, server) = BuildPcm();
        using var _s = server;

        bus.InjectRequest(0x7E1, 0x19, 0x02, 0xFF); // the TCU's address
        Assert.Empty(await bus.Settle());
    }

    [Fact]
    public async Task DisposedServer_StopsAnswering()
    {
        var (bus, source, server) = BuildPcm();
        source.Dtcs.Add(new UdsDtcRecord(0x01, 0x00, 0x00, UdsDtcStatusMask.ConfirmedDtc));

        server.Dispose();

        bus.InjectRequest(PcmRequest, 0x19, 0x02, 0xFF);
        Assert.Empty(await bus.Settle());
    }

    [Fact]
    public void Session_LapsesBackToDefaultAfterTimeout()
    {
        var session = new UdsSessionState { Timeout = TimeSpan.FromMilliseconds(1) };
        session.Set(UdsSessionType.ProgrammingSession);

        Thread.Sleep(20);

        Assert.Equal(UdsSessionType.DefaultSession, session.Current);
    }
}

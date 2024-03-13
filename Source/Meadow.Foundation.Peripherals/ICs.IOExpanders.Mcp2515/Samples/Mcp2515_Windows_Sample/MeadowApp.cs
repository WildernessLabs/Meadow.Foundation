using Meadow;
using Meadow.Foundation.ICs.IOExpanders;

public class MeadowApp : App<Windows>
{
    private Mcp2515 _mcp;

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }

    public override Task Initialize()
    {
        Console.WriteLine("Creating SPI Bus");

        var expander = FtdiExpanderCollection.Devices[0];

        var bus = expander.CreateSpiBus();
        _mcp = new Mcp2515(bus, expander.Pins.C0.CreateDigitalOutputPort(), CanBitRate.BitRate_250KHz);

        return base.Initialize();
    }

    public override async Task Run()
    {
        while (true)
        {
            var frame = _mcp.Read();
            if (frame != null)
            {
                Resolver.Log.Info($"ID: {frame.CanID:X8} [{BitConverter.ToString(frame.Data)}]");
            }
            Thread.Sleep(1000);
        }
    }
}

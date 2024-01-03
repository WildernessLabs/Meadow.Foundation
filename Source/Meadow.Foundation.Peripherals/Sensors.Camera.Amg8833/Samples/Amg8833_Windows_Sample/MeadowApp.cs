using Meadow;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Camera;

public class MeadowApp : App<Windows>
{
    private Cp2112 _expander;

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }

    public override Task Initialize()
    {
        Console.WriteLine("Creating Outputs");

        _expander = Cp2112Collection.Devices.First();
        var bus = _expander.CreateI2cBus();
        var camera = new Amg8833(bus);

        return base.Initialize();
    }
}
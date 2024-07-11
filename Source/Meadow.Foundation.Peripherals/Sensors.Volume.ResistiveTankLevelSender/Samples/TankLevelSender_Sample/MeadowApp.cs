using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Volume;
using Meadow.Units;
using System.Threading.Tasks;

namespace Sensors.Volume.ResistiveTankLevelSender_Sample;

public class MeadowApp : App<F7FeatherV2>
{
    //<!=SNIP=>

    private ResistiveTankLevelSender sender;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        sender = new ResistiveTankLevelSender_12in_33_240(Device.Pins.A00, 4.66.Volts());
        sender.FillLevelChanged += (s, e) => Resolver.Log.Info($"Tank level: {e}%");
        sender.StartUpdating();

        return Task.CompletedTask;
    }

    //<!=SNOP=>
}
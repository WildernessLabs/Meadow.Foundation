using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Moisture;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using VU = Meadow.Units.Voltage.UnitType;

namespace Sensors.Moisture.FC28_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Fc28 fc28;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            fc28 = new Fc28(
                Device.CreateAnalogInputPort(Device.Pins.A01, 5, TimeSpan.FromMilliseconds(40), new Voltage(3.3, Voltage.UnitType.Volts)),
                Device.CreateDigitalOutputPort(Device.Pins.D15),
                minimumVoltageCalibration: new Voltage(3.24f, VU.Volts),
                maximumVoltageCalibration: new Voltage(2.25f, VU.Volts)
            );

            var consumer = Fc28.CreateObserver(
                handler: result => {
                    // the first time through, old will be null.
                    string oldValue = (result.Old is { } old) ? $"{old:n2}" : "n/a"; 
                    Resolver.Log.Info($"Subscribed - " +
                        $"new: {result.New}, " +
                        $"old: {oldValue}");
                },
                filter: null
            );
            fc28.Subscribe(consumer);

            fc28.HumidityUpdated += (object sender, IChangeResult<double> e) =>
            {
                Resolver.Log.Info($"Moisture Updated: {e.New}");
            };

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            var moisture = await fc28.Read();
            Resolver.Log.Info($"Moisture Value { moisture}");

            fc28.StartUpdating(TimeSpan.FromMilliseconds(5000));
        }

        //<!=SNOP=>
    }
}
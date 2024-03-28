using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Lis3mdl_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Lis3mdl sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize hardware...");
            sensor = new Lis3mdl(Device.CreateI2cBus());

            // classical .NET events can also be used:
            sensor.Updated += HandleResult;

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Lis3mdl.CreateObserver(handler: result => HandleResult(this, result),
                                                 filter: result => FilterResult(result));

            sensor.Subscribe(consumer);

            sensor.StartUpdating(TimeSpan.FromSeconds(2));

            return base.Initialize();
        }

        bool FilterResult(IChangeResult<MagneticField3D> result)
        {
            return result.New.Z > new MagneticField(0.1, MagneticField.UnitType.Gauss);
        }

        void HandleResult(object sender, IChangeResult<MagneticField3D> result)
        {
            var mag = result.New;

            Resolver.Log.Info($"Magnetometer (gauss): X = {mag.X.Gauss:0.##}, Y = {mag.Y.Gauss:0.##}, Z = {mag.Z.Gauss:0.##}");
        }

        //<!=SNOP=>
    }
}
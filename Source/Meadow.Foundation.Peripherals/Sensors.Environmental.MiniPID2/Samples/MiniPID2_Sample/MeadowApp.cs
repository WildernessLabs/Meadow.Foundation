using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using System;
using System.Threading.Tasks;

namespace Sensors.Environmental.Ens160_Sample
{
    public class MeadowApp : App<F7FeatherV1>
    {
        //<!=SNIP=>

        MiniPID2 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            sensor = new MiniPID2(Device.Pins.A01, MiniPID2.MiniPID2Type.PPB_WR);

            var consumer = MiniPID2.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: VOC concentration changed by threshold; new: {result.New.PartsPerBillion:N1}ppm");
                },
                filter: result =>
                {
                    if (result.Old is { } oldCon &&
                        result.New is { } newCon)
                    {
                        return Math.Abs((newCon - oldCon).PartsPerMillion) > 10;
                    }
                    return false;
                }
            );

            sensor?.Subscribe(consumer);

            if (sensor != null)
            {
                sensor.Updated += (sender, result) =>
                {
                    Resolver.Log.Info($"  VOC Concentraion: {result.New.PartsPerMillion:N1}ppm");
                };
            }

            sensor?.StartUpdating(TimeSpan.FromSeconds(2));

            return base.Initialize();
        }

        //<!=SNOP=>
    }
}
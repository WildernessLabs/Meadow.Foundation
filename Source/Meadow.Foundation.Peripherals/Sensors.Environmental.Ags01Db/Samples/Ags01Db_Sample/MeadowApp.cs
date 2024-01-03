using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sensors.Environmental.Ags01Db_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ags01Db ags10Db;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize ...");
            ags10Db = new Ags01Db(Device.CreateI2cBus());

            Resolver.Log.Info($"Version: v{ags10Db.GetVersion()}");

            var consumer = Ags01Db.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Concentration New Value {result.New.PartsPerMillion}ppm");
                    Resolver.Log.Info($"Concentration Old Value {result.Old?.PartsPerMillion}ppm");
                },
                filter: null
            );
            ags10Db.Subscribe(consumer);

            ags10Db.Updated += (object sender, IChangeResult<Meadow.Units.Concentration> e) =>
            {
                Resolver.Log.Info($"Concentration Updated: {e.New.PartsPerMillion:N2}ppm");
            };

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            ags10Db.StartUpdating(TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        //<!=SNOP=>

        void TestRead()
        {
            Resolver.Log.Info("TestAgs10DbSensor...");

            while (true)
            {
                var temp = ags10Db.Read().Result;

                Resolver.Log.Info($"Concentration New Value {temp.PartsPerMillion}ppm");
                Thread.Sleep(1000);
            }
        }
    }
}
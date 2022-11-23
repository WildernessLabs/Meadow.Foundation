using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;

namespace Sensors.Environmental.Ags01Db_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ags01Db ags10Db;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize ...");
            ags10Db = new Ags01Db(Device.CreateI2cBus());

            Console.WriteLine($"Version: v{ags10Db.GetVersion()}");

            var consumer = Ags01Db.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Concentration New Value {result.New.PartsPerMillion}ppm");
                    Console.WriteLine($"Concentration Old Value {result.Old?.PartsPerMillion}ppm");
                },
                filter: null
            );
            ags10Db.Subscribe(consumer);

            ags10Db.ConcentrationUpdated += (object sender, IChangeResult<Meadow.Units.Concentration> e) =>
            {
                Console.WriteLine($"Concentration Updated: {e.New.PartsPerMillion:N2}ppm");
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
            Console.WriteLine("TestAgs10DbSensor...");

            while (true)
            {
                var temp = ags10Db.Read().Result;

                Console.WriteLine($"Concentration New Value { temp.PartsPerMillion}ppm");
                Thread.Sleep(1000);
            }
        }
    }
}
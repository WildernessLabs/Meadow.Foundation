using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Ags01Db ags10Db;

        public MeadowApp()
        {
            Console.WriteLine("Initialize ...");
            ags10Db = new Ags01Db(Device.CreateI2cBus());

            Console.WriteLine($"Version: v{ags10Db.GetVersion()}");

            TestUpdating();
            //TestRead();
        }

        void TestUpdating()
        {
            Console.WriteLine("Test Updating...");

            var consumer = Ags01Db.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Concentration New Value { result.New.PartsPerMillion}ppm");
                    Console.WriteLine($"Concentration Old Value { result.Old?.PartsPerMillion}ppm");
                },
                filter: null
            );
            ags10Db.Subscribe(consumer);

            ags10Db.ConcentrationUpdated += (object sender, IChangeResult<Meadow.Units.Concentration> e) =>
            {
                Console.WriteLine($"Concentration Updated: {e.New.PartsPerMillion:N2}ppm");
            };

            ags10Db.StartUpdating(TimeSpan.FromSeconds(1));
        }

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
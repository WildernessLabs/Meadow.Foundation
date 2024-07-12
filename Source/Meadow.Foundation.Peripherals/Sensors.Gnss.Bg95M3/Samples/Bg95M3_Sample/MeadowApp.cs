using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Gnss;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Threading.Tasks;

namespace Sensors.Gnss.Bg95M3_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Bg95M3 bg95M3;

        void ProcessGnssPosition(object sender, IGnssResult location)
        {
            Resolver.Log.Info($"{location}");
        }

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing ...");

            var cellAdapter = Device.NetworkAdapters.Primary<ICellNetworkAdapter>();

            IGnssResult[] resultTypes = new IGnssResult[]
            {
                new GnssPositionInfo(),
                new ActiveSatellites(),
                new CourseOverGround(),
                new SatellitesInView(new Satellite[0])
            };

            bg95M3 = new Bg95M3(cellAdapter, TimeSpan.FromMinutes(30), resultTypes);

            bg95M3.GnssDataReceived += ProcessGnssPosition;

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            bg95M3.StartUpdating();

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}
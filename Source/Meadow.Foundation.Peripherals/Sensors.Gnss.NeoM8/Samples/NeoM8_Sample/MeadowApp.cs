using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Gnss;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Sensors.Gnss.NeoM8_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        NeoM8 gps;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing ...");

            gps = new NeoM8(Device, Device.SerialPortNames.Com4, Device.Pins.D09, Device.Pins.D11);

            gps.GgaReceived += (object sender, GnssPositionInfo location) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info(location);
                Resolver.Log.Info("*********************************************");
            };
            // GLL
            gps.GllReceived += (object sender, GnssPositionInfo location) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info(location);
                Resolver.Log.Info("*********************************************");
            };
            // GSA
            gps.GsaReceived += (object sender, ActiveSatellites activeSatellites) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info(activeSatellites);
                Resolver.Log.Info("*********************************************");
            };
            // RMC (recommended minimum)
            gps.RmcReceived += (object sender, GnssPositionInfo positionCourseAndTime) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info(positionCourseAndTime);
                Resolver.Log.Info("*********************************************");

            };
            // VTG (course made good)
            gps.VtgReceived += (object sender, CourseOverGround courseAndVelocity) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{courseAndVelocity}");
                Resolver.Log.Info("*********************************************");
            };
            // GSV (satellites in view)
            gps.GsvReceived += (object sender, SatellitesInView satellites) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{satellites}");
                Resolver.Log.Info("*********************************************");
            };

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            gps.StartUpdating();

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}
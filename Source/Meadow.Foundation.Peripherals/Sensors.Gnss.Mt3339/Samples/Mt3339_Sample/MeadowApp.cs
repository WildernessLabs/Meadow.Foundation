using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Gnss;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;
using System.Threading.Tasks;

namespace Sensors.Gnss.Mt3339_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mt3339 gps;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initializing ...");

            gps = new Mt3339(Device, Device.PlatformOS.GetSerialPortName("COM4"));

            gps.GgaReceived += (object sender, GnssPositionInfo location) => {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info(location.ToString());
                Resolver.Log.Info("*********************************************");
            };
            // GLL
            gps.GllReceived += (object sender, GnssPositionInfo location) => {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info(location.ToString());
                Resolver.Log.Info("*********************************************");
            };
            // GSA
            gps.GsaReceived += (object sender, ActiveSatellites activeSatellites) => {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info(activeSatellites.ToString());
                Resolver.Log.Info("*********************************************");
            };
            // RMC (recommended minimum)
            gps.RmcReceived += (object sender, GnssPositionInfo positionCourseAndTime) => {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info(positionCourseAndTime.ToString());
                Resolver.Log.Info("*********************************************");

            };
            // VTG (course made good)
            gps.VtgReceived += (object sender, CourseOverGround courseAndVelocity) => {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{courseAndVelocity}");
                Resolver.Log.Info("*********************************************");
            };
            // GSV (satellites in view)
            gps.GsvReceived += (object sender, SatellitesInView satellites) => {
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
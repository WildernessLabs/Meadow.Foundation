using Meadow;
using Meadow.Devices;
using Meadow.Foundation.mikroBUS.Sensors.Gnss;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System.Threading.Tasks;

namespace CGNSS5_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        CGNSS5 gps;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing ...");

            //gps = new CGNSS5(Device.PlatformOS.GetSerialPortName("COM1"), Device.Pins.D09, Device.Pins.D11);

            gps = new CGNSS5(Device.CreateI2cBus(), resetPin: Device.Pins.D09, ppsPin: Device.Pins.D11);

            gps.GgaReceived += (object sender, GnssPositionInfo location) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{location}");
                Resolver.Log.Info("*********************************************");
            };
            // GLL
            gps.GllReceived += (object sender, GnssPositionInfo location) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{location}");
                Resolver.Log.Info("*********************************************");
            };
            // GSA
            gps.GsaReceived += (object sender, ActiveSatellites activeSatellites) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{activeSatellites}");
                Resolver.Log.Info("*********************************************");
            };
            // RMC (recommended minimum)
            gps.RmcReceived += (object sender, GnssPositionInfo positionCourseAndTime) =>
            {
                Resolver.Log.Info("*********************************************");
                Resolver.Log.Info($"{positionCourseAndTime}");
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
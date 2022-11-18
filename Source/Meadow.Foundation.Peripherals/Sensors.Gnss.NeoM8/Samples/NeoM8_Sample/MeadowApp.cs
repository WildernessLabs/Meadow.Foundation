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
            Console.WriteLine("Initializing ...");

            gps = new NeoM8(Device, Device.SerialPortNames.Com4, Device.Pins.D09, Device.Pins.D11);

            gps.GgaReceived += (object sender, GnssPositionInfo location) =>
            {
                Console.WriteLine("*********************************************");
                Console.WriteLine(location);
                Console.WriteLine("*********************************************");
            };
            // GLL
            gps.GllReceived += (object sender, GnssPositionInfo location) =>
            {
                Console.WriteLine("*********************************************");
                Console.WriteLine(location);
                Console.WriteLine("*********************************************");
            };
            // GSA
            gps.GsaReceived += (object sender, ActiveSatellites activeSatellites) =>
            {
                Console.WriteLine("*********************************************");
                Console.WriteLine(activeSatellites);
                Console.WriteLine("*********************************************");
            };
            // RMC (recommended minimum)
            gps.RmcReceived += (object sender, GnssPositionInfo positionCourseAndTime) =>
            {
                Console.WriteLine("*********************************************");
                Console.WriteLine(positionCourseAndTime);
                Console.WriteLine("*********************************************");

            };
            // VTG (course made good)
            gps.VtgReceived += (object sender, CourseOverGround courseAndVelocity) =>
            {
                Console.WriteLine("*********************************************");
                Console.WriteLine($"{courseAndVelocity}");
                Console.WriteLine("*********************************************");
            };
            // GSV (satellites in view)
            gps.GsvReceived += (object sender, SatellitesInView satellites) =>
            {
                Console.WriteLine("*********************************************");
                Console.WriteLine($"{satellites}");
                Console.WriteLine("*********************************************");
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
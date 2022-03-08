using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Gnss;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace Sensors.Gnss.Mt3339_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>
        Mt3339 gps;

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");

            gps = new Mt3339(Device, Device.SerialPortNames.Com4);

            Subscribe();

            gps.StartUpdating();
        }

        void Subscribe()
        {
            gps.GgaReceived += (object sender, GnssPositionInfo location) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine(location);
                Console.WriteLine("*********************************************");
            };
            // GLL
            gps.GllReceived += (object sender, GnssPositionInfo location) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine(location);
                Console.WriteLine("*********************************************");
            };
            // GSA
            gps.GsaReceived += (object sender, ActiveSatellites activeSatellites) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine(activeSatellites);
                Console.WriteLine("*********************************************");
            };
            // RMC (recommended minimum)
            gps.RmcReceived += (object sender, GnssPositionInfo positionCourseAndTime) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine(positionCourseAndTime);
                Console.WriteLine("*********************************************");

            };
            // VTG (course made good)
            gps.VtgReceived += (object sender, CourseOverGround courseAndVelocity) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine($"{courseAndVelocity}");
                Console.WriteLine("*********************************************");
            };
            // GSV (satellites in view)
            gps.GsvReceived += (object sender, SatellitesInView satellites) => {
                Console.WriteLine("*********************************************");
                Console.WriteLine($"{satellites}");
                Console.WriteLine("*********************************************");
            };
        }
        //<!—SNOP—>
    }
}
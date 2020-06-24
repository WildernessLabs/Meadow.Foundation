﻿using System;
using System.Text;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;
using Sensors.Location.MediaTek;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mt3339 gps;

        public MeadowApp()
        {
            Console.WriteLine("App Start");

            Initialize();

            Console.WriteLine("Post Init");

            gps.StartUpdataing();
        }

        void Initialize()
        {
            gps = new Mt3339(Device, Device.SerialPortNames.Com4);

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
    }
}
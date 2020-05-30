using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.Sensors.GPS;
using System.Text;

namespace BasicSensors.GPS.NMEA_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        ISerialPort port;
        byte[] data = new byte[512];

        public MeadowApp()
        {
            //COM4 - Pins D00 & D01 on the Meadow F7
           /* port = Device.CreateSerialPort(Device.SerialPortNames.Com4, 9600);
            Console.WriteLine("Serial port created");
            port.Open();
            Console.WriteLine("Serial port opened");*/

            //LoopBackTest();
            //SerialReadTest();
            //EventTest();
            TestNMEA();
        }

        //simple loopback test - bridge pins D01 & D01 for COM4 on the Meadow F7
        void LoopBackTest()
        {
            while (true)
            {
                Console.WriteLine("Writing data...");
                var written = port.Write(Encoding.ASCII.GetBytes("Hello Meadow!"));//max write is 255 bytes in B3.11

                Console.WriteLine($"Wrote {written} bytes");
                var read = port.Read(data, 0, data.Length);
                Console.WriteLine($"Read {read} bytes: {BitConverter.ToString(data, 0, read)}");

                Thread.Sleep(2000);
            }
        }
        void SerialReadTest()
        {
            string msg = string.Empty;

            while (true)
            {
                var len = port.Read(data, 0, data.Length);

             //   msg = Encoding.ASCII.GetString(data, 0, len);

             /*   for (var index = 0; index < len; index++)
                {
                    msg += (char)buffer[index];
                }*/

                Console.WriteLine($"Read {len} bytes: {BitConverter.ToString(data, 0, len)}");

             //   Console.WriteLine(msg);

                Thread.Sleep(100);
            }
        }

        void EventTest()
        {
            port = Device.CreateSerialPort(Device.SerialPortNames.Com4, 9600);
            port.Open();

            port.DataReceived += Sp_DataReceived;
        }

        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var len = port.Read(data, 0, data.Length);

            Console.WriteLine($"Read {len} bytes");
            Console.WriteLine($"Data: {BitConverter.ToString(data, 0, len)}");
       }

        void TestNMEA()
        { 
            var gps = new NMEA(Device, Device.SerialPortNames.Com4, 9600, Parity.None, 8, StopBits.One);
                
            var ggaDecoder = new GGADecoder();
            ggaDecoder.OnPositionReceived += GgaDecoder_OnPositionReceived;
            gps.AddDecoder(ggaDecoder);
            //
            var gllDecoder = new GLLDecoder();
            gllDecoder.OnGeographicLatitudeLongitudeReceived += GllDecoder_OnGeographicLatitudeLongitudeReceived;
            gps.AddDecoder(gllDecoder);
            //
            var gsaDecoder = new GSADecoder();
            gsaDecoder.OnActiveSatellitesReceived += GsaDecoder_OnActiveSatelitesReceived;
            gps.AddDecoder(gsaDecoder);
            //
            var rmcDecoder = new RMCDecoder();
            rmcDecoder.OnPositionCourseAndTimeReceived += RmcDecoder_OnPositionCourseAndTimeReceived;
            gps.AddDecoder(rmcDecoder);
            //
            var vtgDecoder = new VTGDecoder();
            vtgDecoder.OnCourseAndVelocityReceived += VtgDecoder_OnCourseAndVelocityReceived;
            gps.AddDecoder(vtgDecoder); 
            //
            gps.Open();
            Thread.Sleep(Timeout.Infinite); 
        }

        private static string DecodeDMPostion(DegreeMinutePosition dmp)
        {
            var position = dmp.Degrees.ToString("f2") + "d " + dmp.Minutes.ToString("f2") + "m ";
            switch (dmp.Direction)
            {
                case DirectionIndicator.East:
                    position += "E";
                    break;
                case DirectionIndicator.West:
                    position += "W";
                    break;
                case DirectionIndicator.North:
                    position += "N";
                    break;
                case DirectionIndicator.South:
                    position += "S";
                    break;
                case DirectionIndicator.Unknown:
                    position += "Unknown";
                    break;
            }
            return position;
        }

        private static void VtgDecoder_OnCourseAndVelocityReceived(object sender, CourseOverGround courseAndVelocity)
        {
            Console.WriteLine("Satellite information received.");
            Console.WriteLine("True heading: " + courseAndVelocity.TrueHeading.ToString("f2"));
            Console.WriteLine("Magnetic heading: " + courseAndVelocity.MagneticHeading.ToString("f2"));
            Console.WriteLine("Knots: " + courseAndVelocity.Knots.ToString("f2"));
            Console.WriteLine("KPH: " + courseAndVelocity.KPH.ToString("f2"));
            Console.WriteLine("*********************************************\n");
        }

        private static void RmcDecoder_OnPositionCourseAndTimeReceived(object sender,
            PositionCourseAndTime positionCourseAndTime)
        {
            Console.WriteLine("Satellite information received.");
            Console.WriteLine("Time of reading: " + positionCourseAndTime.TimeOfReading);
            Console.WriteLine("Latitude: " + DecodeDMPostion(positionCourseAndTime.Latitude));
            Console.WriteLine("Longitude: " + DecodeDMPostion(positionCourseAndTime.Longitude));
            Console.WriteLine("Speed: " + positionCourseAndTime.Speed.ToString("f2"));
            Console.WriteLine("Course: " + positionCourseAndTime.Course.ToString("f2"));
            Console.WriteLine("*********************************************\n");
        }

        private static void GsaDecoder_OnActiveSatelitesReceived(object sender, ActiveSatellites activeSatellites)
        {
            Console.WriteLine("Satellite information received.");
            Console.WriteLine("Number of satellites involved in fix: " + activeSatellites.SatellitesUsedForFix.Length);
            Console.WriteLine("Dilution of precision: " + activeSatellites.DilutionOfPrecision.ToString("f2"));
            Console.WriteLine("HDOP: " + activeSatellites.HorizontalDilutionOfPrecision.ToString("f2"));
            Console.WriteLine("VDOP: " + activeSatellites.VerticalDilutionOfPrecision.ToString("f2"));
            Console.WriteLine("*********************************************\n");
        }

        private static void GllDecoder_OnGeographicLatitudeLongitudeReceived(object sender, GPSLocation location)
        {
            Console.WriteLine("Location information received.");
            Console.WriteLine("Time of reading: " + location.ReadingTime);
            Console.WriteLine("Latitude: " + DecodeDMPostion(location.Latitude));
            Console.WriteLine("Longitude: " + DecodeDMPostion(location.Longitude));
            Console.WriteLine("*********************************************\n");
        }

        private static void GgaDecoder_OnPositionReceived(object sender, GPSLocation location)
        {
            Console.WriteLine("Location information received.");
            Console.WriteLine("Time of reading: " + location.ReadingTime);
            Console.WriteLine("Latitude: " + DecodeDMPostion(location.Latitude));
            Console.WriteLine("Longitude: " + DecodeDMPostion(location.Longitude));
            Console.WriteLine("Altitude: " + location.Altitude.ToString("f2"));
            Console.WriteLine("Number of satellites: " + location.NumberOfSatellites);
            Console.WriteLine("Fix quality: " + location.FixQuality);
            Console.WriteLine("HDOP: " + location.HorizontalDilutionOfPrecision.ToString("f2"));
            Console.WriteLine("*********************************************\n");
        }
    }
}
using System;
using System.Text;
using Meadow.Foundation.Sensors.GPS;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Sensors.Location.MediaTek
{
    public class NmeaEventArgs
    {
        public string NmeaSentence { get; set; }
    }

    public class Mt3339
    {
        SerialMessagePort serialPort;
        NmeaSentenceParser nmeaParser;

        public event EventHandler<NmeaEventArgs> NmeaSentenceArrived = delegate{};

        public Mt3339(SerialMessagePort serialPort, int baud = 9600)
        {
            this.serialPort = serialPort;
            // TODO: re-expose this.
            //this.serialPort.BaudRate = baud;
            Init();

        }

        protected void Init()
        {
            this.serialPort.MessageReceived += SerialPort_MessageReceived;
            InitParsers();
        }

        public void StartUpdataing()
        {
            Console.WriteLine("initializing serial port");
            serialPort.Open();
            Console.WriteLine("serial port opened.");
        }

        protected void InitParsers()
        {
            Console.WriteLine("Create NMEA");
            nmeaParser = new NmeaSentenceParser();

            Console.WriteLine("Add decoders");
            var ggaDecoder = new GGADecoder();
            nmeaParser.AddDecoder(ggaDecoder);

            ggaDecoder.OnPositionReceived += (object sender, GnssPositionInfo location) => {
                Console.WriteLine($"location.Valid:{location.Valid}");
                Console.WriteLine($"location.NumberOfSatellites:{location.NumberOfSatellites}");
                Console.WriteLine($"location.Position.Latittude:{location.Position.Latitude}");
            };



            var gllDecoder = new GLLDecoder();
            //gllDecoder.OnGeographicLatitudeLongitudeReceived += GllDecoder_OnGeographicLatitudeLongitudeReceived;
            nmeaParser.AddDecoder(gllDecoder);

            var gsaDecoder = new GSADecoder();
            //gsaDecoder.OnActiveSatellitesReceived += GsaDecoder_OnActiveSatelitesReceived;
            nmeaParser.AddDecoder(gsaDecoder);


            // RMC (recommended minimum)
            var rmcDecoder = new RMCDecoder();
            nmeaParser.AddDecoder(rmcDecoder);
            rmcDecoder.OnPositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) => {
                Console.WriteLine($"RMC message decoded; time:{positionCourseAndTime.TimeOfReading}UTC, valid:{positionCourseAndTime.Valid}");
                if (positionCourseAndTime.Valid) {
                    Console.WriteLine($"lat:{positionCourseAndTime.Position.Latitude}, long: {positionCourseAndTime.Position.Longitude}");
                }
                Console.WriteLine("I wish a muthafucka would.");
            };

            // VTG (course made good)
            var vtgDecoder = new VTGDecoder();
            nmeaParser.AddDecoder(vtgDecoder);
            vtgDecoder.OnCourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) => {
                Console.WriteLine($"VTG process finished: trueHeading:{courseAndVelocity.TrueHeading}, magneticHeading:{courseAndVelocity.MagneticHeading}, knots:{courseAndVelocity.Knots}, kph:{courseAndVelocity.Kph}");
            };

        }

        private void SerialPort_MessageReceived(object sender, SerialMessageEventArgs e)
        {
            Console.WriteLine("Message arrived.");

            string msg = (e.GetMessageString(Encoding.ASCII));

            Console.WriteLine($"msg:{msg}");

            Console.WriteLine($"Sending off to the parser");
            nmeaParser.ParseNmeaMessage(msg);

        }

        //private async void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    byte[] bytes = await serialPort.ReadTo(new char[] { '\n' }, false);
        //    var msg = Encoding.ASCII.GetString(bytes);
        //    //Console.WriteLine(msg);

        //    this.NmeaSentenceArrived(this, new NmeaEventArgs() { NmeaSentence = msg });
        //}

        public void StartDumpingReadings()
        {
            //var serialTextFile = new SerialTextFile(serialPort, "\r\n");
            //serialTextFile.OnLineReceived += (s, line) => {
            //    Console.WriteLine(line);
            //};



            //serialPort.ReadToToken('\n');
        }

        //public async Task<GnssPositionInfo> Read()
        //{
        //    var loc = new GnssPositionInfo();
        //    return loc;
        //}

        //public async Task<> StartUpdating()
        //{
        //}

        //public void StopUpdating()
        //{
        //}
    }
}

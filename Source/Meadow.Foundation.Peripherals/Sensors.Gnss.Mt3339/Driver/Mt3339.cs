﻿using System;
using System.Text;
using Meadow.Foundation.Sensors.Location.Gnss;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Gnss
{
    /// <summary>
    /// NMEA Event args - holds an NMEA sentence as a string
    /// </summary>
    public class NmeaEventArgs
    {
        /// <summary>
        /// The NMEA sentence
        /// </summary>
        public string NmeaSentence { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents MT3339 MediaTek high-performance, single-chip, multi-GNSS solution 
    /// </summary>
    public class Mt3339
    {
        readonly ISerialMessagePort serialPort;
        NmeaSentenceProcessor? nmeaProcessor;

        /// <summary>
        /// Raised when GAG data is recieved
        /// </summary>
        public event EventHandler<GnssPositionInfo> GgaReceived = delegate { };

        /// <summary>
        /// Raised when GAG data is recieved
        /// </summary>
        public event EventHandler<GnssPositionInfo> GllReceived = delegate { };

        /// <summary>
        /// Raised when GSA data is recieved
        /// </summary>
        public event EventHandler<ActiveSatellites> GsaReceived = delegate { };

        /// <summary>
        /// Raised when RMC data is recieved
        /// </summary>
        public event EventHandler<GnssPositionInfo> RmcReceived = delegate { };

        /// <summary>
        /// Raised when VTG data is recieved
        /// </summary>
        public event EventHandler<CourseOverGround> VtgReceived = delegate { };

        /// <summary>
        /// Raised when GSV data is recieved
        /// </summary>
        public event EventHandler<SatellitesInView> GsvReceived = delegate { };

        // TODO: if we want to make this public then we're going to have to add
        // a bunch of checks around baud rate, 8n1, etc.
        /// <summary>
        /// Create a new Mt3339 object 
        /// </summary>
        /// <param name="serialPort">The serial port</param>
        protected Mt3339(ISerialMessagePort serialPort)
        {
            this.serialPort = serialPort;

            serialPort.MessageReceived += SerialPort_MessageReceived;

            Initialize();
        }

        /// <summary>
        /// Create a new Mt3339 object
        /// </summary>
        /// <param name="device">IMeadowDevice instance</param>
        /// <param name="serialPortName">The serial port name to create</param>
        public Mt3339(ISerialMessageController device, SerialPortName serialPortName)
            : this(device.CreateSerialMessagePort(
                serialPortName, suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                preserveDelimiter: true, readBufferSize: 512))
        { }

        /// <summary>
        /// Initialize the GPS
        /// </summary>
        protected void Initialize()
        {
            serialPort.MessageReceived += SerialPort_MessageReceived;
            InitDecoders();
            Resolver.Log.Info("Finish Mt3339 initialization.");
        }

        /// <summary>
        /// Start updates
        /// </summary>
        public void StartUpdating()
        {
            serialPort.Open();

            this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_Q_RELEASE));

            this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PGCMD_ANTENNA));

            this.serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_SET_NMEA_OUTPUT_ALLDATA));
        }

        /// <summary>
        /// Initialize decoders
        /// </summary>
        protected void InitDecoders()
        {
            nmeaProcessor = new NmeaSentenceProcessor();

            var mtkDecoder = new MtkDecoder();
         
            nmeaProcessor.RegisterDecoder(mtkDecoder);

            var ggaDecoder = new GgaDecoder();
           
            nmeaProcessor.RegisterDecoder(ggaDecoder);
            ggaDecoder.PositionReceived += (object sender, GnssPositionInfo location) => 
                GgaReceived(this, location);
            
            var gllDecoder = new GllDecoder();
            nmeaProcessor.RegisterDecoder(gllDecoder);
            gllDecoder.GeographicLatitudeLongitudeReceived += (object sender, GnssPositionInfo location) => {
                GllReceived(this, location);
            };

            var gsaDecoder = new GsaDecoder();
            nmeaProcessor.RegisterDecoder(gsaDecoder);
            gsaDecoder.ActiveSatellitesReceived += (object sender, ActiveSatellites activeSatellites) => 
                GsaReceived(this, activeSatellites);
            
            var rmcDecoder = new RmcDecoder();
            nmeaProcessor.RegisterDecoder(rmcDecoder);
            rmcDecoder.PositionCourseAndTimeReceived += (object sender, GnssPositionInfo positionCourseAndTime) => 
                RmcReceived(this, positionCourseAndTime);

            var vtgDecoder = new VtgDecoder();
            nmeaProcessor.RegisterDecoder(vtgDecoder);
            vtgDecoder.CourseAndVelocityReceived += (object sender, CourseOverGround courseAndVelocity) =>
                VtgReceived(this, courseAndVelocity);

            var gsvDecoder = new GsvDecoder();
            nmeaProcessor.RegisterDecoder(gsvDecoder);
            gsvDecoder.SatellitesInViewReceived += (object sender, SatellitesInView satellites) =>
                GsvReceived(this, satellites);
        }

        private void SerialPort_MessageReceived(object sender, SerialMessageData e)
        {
            string msg = (e.GetMessageString(Encoding.ASCII));
            nmeaProcessor?.ProcessNmeaMessage(msg);
        }
    }
}
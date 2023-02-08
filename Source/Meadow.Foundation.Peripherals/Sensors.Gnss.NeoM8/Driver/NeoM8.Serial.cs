using Meadow.Hardware;
using System.Text;

namespace Meadow.Foundation.Sensors.Gnss
{
    public partial class NeoM8
    {
        readonly ISerialMessagePort serialPort;

        // TODO: if we want to make this public then we're going to have to add
        // a bunch of checks around baud rate, 8n1, etc.
        /// <summary>
        /// Create a new NEOM8 object
        /// </summary>
        protected NeoM8(ISerialMessagePort serialPort, IDigitalOutputPort resetPort = null, IDigitalInputPort ppsPort = null)
        {
            this.serialPort = serialPort;
            ResetPort = resetPort;
            PulsePerSecondPort = ppsPort;

            InitializeSerial();
        }

        /// <summary>
        /// Create a new NEOM8 object
        /// </summary>
        /// <param name="device">IMeadowDevice instance</param>
        /// <param name="serialPortName">The serial port name to create</param>
        /// <param name="resetPin">The reset pin</param>
        /// <param name="ppsPin">The pulse per second pin</param>
        public NeoM8(IMeadowDevice device, SerialPortName serialPortName, IPin resetPin, IPin ppsPin = null)
            : this(device.CreateSerialMessagePort(
                serialPortName,
                suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                preserveDelimiter: true,
                readBufferSize: 512),
                device.CreateDigitalOutputPort(resetPin, true),
                device.CreateDigitalInputPort(ppsPin, InterruptMode.EdgeRising, ResistorMode.InternalPullDown))
        { }

        void InitializeSerial()
        {
            communicationMode = CommunicationMode.Serial;
            serialPort.MessageReceived += MessageReceived;
            InitDecoders();

            Reset().Wait();

            Resolver.Log.Debug("Finish NeoM8 Serial initialization");
        }

        void StartUpdatingSerial()
        {
            if (serialPort.IsOpen)
            {
                Resolver.Log.Debug("serial port already open");
                return;
            }

            Resolver.Log.Debug("opening serial port");
            serialPort.Open();
            Resolver.Log.Debug("serial port opened");

            Resolver.Log.Debug("Requesting NMEA data");
            serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_SET_NMEA_OUTPUT_ALLDATA));
            serialPort.Write(Encoding.ASCII.GetBytes(Commands.PMTK_Q_RELEASE));
            serialPort.Write(Encoding.ASCII.GetBytes(Commands.PGCMD_ANTENNA));
        }
    }
}
using Meadow.Hardware;

namespace Meadow.Foundation.Communications
{
    /// <summary>
    ///     Provide a mechanism for reading lines of text from a SerialPort.
    /// </summary>
    public class SerialTextFile
    {
        #region Constants

        /// <summary>
        ///     Default buffer size for the incoming data from the serial port.
        /// </summary>
        private const int MAXIMUM_BUFFER_SIZE = 256;

        #endregion Constants

        #region Member variables / fields

        /// <summary>
        ///     Serial port object that the
        /// </summary>
        private readonly SerialPort _serialPort;

        /// <summary>
        ///     Buffer to hold the incoming text from the serial port.
        /// </summary>
        private string _buffer = string.Empty;

        /// <summary>
        ///     The static buffer is used when processing the text coming in from the
        ///     serial port.
        /// </summary>
        private readonly byte[] _staticBuffer = new byte[MAXIMUM_BUFFER_SIZE];

        /// <summary>
        ///     Character(s) that indicate an end of line in the text stream.
        /// </summary>
        private readonly string _endOfLine = "\r\n";

        #endregion Member variables / fields

        #region Events and delegates

        /// <summary>
        ///     Delegate for the line ready event.
        /// </summary>
        /// <param name="line">Line of text ready for processing.</param>
        /// <param name="sender">Reference to the object generating the event.</param>
        public delegate void LineReceived(object sender, string line);

        /// <summary>
        ///     A complete line of text has been read, send this to the event subscriber.
        /// </summary>
        public event LineReceived OnLineReceived;

        #endregion Events and delegates

        #region Constructors

        /// <summary>
        ///     Default constructor for the SerialTextFile class, made private to prevent the
        ///     programmer from using this method of construcing an object.
        /// </summary>
        private SerialTextFile()
        {
        }

        /// <summary>
        ///     Create a new SerialTextFile and attach the instance to the specfied serial port.
        /// </summary>
        /// <param name="port">Serial port name.</param>
        /// <param name="baudRate">Baud rate.</param>
        /// <param name="parity">Parity.</param>
        /// <param name="dataBits">Data bits.</param>
        /// <param name="stopBits">Stop bits.</param>
        /// <param name="endOfLine">Text indicating the end of a line of text.</param>
        public SerialTextFile(string port, int baudRate, ParityType parity, int dataBits, NumberOfStopBits stopBits,
            string endOfLine)
        {
            _serialPort = new SerialPort(port, baudRate, parity, dataBits, stopBits);
            _endOfLine = endOfLine;
            _serialPort.DataReceived += _serialPort_DataReceived;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Open the serial port and start processing the data from the serial port.
        /// </summary>
        public void Open()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
        }

        /// <summary>
        ///     Close the serial port and stop processing data.
        /// </summary>
        /// <remarks>
        ///     This method clears the buffer and destroys any pending text.
        /// </remarks>
        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _buffer = "";
        }

        #endregion Methods

        #region Interrupt handlers

        /// <summary>
        ///     Process the data from the serial port.
        /// </summary>
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialDataType.Chars)
            {
                lock (_buffer)
                {
                    int amount = ((SerialPort) sender).Read(_staticBuffer, 0, MAXIMUM_BUFFER_SIZE);
                    if (amount > 0)
                    {
                        for (var index = 0; index < amount; index++)
                        {
                            _buffer += (char) _staticBuffer[index];
                        }
                    }
                    var eolMarkerPosition = _buffer.IndexOf(_endOfLine);
                    while (eolMarkerPosition >= 0)
                    {
                        var line = _buffer.Substring(0, eolMarkerPosition);
                        _buffer = _buffer.Substring(eolMarkerPosition + 2);
                        eolMarkerPosition = _buffer.IndexOf(_endOfLine);
                        if (OnLineReceived != null)
                        {
                            OnLineReceived(this, line);
                        }
                    }
                }
            }
        }

        #endregion Interrupt handlers

    }
}
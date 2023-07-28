using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Distance
{
    /// <summary>
    /// Represents the ME007YS serial distance sensor
    /// </summary>
    public class Me007ys : ByteCommsSensorBase<Length>, IRangeFinder
    {
        /// <summary>
        /// Raised when the value of the reading changes
        /// </summary>
        public event EventHandler<IChangeResult<Length>> DistanceUpdated = delegate { };

        /// <summary>
        /// Distance from sensor to object
        /// </summary>
        public Length? Distance { get; protected set; }

        //The baud rate is 9600, 8 bits, no parity, with one stop bit
        readonly ISerialPort serialPort;

        static readonly int portSpeed = 9600;

        static readonly byte[] readBuffer = new byte[16];

        DateTime lastUpdate = DateTime.MinValue;

        TimeSpan? updateInterval;

        /// <summary>
        /// Creates a new ME007YS object communicating over serial
        /// </summary>
        /// <param name="device">The device conected to the sensor</param>
        /// <param name="serialPortName">The serial port</param>
        public Me007ys(IMeadowDevice device, SerialPortName serialPortName)
            : this(device.CreateSerialPort(serialPortName, portSpeed))
        { }

        /// <summary>
        /// Creates a new ME007YS object communicating over serial
        /// </summary>
        /// <param name="serialMessage">The serial message port</param>
        public Me007ys(ISerialPort serialMessage)
        {
            this.serialPort = serialMessage;
            this.serialPort.DataReceived += SerialPort_DataReceived;
        }

        /// <summary>
        /// Start a distance measurement
        /// </summary>
        public void MeasureDistance()
        {
            _ = ReadSensor();
        }

        /// <summary>
        /// Read the distance from the sensor
        /// </summary>
        /// <returns></returns>
        protected override Task<Length> ReadSensor()
        {
            var len = Distance != null ? Distance.Value : new Length(0);
            return Task.FromResult(len);
        }

        /// <summary>
        /// Raise distance change event for subscribers
        /// </summary>
        /// <param name="changeResult"></param>
        protected override void RaiseEventsAndNotify(IChangeResult<Length> changeResult)
        {
            DistanceUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Start updating distances
        /// </summary>
        /// <param name="updateInterval">The interval used to notify external subscribers</param>
        public override void StartUpdating(TimeSpan? updateInterval)
        {
            lock (samplingLock)
            {
                if (IsSampling) return;
                IsSampling = true;

                this.updateInterval = updateInterval;

                serialPort.Open();
            }
        }

        /// <summary>
        /// Stop sampling 
        /// </summary>
        public override void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) { return; }
                IsSampling = false;

                updateInterval = null;

                serialPort.Close();
            }
        }

        //This sensor will write a single byte of 0xFF alternating with 
        //3 bytes: 2 bytes for distance and a 3rd for the checksum
        //when 3 bytes are available we know we have a distance reading ready
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var len = serialPort.BytesToRead;
            serialPort.Read(readBuffer, 0, Math.Min(len, readBuffer.Length));

            if (len == 3)
            {
                var mm = readBuffer[0] << 8 | readBuffer[1];

                ChangeResult<Length> changeResult = new ChangeResult<Length>()
                {
                    New = new Length(mm, Length.UnitType.Millimeters),
                    Old = Distance,
                };

                Distance = changeResult.New;

                if (updateInterval == null || DateTime.Now - lastUpdate >= updateInterval)
                {
                    lastUpdate = DateTime.Now;
                    RaiseEventsAndNotify(changeResult);
                }
            }
        }
    }
}
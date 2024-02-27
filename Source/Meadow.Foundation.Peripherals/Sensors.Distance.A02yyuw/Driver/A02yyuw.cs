using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Distance;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Distance
{
    /// <summary>
    /// Represents the A02YYUW serial distance sensor
    /// </summary>
    public class A02yyuw : PollingSensorBase<Length>, IRangeFinder, ISleepAwarePeripheral, IDisposable
    {
        /// <summary>
        /// Distance from sensor to object
        /// </summary>
        public Length? Distance { get; protected set; }

        /// <summary>
        /// Value returned when the sensor cannot determine the distance
        /// </summary>
        public Length OutOfRangeValue { get; } = new Length(25, Length.UnitType.Centimeters);

        /// <summary>
        /// The maximum time to wait for a sensor reading
        /// </summary>
        public TimeSpan SensorReadTimeOut { get; set; } = TimeSpan.FromSeconds(1000);

        //The baud rate is 9600, 8 bits, no parity, with one stop bit
        private readonly ISerialPort serialPort;

        private static readonly int portSpeed = 9600;

        //Serial read variables 
        readonly byte[] readBuffer = new byte[16];
        int serialDataBytesRead = 0;
        byte serialDataFirstByte;

        private TaskCompletionSource<Length>? dataReceivedTaskCompletionSource;

        private readonly bool createdPort = false;

        private bool isDisposed = false;

        /// <summary>
        /// Creates a new A02YYUW object communicating over serial
        /// </summary>
        /// <param name="device">The device connected to the sensor</param>
        /// <param name="serialPortName">The serial port</param>
        public A02yyuw(IMeadowDevice device, SerialPortName serialPortName)
            : this(device.CreateSerialPort(serialPortName, portSpeed))
        {
            createdPort = true;
        }

        /// <summary>
        /// Creates a new A02YYUW object communicating over serial
        /// </summary>
        /// <param name="serialMessage">The serial message port</param>
        public A02yyuw(ISerialPort serialMessage)
        {
            serialPort = serialMessage;
            serialPort.ReadTimeout = TimeSpan.FromSeconds(5);
            serialPort.DataReceived += SerialPortDataReceived;
        }

        /// <summary>
        /// Start a distance measurement
        /// </summary>
        public void MeasureDistance()
        {
            _ = Read();
        }

        /// <summary>
        /// Convenience method to get the current sensor reading
        /// </summary>
        public override Task<Length> Read()
        {
            return ReadSensor();
        }

        /// <summary>
        /// Read the distance from the sensor
        /// </summary>
        /// <returns></returns>
        protected override Task<Length> ReadSensor()
        {
            return ReadSingleValue();
        }

        /// <summary>
        /// Start updating distances
        /// </summary>
        /// <param name="updateInterval">The interval used to notify external subscribers</param>
        public override void StartUpdating(TimeSpan? updateInterval)
        {
            lock (samplingLock)
            {
                base.StartUpdating(updateInterval);
            }
        }

        /// <summary>
        /// Stop sampling 
        /// </summary>
        public override void StopUpdating()
        {
            lock (samplingLock)
            {
                serialPort?.Close();
                base.StopUpdating();
            }
        }

        //This sensor will write a single byte of 0xFF alternating with 
        //3 bytes: 2 bytes for distance and a 3rd for the checksum
        //when 3 bytes are available we know we have a distance reading ready
        private async Task<Length> ReadSingleValue()
        {
            dataReceivedTaskCompletionSource = new TaskCompletionSource<Length>();

            if (serialPort.IsOpen == false)
            {
                serialPort.Open();
            }

            var timeOutTask = Task.Delay(SensorReadTimeOut);

            await Task.WhenAny(dataReceivedTaskCompletionSource.Task, timeOutTask);

            serialPort.Close();

            if (dataReceivedTaskCompletionSource.Task.IsCompletedSuccessfully == true)
            {
                return dataReceivedTaskCompletionSource.Task.Result;
            }
            return new Length(0);
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort.IsOpen == false || serialPort.BytesToRead == 0 || dataReceivedTaskCompletionSource?.Task.IsCompletedSuccessfully == true)
            {
                return;
            }

            var len = serialPort.BytesToRead;

            serialPort.Read(readBuffer, 0, Math.Min(len, readBuffer.Length));

            for (int i = 0; i < len; i++)
            {
                if (readBuffer[i] == 0xFF)
                {
                    serialDataBytesRead = 0;
                }
                else if (serialDataBytesRead == 0)
                {
                    serialDataFirstByte = readBuffer[i];
                    serialDataBytesRead++;
                }
                else if (serialDataBytesRead == 1)
                {
                    serialDataBytesRead = 2;
                    var lengthInMillimeters = serialDataFirstByte << 8 | readBuffer[i];

                    if (lengthInMillimeters != 0) //device should never return 0
                    {
                        var length = new Length(lengthInMillimeters, Length.UnitType.Millimeters);
                        dataReceivedTaskCompletionSource?.SetResult(length);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Called before the platform goes into Sleep state
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task BeforeSleep(CancellationToken cancellationToken)
        {
            if (createdPort && serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called after the platform returns to Wake state
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task AfterWake(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (createdPort && serialPort != null)
                    {
                        if (serialPort.IsOpen)
                        {
                            serialPort.Close();
                        }
                        serialPort.Dispose();
                    }
                }

                isDisposed = true;
            }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
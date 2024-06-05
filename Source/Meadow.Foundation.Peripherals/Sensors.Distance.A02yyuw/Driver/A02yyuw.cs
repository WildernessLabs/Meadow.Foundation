using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Distance;
using Meadow.Units;
using System;
using System.IO.Ports;
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
        /// Constant for output mode UART Auto
        /// </summary>
        public const byte MODE_UART_AUTO = 1;
        /// <summary>
        /// Constant for output mode UART Control
        /// </summary>
        public const byte MODE_UART_CONTROL = 2;
        /// <summary>
        /// Distance from sensor to object
        /// </summary>
        public Length? Distance => Conditions;
        /// <summary>
        /// The maximum time to wait for a sensor reading
        /// </summary>
        public TimeSpan SensorReadTimeOut { get; set; } = TimeSpan.FromSeconds(2);

        //The baud rate is 9600, 8 bits, no parity, with one stop bit
        private readonly ISerialPort serialPort;

        private static readonly int portSpeed = 9600;

        //Serial read variables 
        readonly byte[] readBuffer = new byte[4];
        int serialDataBytesRead = 0;
        //byte serialDataFirstByte;

        // Output buffer
        private byte[] sendBufer = { 0 };

        private byte outPutMode;

        private readonly bool createdPort = false;

        private bool isDisposed = false;

        private TaskCompletionSource<Length>? dataReceivedTaskCompletionSource;

        /// <summary>
        /// Creates a new A02YYUW object communicating over serial
        /// </summary>
        /// <param name="device">The device connected to the sensor</param>
        /// <param name="serialPortName">The serial port</param>
        /// <param name="outPutModeParam">Output mode of the distance sensor, default is AUTO</param>
        public A02yyuw(IMeadowDevice device, SerialPortName serialPortName, byte outPutModeParam = MODE_UART_AUTO)
            : this(device.CreateSerialPort(serialPortName, portSpeed))
        {
            createdPort = true;
            outPutMode = outPutModeParam;
            serialPort.DataReceived += SerialPortDataReceived;
            serialPort.ReadTimeout = SensorReadTimeOut;
            serialPort.Open();
            serialPort.ClearReceiveBuffer();

        }

        /// <summary>
        /// Creates a new A02YYUW object communicating over serial
        /// </summary>
        /// <param name="serialMessage">The serial message port</param>
        /// <param name="outPutModeParam">Output mode of the distance sensor, default is AUTO</param>

        public A02yyuw(ISerialPort serialMessage, byte outPutModeParam = MODE_UART_AUTO)
        {
            createdPort = true;
            serialPort = serialMessage;
            serialPort.ReadTimeout = SensorReadTimeOut;
            outPutMode = outPutModeParam;
            serialPort.DataReceived += SerialPortDataReceived;
            serialPort.Open();
            serialPort.ClearReceiveBuffer();

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

            if (outPutMode == MODE_UART_CONTROL) // in UART Control Mode, send a 0 value over serial to toggle the control line

            {
                serialPort.Write(sendBufer);
            }
            await Task.WhenAny(dataReceivedTaskCompletionSource.Task, timeOutTask);

            if (dataReceivedTaskCompletionSource.Task.IsCompletedSuccessfully == true)
            {
                return dataReceivedTaskCompletionSource.Task.Result;
            }
            return new Length(0);

        }

        private bool DoCheckSum(int ii)
        {
            var checkSum = (ushort)((ushort)readBuffer[ii] + (ushort)readBuffer[ii + 1] + (ushort)readBuffer[ii + 2]);
            return (checkSum & 0x00FF) == readBuffer[ii + 3];
        }

        private void SerialPortDataReceived(object sender, Hardware.SerialDataReceivedEventArgs e)
        {
            if (serialPort.IsOpen == false || serialPort.BytesToRead == 0 || dataReceivedTaskCompletionSource?.Task.IsCompletedSuccessfully == true)
            {
                return;
            }

            var len = serialPort.BytesToRead;

            //serialPort.Read(readBuffer, 0, Math.Min(len, readBuffer.Length));
            var bytesRead = serialPort.Read(readBuffer, 0, 4);

            if ((bytesRead == 4) && (DoCheckSum(0)))
            {
                var lengthInMillimeters = (readBuffer[1] * 256) + readBuffer[2];
                var length = new Length(lengthInMillimeters, Length.UnitType.Millimeters);
                dataReceivedTaskCompletionSource?.SetResult(length);
                //this.Conditions = new Length(, Length.UnitType.Millimeters);
                dataReceivedTaskCompletionSource?.SetResult(length);
            }
            else
            {
                serialPort.ClearReceiveBuffer();
                serialPort.Write(sendBufer);
                bytesRead = serialPort.Read(readBuffer, 0, 4);
                var lengthInMillimeters = (readBuffer[1] * 256) + readBuffer[2];
                var length = new Length(lengthInMillimeters, Length.UnitType.Millimeters);
                dataReceivedTaskCompletionSource?.SetResult(length);
                //this.Conditions = new Length(, Length.UnitType.Millimeters);
                dataReceivedTaskCompletionSource?.SetResult(length);


            }
            return;
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
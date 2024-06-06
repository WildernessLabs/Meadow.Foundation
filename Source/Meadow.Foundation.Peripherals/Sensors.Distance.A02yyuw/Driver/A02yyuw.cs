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
        readonly byte[] readBuffer;   //= new byte[4];

        private bool createdPort;

        // Output buffer - just a zero
        private byte[] sendBufer = { 0 };

        private byte outPutMode;

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
            if (outPutModeParam == MODE_UART_AUTO)
            {
                readBuffer = new byte[200]; // good for 5 seconds between reads
            }
            else
            {
                readBuffer = new byte[4];
            }
            outPutMode = outPutModeParam;
            createdPort = true;
            //serialPort.DataReceived += SerialPortDataReceived;
            serialPort.ReadTimeout = SensorReadTimeOut;
            if (!(serialPort.IsOpen))
                serialPort.Open();
            serialPort.ClearReceiveBuffer();
        }

        /// <summary>
        /// Creates a new A02YYUW object communicating over serial
        /// </summary>
        /// <param name="serialMessage">The serial message port</param>
        /// <param name="outPutModeParam">Output mode of the distance sensor, default is UART_AUTO</param>

        public A02yyuw(ISerialPort serialMessagePort, byte outPutModeParam = MODE_UART_AUTO)
        {
            if (outPutModeParam == MODE_UART_AUTO)
            {
                readBuffer = new byte[200]; // good for 5 seconds between reads
            }
            else
            {
                readBuffer = new byte[4];
            }
            serialPort = serialMessagePort;
            serialPort.ReadTimeout = SensorReadTimeOut;
            outPutMode = outPutModeParam;
            createdPort = false;
            // serialPort.DataReceived += SerialPortDataReceived;
            if (!(serialPort.IsOpen))
                serialPort.Open();
            serialPort.ClearReceiveBuffer();

        }
        void IRangeFinder.MeasureDistance()
        {
            _ = ReadSensor();
            return;
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
                if (serialPort.IsOpen == false)
                {
                    serialPort.Open();
                }
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
                serialPort.Close();
                base.StopUpdating();
            }
        }


        private async Task<Length> ReadSingleValue()
        {
            int bytesRead, bytesToRead;
            if (outPutMode == MODE_UART_CONTROL) // in UART Control Mode, send a 0 value over serial to toggle the control line
            {
                /* In UART Controlled mode, a trigger pulse (by writing a 0 on serial port) is followed by
                * data for distance presented to serial read after 70 ms.
                * because it is a blocking read, we don't really need the task.Delay, but we need to call 
                * some Task activity because inheritance demands an aync method.
                */
                do
                {
                    serialPort.Write(sendBufer);
                    await Task.Delay(70);
                    bytesRead = serialPort.Read(readBuffer, 0, 4);

                    if ((bytesRead == 4) && (DoCheckSum(0)))
                    {
                        Length distance = new Length((readBuffer[1] * 256) + readBuffer[2], Length.UnitType.Millimeters);
                        Conditions = distance;
                        return distance;
                    }
                    else
                    {
                        serialPort.ClearReceiveBuffer();
                    }
                } while (true);
            }
            else
            {
                if (outPutMode == MODE_UART_AUTO)
                    /* In UART Auto mode, updated distance data is sent to the serial receive 
                     *  buffer automatically  at a rate no faster than 10 Hz. The serial receive buffer
                     *  size default is 1024 bytes. We have a 200 byte serial read buffer, so if
                     *  we are Updating, we can go for at least 5 seconds without losing data.
                     *  We only care about most recent distance, not rest of data in buffer 
                     */
                {
                    bytesToRead = serialPort.BytesToRead;
                    if (bytesToRead >= 200)
                    {
                        serialPort.ClearReceiveBuffer();
                        bytesToRead = 0;
                    }
                    while (bytesToRead < 7) // not just 4 because we may catch the middle of a distance reading
                    {
                        await Task.Delay(100);
                        bytesToRead = serialPort.BytesToRead;
                    }

                    bytesRead = serialPort.Read(readBuffer, 0, bytesToRead);
                    int iByte;
                    for (iByte = bytesToRead - 4; DoCheckSum(iByte) == false && iByte >= 0; iByte -= 1) { };
                    if (iByte > 0)
                    {
                        Length distance = new Length((readBuffer[iByte] * 256) + readBuffer[iByte + 1], Length.UnitType.Millimeters);
                        Conditions = distance;
                        return distance;
                    }
                    else
                    {
                        return new Length(0);
                    }
                }


            }
            return new Length(0); // only get here if there is no distance
        }

    

        private bool DoCheckSum(int ii)
        {
            if (readBuffer[ii] == 255)
            {
                var checkSum = (ushort)((ushort)readBuffer[ii] + (ushort)readBuffer[ii + 1] + (ushort)readBuffer[ii + 2]);
                return (checkSum & 0x00FF) == readBuffer[ii + 3];
            }
            return false;
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
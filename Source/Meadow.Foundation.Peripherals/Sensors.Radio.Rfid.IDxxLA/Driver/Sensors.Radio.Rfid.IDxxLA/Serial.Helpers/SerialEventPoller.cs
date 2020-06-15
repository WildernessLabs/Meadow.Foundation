using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Radio.Rfid.Serial.Helpers
{
    /// <summary>
    /// Helper class to fake events for a serial port by using polling behind the scenes.
    /// Useful until events are fully supported for <see cref="ISerialPort" />.
    /// </summary>
    public class SerialEventPoller : IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Event for if there is data in the serial port buffer to read.
        /// </summary>
        public event DataReceivedEventHandler DataReceived = delegate { };

        /// <summary>
        /// Creates a new event poller for the provided <see cref="ISerialPort" />.
        /// Call <see cref="Start" /> to begin poling.
        /// </summary>
        /// <param name="serialPort">The serial port to poll.</param>
        public SerialEventPoller(ISerialPort serialPort)
        {
            SerialPort = serialPort;
        }

        /// <summary>
        /// The currently used <see cref="ISerialPort" />.
        /// </summary>
        public ISerialPort SerialPort { get; }

        public void Dispose()
        {
            Stop();
            _cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// Start polling the <see cref="ISerialPort" /> buffer.
        /// </summary>
        /// <param name="pollingIntervalMs">The interval between polling calls. Defaults to 100ms.</param>
        public void Start(int pollingIntervalMs = 100)
        {
            Stop();

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            Task.Factory.StartNew(
                () => PollForData(SerialPort, pollingIntervalMs, token),
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        /// <summary>
        /// Stop polling the <see cref="ISerialPort" /> buffer.
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void PollForData(ISerialPort port, int intervalMs, CancellationToken token)
        {
            while (true)
            {
                if (port.IsOpen && port.BytesToRead > 0)
                {
                    var handler = Volatile.Read(ref DataReceived);
                    handler?.Invoke(this, new PolledSerialDataReceivedEventArgs { SerialPort = port });
                }

                Thread.Sleep(intervalMs);
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    public class PolledSerialDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The serial port with data in it's buffer.
        /// You should check there is still data in the buffer before consuming.
        /// </summary>
        public ISerialPort SerialPort { get; set; }
    }

    public delegate void DataReceivedEventHandler(object sender, PolledSerialDataReceivedEventArgs e);
}

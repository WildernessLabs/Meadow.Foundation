using Meadow.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Logging
{
    public class UdpLogger : ILogProvider, IDisposable
    {
        private bool _isDisposed;
        private int _port;
        private UdpClient _client;
        private IPEndPoint _broadcast;
        private char _delimiter;

        public UdpLogger(int port = 5100, char delimiter = '\t')
        {
            _port = port;
            _delimiter = delimiter;
            _client = new UdpClient();
            _client.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
            _broadcast = new IPEndPoint(IPAddress.Broadcast, _port);
        }

        public void Log(LogLevel level, string message)
        {
            var payload = Encoding.UTF8.GetBytes($"{level}{_delimiter}{message}\n");
            _client.Send(payload, payload.Length, _broadcast);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _client.Dispose();
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
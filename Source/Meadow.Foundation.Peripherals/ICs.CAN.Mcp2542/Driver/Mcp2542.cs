using Meadow.Hardware;
using Meadow.Logging;
using System;

namespace Meadow.Foundation.ICs.CAN
{
    /// <summary>
    /// Encapsulation for the Microchip MCP2542 CAN FD transceiver
    /// </summary>
    public partial class Mcp2542
    {
        public const int DefaultBaudRate = 9600;

        private ISerialPort Port { get; }
        private IDigitalOutputPort? STBYPort { get; }
        private Logger? Logger { get; }

        public Mcp2542(ISerialPort port, IDigitalOutputPort? standby = null, Logger? logger = null)
        {
            Port = port;
            Logger = logger;
            STBYPort = standby;

            Port.Open();
        }

        public bool Standby
        {
            set
            {
                if (STBYPort == null) throw new Exception("No standby port provided");
                STBYPort.State = value;
            }
            get
            {
                if (STBYPort == null) return false;
                return STBYPort.State;
            }
        }

        public byte[] Read()
        {
            Logger.Trace($"{Port.BytesToRead} bytes available");

            byte[] buffer = new byte[8];

            var read = Port.Read(buffer, 0, 8);

            Logger.Trace($"RX {read} bytes");

            return buffer;
        }
    }
}
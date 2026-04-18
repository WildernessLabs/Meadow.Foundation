using Meadow.Hardware;
using System;
using System.Text;
using System.Threading;
using SysPorts = System.IO.Ports;

/// <summary>
/// Minimal ISerialMessagePort implementation backed by System.IO.Ports.SerialPort.
/// Uses a background read thread — DataReceived is unreliable on Linux.
/// Used only for desktop testing, not part of the driver itself.
/// </summary>
internal class DesktopSerialMessagePort : ISerialMessagePort
{
    private readonly SysPorts.SerialPort _port;
    private readonly byte[] _delimiter;
    private readonly StringBuilder _accumulator = new StringBuilder();
    private Thread? _readThread;
    private volatile bool _running;

    public event EventHandler<SerialMessageData>? MessageReceived;

    public int BaudRate { get => _port.BaudRate; set => _port.BaudRate = value; }
    public int DataBits => _port.DataBits;
    public bool IsOpen => _port.IsOpen;
    public Meadow.Hardware.Parity Parity => (Meadow.Hardware.Parity)_port.Parity;
    public Meadow.Hardware.StopBits StopBits => (Meadow.Hardware.StopBits)_port.StopBits;
    public string PortName => _port.PortName;
    public int ReceiveBufferSize => _port.ReadBufferSize;

    public DesktopSerialMessagePort(string portName, int baudRate, byte[] delimiter)
    {
        _delimiter = delimiter;
        _port = new SysPorts.SerialPort(portName, baudRate)
        {
            ReadTimeout = 100
        };
    }

    public void Open()
    {
        _port.Open();
        _running = true;
        _readThread = new Thread(ReadLoop) { IsBackground = true, Name = "SerialReadLoop" };
        _readThread.Start();
    }

    public void Close()
    {
        _running = false;
        _port.Close();
    }

    private void ReadLoop()
    {
        var delimStr = Encoding.ASCII.GetString(_delimiter);
        var buf = new byte[256];

        while (_running)
        {
            try
            {
                int count = _port.Read(buf, 0, buf.Length);
                if (count > 0)
                {
                    _accumulator.Append(Encoding.ASCII.GetString(buf, 0, count));
                    var text = _accumulator.ToString();
                    int idx;
                    while ((idx = text.IndexOf(delimStr)) >= 0)
                    {
                        var message = text[..idx];
                        _accumulator.Remove(0, idx + delimStr.Length);
                        text = _accumulator.ToString();

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            MessageReceived?.Invoke(this,
                                new SerialMessageData { Message = Encoding.ASCII.GetBytes(message) });
                        }
                    }
                }
            }
            catch (TimeoutException) { /* normal — no data */ }
            catch { /* port closed */ break; }
        }
    }

    public void ClearReceiveBuffer() => _port.DiscardInBuffer();

    public int Write(byte[] buffer)
    {
        _port.Write(buffer, 0, buffer.Length);
        return buffer.Length;
    }

    public int Write(byte[] buffer, int offset, int count)
    {
        _port.Write(buffer, offset, count);
        return count;
    }
}

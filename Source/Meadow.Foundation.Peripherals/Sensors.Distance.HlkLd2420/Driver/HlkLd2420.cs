using Meadow.Hardware;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Distance;

public class HlkLd2420
{
    private readonly ISerialPort serialPort;
    private readonly SerialMessageProcessor processor;

    public bool IsConnected => serialPort.IsOpen;

    private readonly CircularBuffer<byte> buffer = new CircularBuffer<byte>(4096);
    private bool inCommandMode = false;

    public HlkLd2420(ISerialPort port, IPin? interruptPin = null)
    {
        processor = new SerialMessageProcessor(4096, new byte[] { 0x0d, 0x0a }, false);
        processor.MessageReceived += Processor_MessageReceived;
        serialPort = port;
        serialPort.DataReceived += SerialPort_DataReceived;
    }

    private void Processor_MessageReceived(object sender, SerialMessageData e)
    {
        Debug.WriteLine($"RX: {e.Message.Length} bytes");

        if (e.Message[0] == 0xfd)
        {
            // length
            var length = BitConverter.ToUInt16(e.Message, 4);
            var commandRaw = BitConverter.ToUInt16(e.Message, 6);
            if ((commandRaw & 0x0100) == 0x0100)
            {
                // this is a response to a command
                var command = (Command)(commandRaw & 0xff);
                Debug.WriteLine($"frame of {length} bytes for command {command}");

                switch (command)
                {
                    case Command.ExitCommandMode:
                        inCommandMode = false;
                        break;
                    case Command.EnterCommandMode:
                        inCommandMode = true;
                        break;
                    case Command.ReadVersion:
                        var versionLength = e.Message[10]; // technically it's a little-endian short, but it's never so long it won't fit in 1 byte
                        var version = Encoding.ASCII.GetString(e.Message, 12, versionLength);

                        Debug.WriteLine(version);
                        break;
                }
            }
            else
            {
                Debug.WriteLine($"non-response frame?");
            }
        }
        else // probably text
        {
            var m = e.GetMessageString(Encoding.ASCII);
            if (m.StartsWith("Range"))
            {
                Debug.WriteLine(m);
            }
            else if (m.StartsWith("ON"))
            {
                Debug.WriteLine(m);
            }
            else
            {
                Debug.WriteLine($"malformed message?");
                Debug.WriteLine(m);
            }
        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        processor.Process(serialPort.ReadAll());
    }

    private void SendCommand(Command command)
    {
        SendCommand(command, stackalloc byte[0]);
    }

    private void SendCommand(Command command, ReadOnlySpan<byte> payload)
    {
        serialPort.Write(new byte[]
            {
                0xfd, 0xfc, 0xfb, 0xfa,
            });

        serialPort.Write(new byte[]
            {
                (byte)(payload.Length + 2), 0x00, // technically this is a short, but no point in converting since they are always <= 0xff
            });

        serialPort.Write(new byte[]
            {
                (byte)command, 0x00, // technically this is a short, but no point in converting since they are always <= 0xff
            });

        if (payload.Length > 0)
        {
            serialPort.Write(payload.ToArray());
        }

        serialPort.Write(new byte[] { 0x04, 0x03, 0x02, 0x01 });
    }

    public string ReadVersion()
    {
        SendCommand(Command.ReadVersion);

        Thread.Sleep(100);


        return "test";
    }

    public void EnterEngineeringMode()
    {
        serialPort.Write(new byte[]
            {
                0xfd, 0xfc, 0xfb, 0xfa,
                0x02, 0x00,
                0x62, 0x00,
                0x04, 0x03, 0x02, 0x01
            });

        // TODO: extract the command response
    }

    public void ExitEngineeringMode()
    {
        serialPort.Write(new byte[]
            {
                0xfd, 0xfc, 0xfb, 0xfa,
                0x02, 0x00,
                0x63, 0x00,
                0x04, 0x03, 0x02, 0x01
            });

        // TODO: extract the command response
    }

    public void EnterCommandMode()
    {
        SendCommand(Command.EnterCommandMode, stackalloc byte[] { 0x01, 0x00 });

        processor.ChangeDelimiter(new byte[] { 0x04, 0x03, 0x02, 0x01 });

        Thread.Sleep(100);
    }

    public void ExitCommandMode()
    {
        SendCommand(Command.ExitCommandMode);

        processor.ChangeDelimiter(new byte[] { 0x0d, 0x0a });

        Thread.Sleep(100);
    }

    public void ReadConfiguration()
    {
        serialPort.Write(new byte[]
            {
                0xfd, 0xfc, 0xfb, 0xfa,
                0x02, 0x00,
                0x61, 0x00,
                0x04, 0x03, 0x02, 0x01
            });

        var started = false;

        while (!started)
        {
            // search for header
            serialPort.ReadByte();
            // read until footer
        }
    }

    public async Task<bool> Connect()
    {
        if (!serialPort.IsOpen)
        {
            serialPort.Open();
        }

        return true;
    }

}
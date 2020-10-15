using System;
using System.Threading;
using Meadow.Hardware;
using static Meadow.Foundation.Audio.Mp3.Yx5300;

namespace Meadow.Foundation.Audio.Mp3
{
    public class Yx5300Response
    {
        public Responses Response { get; private set; }
        public int Value { get; private set; }

        public void ParseResponse(byte[] data)
        {


        }
    }

    public class Yx5300
    {
        public enum PlayStatus
        {
            Stopped = 0,
            Playing = 1,
            Paused = 2,
            Unknown,
        }

        public enum Responses
        {
            SDCardInserted = 0x3A,
            PlayComplete = 0x3D,
            Error = 0x40,
            DataReceived = 0x41,
            PlayBackStatus = 0x42,
            Volume = 0x43,
            FileCount = 0x48,
            CurrentFile = 0x4C,
            FolderFileCount = 0x4E,
            FolderCount = 0x4F,
        }

        public enum Commands
        {
            Next = 0x01,
            Previous = 0x02,
            PlayIndex = 0x03,
            VolumeUp = 0x04,
            VolumeDown = 0x05,
            SetVolume = 0x06,
            
            Loop = 0x08,
            SelectDevice = 0x09,
            Sleep = 0x0A,
            Wake = 0x0B,
            Reset = 0x0C,
            Play = 0x0D,
            Pause = 0x0E,
            Stop = 0x16,
            PlayFolder = 0x17,
            Shuffle = 0x18, //might not work
            PlayWithVolume = 0x22,
   
            CurrentFilePlaying = 0x4C,
            GetStatus = 0x42,
            GetVolume = 0x43,
            GetNumberOfTracksInFolder = 0x4E,
            GetTotalTracks = 0x48,
            GetNumberOfFolders = 0x4F
        }

        ISerialPort serialPort;

        protected Yx5300(ISerialPort serialPort)
        {
            this.serialPort = serialPort;

            serialPort.Open();

            SendCommand(Commands.Reset);

            Thread.Sleep(100);

            SendCommand((byte)Commands.SelectDevice, 0, 2);

            Thread.Sleep(500);
        }

        public Yx5300(IIODevice device, SerialPortName serialPortName)
            : this(device.CreateSerialPort(
                serialPortName))
        { }

        public void SetVolume(byte volume)
        {
            if(volume > 30) { volume = 30; }
            SendCommand((byte)Commands.SetVolume, 0, volume);
        }

        public void Play()
        {
            SendCommand(Commands.Play);
        }

        public void Pause()
        {
            SendCommand(Commands.Pause);
        }

        public void Stop()
        {
            SendCommand(Commands.Stop);
        }

        public void SendCommand(Commands command)
        {
            SendCommand((byte)command, 0, 0);
        }

        public void SendCommand(byte command, byte data1, byte data2)
        {
            byte[] sendBuffer = new byte[8]; // { 0 }; // Buffer for Send commands.
            String mp3send = string.Empty;

            Thread.Sleep(20);

            // Command Structure 0x7E 0xFF 0x06 CMD FBACK DAT1 DAT2 0xEF
            sendBuffer[0] = 0x7E;    // Start byte
            sendBuffer[1] = 0xFF;    // Version
            sendBuffer[2] = 0x06;    // Command length not including Start and End byte.
            sendBuffer[3] = command; // Command
            sendBuffer[4] = 0x01;    // Feedback 0x00 NO, 0x01 YES
            sendBuffer[5] = data2;    // DATA1 datah
            sendBuffer[6] = data2;    // DATA2 datal
            sendBuffer[7] = 0xEF;    // End byte

            serialPort.Write(sendBuffer);

            Console.WriteLine($"SendBuffer: {BitConverter.ToString(sendBuffer)}");
        }

        public byte[] ReadResponse()
        {
            byte[] response = new byte[15];
            byte value = 0;
            int index = 0;

            do
            {
                if(serialPort.BytesToRead == 0)
                {
                    Console.WriteLine("No data available");
                    Thread.Sleep(50);
                    break;
                }

                value = (byte)serialPort.ReadByte();

                if(value == 0x7E) //new response
                {
                    index = 0;
                }
                response[index++] = value;
            }
            while (value != 0xEF);

            return response;
        }
    }
}
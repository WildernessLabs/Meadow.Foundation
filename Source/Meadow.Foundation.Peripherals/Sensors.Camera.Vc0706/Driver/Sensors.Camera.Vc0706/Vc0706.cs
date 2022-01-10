using Meadow.Hardware;
using System;
using System.Text;
using System.Threading;

namespace Meadow.Foundation.Sensors.Camera
{
    public class Vc0706
    {
        static byte RESET = 0x26;
        static byte GEN_VERSION = 0x11;
        static byte SET_PORT = 0x24;
        static byte READ_FBUF = 0x32;
        static byte GET_FBUF_LEN = 0x34;
        static byte FBUF_CTRL = 0x36;
        static byte DOWNSIZE_CTRL = 0x54;
        static byte DOWNSIZE_STATUS = 0x55;
        static byte READ_DATA = 0x30;
        static byte WRITE_DATA = 0x31;
        static byte COMM_MOTION_CTRL = 0x37;
        static byte COMM_MOTION_STATUS = 0x38;
        static byte COMM_MOTION_DETECTED = 0x39;
        static byte COLOR_CTRL = 0x3C;
        static byte COLOR_STATUS = 0x3D;
        static byte MOTION_CTRL = 0x42;
        static byte MOTION_STATUS = 0x43;
        static byte TVOUT_CTRL = 0x44;
        static byte OSD_ADD_CHAR = 0x45;

        static byte STOPCURRENTFRAME = 0x0;
      //  static byte STOPNEXTFRAME = 0x1;
        static byte RESUMEFRAME = 0x3;
      //  static byte STEPFRAME = 0x2;

        public enum ImageSize : byte
        {
            _640x480 = 0x00,
            _320x240 = 0x11,
            _160x120 = 0x22,
            Unknown,
        }

        public enum Baud : byte
        {
            _9600,
            _19200,
            _38400,
            _57600
        }

        public enum ColorMode : byte
        {
            Automatic,
            Color,
            BlackWhite,
        }

        static byte MOTIONCONTROL = 0x0;
        static byte UARTMOTION = 0x01;
        static byte ACTIVATEMOTION = 0x01;

        static byte SET_ZOOM = 0x52;
        static byte GET_ZOOM = 0x53;

        static byte CAMERABUFFSIZE = 100;
        static byte CAMERADELAY = 10;

        ISerialPort serialPort;

        byte serialNum;
        byte[] camerabuff = new byte[CAMERABUFFSIZE + 1];
        byte bufferLen;
        ushort frameptr;

        public Vc0706(ISerialController device, SerialPortName portName, int baud)
        {
             serialPort = device.CreateSerialPort(portName, baud);
            /*serialPort = device.CreateSerialMessagePort(
                        portName: portName, 
                        suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                        baudRate: baud,
                        preserveDelimiter: true, 
                        readBufferSize: 512);*/
            serialPort.Open();

            switch(baud)
            {
                case 9600:
                default:
                    SetBaud9600();
                    break;
                case 19200:
                    SetBaud19200();
                    break;
                case 38400:
                    SetBaud38400();
                    break;
                case 57600:
                    SetBaud57600();
                    break;
            }
        }

        bool Reset()
        {
            byte[] args = { 0x0 };

            return RunCommand(RESET, args, 1, 5);
        }

        public bool IsMotionDetected()
        {
            if (ReadResponse(4) != 4)
            {
                return false;
            }
            if (!VerifyResponse(COMM_MOTION_DETECTED))
            {
                return false;
            }

            return true;
        }

        public bool SetMotionStatus(byte x, byte d1, byte d2)
        {
            byte[] args = { 0x03, x, d1, d2 };

            return RunCommand(MOTION_CTRL, args, (byte)args.Length, 5);
        }

        public bool GetMotionStatus(byte x)
        {
            byte[] args = { 0x01, x };

            return RunCommand(MOTION_STATUS, args, (byte)args.Length, 5);
        }

        public bool SetMotionDetect(bool flag)
        {
            if (!SetMotionStatus(MOTIONCONTROL, UARTMOTION,
                                 ACTIVATEMOTION))
                return false;

            byte[] args = { 0x01, (byte)((flag == true)?1:0) };

            return RunCommand(COMM_MOTION_CTRL, args, (byte)args.Length, 5);
        }

        public bool GetMotionDetect()
        {
            byte[] args = { 0x0 };

            if (!RunCommand(COMM_MOTION_STATUS, args, 1, 6))
                return false;

            return camerabuff[5] != 0;
        }

        public ImageSize GetImageSize()
        {
            byte[] args = { 0x4, 0x4, 0x1, 0x00, 0x19 };
            if (!RunCommand(READ_DATA, args, (byte)args.Length, 6))
            { 
                return ImageSize.Unknown; 
            }

            return (ImageSize)camerabuff[5];
        }

        public bool SetImageSize(ImageSize imageSize)
        {
            byte[] args = { 0x05, 0x04, 0x01, 0x00, 0x19, (byte)imageSize };

            var ret = RunCommand(WRITE_DATA, args, (byte)args.Length, 5);
            Reset();
            return ret;
        }

        public byte GetDownsize()
        {
            byte[] args = { 0x0 };
            if (RunCommand(DOWNSIZE_STATUS, args, 1, 6) == false)
            {
                return 0;//ToDo was -1 in Arduino code - validate
            }

            return camerabuff[5];
        }

        public bool SetDownsize(byte newsize)
        {
            byte[] args = { 0x01, newsize };

            return RunCommand(DOWNSIZE_CTRL, args, 2, 5);
        }
        public string GetVersion()
        {
            SendCommand(GEN_VERSION, new byte[]{ 0x01 }, 1);
            // get reply
            if (ReadResponse(CAMERABUFFSIZE) == 0)
            { return string.Empty; }

            camerabuff[bufferLen] = 0; 

            var versionData = new byte[14];
            Array.Copy(camerabuff, 5, versionData, 0, 11);
            versionData[12] = versionData[13] = 0;

            return new string(new UTF8Encoding().GetChars(versionData));
        }

        bool SetBaud(Baud baud)
        {
            byte[] args;
            switch (baud)
            {
                case Baud._57600:
                    args = new byte[] { 0x03, 0x01, 0x1C, 0x1C };
                    break;
                case Baud._38400:
                    args = new byte[] { 0x03, 0x01, 0x2A, 0xF2 };
                    break;
                case Baud._19200:
                    args = new byte[]{ 0x03, 0x01, 0x56, 0xE4 };
                    break;
                case Baud._9600:
                default:
                    args = new byte[] { 0x03, 0x01, 0xAE, 0xC8 };
                    break;
            }

            SendCommand(SET_PORT, args, (byte)args.Length);
            // get reply
            if (ReadResponse(CAMERABUFFSIZE) == 0)
            {
                return false;
            }

            camerabuff[bufferLen] = 0; // end it!
            return true;
        }

        bool SetBaud9600()
        {
            return SetBaud(Baud._9600);
        }

        bool SetBaud19200()
        {
            return SetBaud(Baud._19200);
        }

        bool SetBaud38400()
        {
            return SetBaud(Baud._38400);
        }

        public bool SetBaud57600()
        {
            return SetBaud(Baud._57600);
        }

        public void SetOnScreenDisplay(byte x, byte y, string message)
        {
            if (message.Length > 14)
            {
                message = message.Substring(0, 14);
            }

            var args = new byte[17];
            args[0] = (byte)message.Length;
            args[1] = (byte)(message.Length - 1);
            args[2] = (byte)((y & 0xF) | ((x & 0x3) << 4));

            for (byte i = 0; i < message.Length; i++)
            {
                char c = message[i];

                if ((c >= '0') && (c <= '9'))
                {
                    args[3 + i] = (byte)(c - '0');
                }
                else if ((c >= 'A') && (c <= 'Z'))
                {
                    args[3 + i] = (byte)(c - 'A');
                    args[3 + i] += 10;
                }
                else if ((c >= 'a') && (c <= 'z'))
                {
                    args[3 + i] = (byte)(c - 'a');
                    args[3 + i] += 36;
                }
            }

            RunCommand(OSD_ADD_CHAR, args, (byte)args.Length, 5);
           // printBuff();
        }

        public bool SetCompression(byte c)
        {
            byte[] args = { 0x5, 0x1, 0x1, 0x12, 0x04, c };
            return RunCommand(WRITE_DATA, args, (byte)args.Length, 5);
        }

        public byte GetCompression()
        {
            byte[] args = { 0x4, 0x1, 0x1, 0x12, 0x04 };
            RunCommand(READ_DATA, args, (byte)args.Length, 6);
            PrintBuffer();
            return camerabuff[5];
        }
        public bool SetPanTiltZoom(ushort wz, ushort hz, ushort pan,
                                        ushort tilt)
        {
            byte[] args = {0x08, 
                            (byte)(wz >> 8), (byte)wz, 
                            (byte)(hz >> 8), (byte)wz,
                            (byte)(pan >> 8), (byte)pan,     
                            (byte)(tilt >> 8), (byte)tilt};

            return (!RunCommand(SET_ZOOM, args, (byte)args.Length, 5));
        }

        public Tuple<ushort, ushort, ushort, ushort, ushort, ushort> GetPanTiltZoom()
        {
            byte[] args = { 0x0 };

            if (!RunCommand(GET_ZOOM, args, (byte)args.Length, 16))
            { return null; }

            PrintBuffer();

            ushort w = camerabuff[5];
            w <<= 8;
            w |= camerabuff[6];

            ushort h = camerabuff[7];
            h <<= 8;
            h |= camerabuff[8];

            ushort wz = camerabuff[9];
            wz <<= 8;
            wz |= camerabuff[10];

            ushort hz = camerabuff[11];
            hz <<= 8;
            hz |= camerabuff[12];

            ushort pan = camerabuff[13];
            pan <<= 8;
            pan |= camerabuff[14];

            ushort tilt = camerabuff[15];
            tilt <<= 8;
            tilt |= camerabuff[16];

            return new Tuple<ushort, ushort, ushort, ushort, ushort, ushort>(w, h, wz, hz, pan, tilt);
        }

        public bool TakePicture()
        {
            frameptr = 0;
            return CameraFrameBuffCtrl(STOPCURRENTFRAME);
        }

        public bool ResumeVideo()
        {
            return CameraFrameBuffCtrl(RESUMEFRAME);
        }

        public bool TvOn()
        {
            byte[] args = { 0x1, 0x1 };
            return RunCommand(TVOUT_CTRL, args, (byte)args.Length, 5);
        }

        public bool TvOff()
        {
            byte[] args = { 0x1, 0x0 };
            return RunCommand(TVOUT_CTRL, args, (byte)args.Length, 5);
        }

        public ColorMode GetColorMode()
        {
            RunCommand(COLOR_STATUS, new byte[] { 0x1 }, 1, 8);
            return (ColorMode)camerabuff[7];
        }

        public void SetColorMode(ColorMode colorControl)
        {
            RunCommand(COLOR_CTRL, new byte[] { 0x1, (byte)colorControl }, 2, 5);
        }

        bool CameraFrameBuffCtrl(byte command)
        {
            byte[] args = { 0x1, command };
            return RunCommand(FBUF_CTRL, args, (byte)args.Length, 5);
        }

        public uint GetFrameLength()
        {
            byte[] args = { 0x01, 0x00 };
            if (!RunCommand(GET_FBUF_LEN, args, (byte)args.Length, 9))
                return 0;

            uint len;
            len = camerabuff[5];
            len <<= 8;
            len |= camerabuff[6];
            len <<= 8;
            len |= camerabuff[7];
            len <<= 8;
            len |= camerabuff[8];

            return len;
        }

        public byte BytesAvailable()
        { 
            return bufferLen; 
        }

        public byte[] ReadPicture(byte n)
        {
            byte[] args = {0x0C,
                    0x0,
                    0x0A,
                    0,
                    0,
                    (byte)(frameptr >> 8),
                    (byte)(frameptr & 0xFF),
                    0,
                    0,
                    0,
                    n,
                    (byte)(CAMERADELAY >> 8),
                    (byte)(CAMERADELAY & 0xFF)};

            if (!RunCommand(READ_FBUF, args, (byte)args.Length, 5, false))
            { 
                return new byte[0]; 
            }

            // read into the buffer PACKETLEN!
            if (ReadResponse((byte)(n + 5), CAMERADELAY) == 0)
            { 
                return new byte[0]; 
            }

            frameptr += n;

            return camerabuff;
        }

        bool RunCommand(byte cmd, 
                        byte[] args, 
                        byte argn,
                        byte resplen, 
                        bool flushflag = true)
        {   // flush out anything in the buffer?
            if (flushflag)
            {
                ReadResponse(100, 10);
            }

            SendCommand(cmd, args, argn);
            if (ReadResponse(resplen) != resplen)
            {
                return false;
            }
            if (!VerifyResponse(cmd))
            {
                return false;
            }
            return true;
        }

        void SendCommand(byte cmd, byte[] args = null, byte argn = 0)
        {
            serialPort.Write(new byte[] { 0x56, serialNum, cmd });

            for (byte i = 0; i < argn; i++)
            {
                serialPort.Write(new byte[] { args[i] });
            }
        }

        byte ReadResponse(byte numbytes, byte timeout = 200)
        {
            byte counter = 0;
            bufferLen = 0;
            int avail;

            //100 != 0 && 200 != 0
            while ((bufferLen != numbytes) && (timeout != counter))
            {
                avail = serialPort.BytesToRead;

                if (avail <= 0)
                {
                    Thread.Sleep(1);
                    counter++;
                    continue;
                }
                counter = 0;
                // there's a byte!
                camerabuff[bufferLen++] = (byte)serialPort.ReadByte(); // hwSerial->read();
                //Console.WriteLine($"{avail}: A byte! {camerabuff[bufferLen - 1]}");
            }
            return bufferLen;
        }

        bool VerifyResponse(byte command)
        {
            if ((camerabuff[0] != 0x76) || (camerabuff[1] != serialNum) ||
                (camerabuff[2] != command) || (camerabuff[3] != 0x0))
            {
                return false;
            }
            return true;
        }

        void PrintBuffer()
        {
            for (byte i = 0; i < bufferLen; i++)
            {
                Console.WriteLine(camerabuff[i]);
            }
        }
    }
}
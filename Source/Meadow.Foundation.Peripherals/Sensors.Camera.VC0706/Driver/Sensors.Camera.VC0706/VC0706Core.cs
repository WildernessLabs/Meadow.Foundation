using System;
using System.IO;
using System.Text;
using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Camera
{
    public class VC0706Core : IDisposable
    {
        #region enums

        //could make zoom public but the camera is so low res I don't see the value
        enum Zoom : byte
        {
            Normal,
            _2x,
            _4x
        }

        protected enum Command : byte
        {
            GEN_VERSION = 0x11,
            SET_SERIAL_NUMBER = 0x21,
            SET_PORT = 0x24,
            SYSTEM_RESET = 0x26,
            READ_DATA = 0x30,
            WRITE_DATA = 0x31,
            READ_FBUF = 0x32,
            WRITE_FBUF = 0x33,
            GET_FBUF_LEN = 0x34,
            SET_FBUF_LEN = 0x35,
            FBUF_CTRL = 0x36,
            COMM_MOTION_CTRL = 0x37,
            COMM_MOTION_STATUS = 0x38,
            COMM_MOTION_DETECTED = 0x39,
            MIRROR_CTRL = 0x3A,
            MIRROR_STATUS = 0x3B,
            COLOR_CTRL = 0x3C,
            COLOR_STATUS = 0x3D,
            POWER_SAVE_CTRL = 0x3E,
            POWER_SAVE_STATUS = 0x3F,
            AE_CTRL = 0x40,
            AE_STATUS = 0x41,
            MOTION_CTRL = 0x42,
            MOTION_STATUS = 0x43,
            TV_OUT_CTRL = 0x44,
            OSD_ADD_CHAR = 0x45, // unsupported by the VC0706 firmware
            DOWNSIZE_SIZE_SET = 0x52,
            DOWNSIZE_SIZE_GET = 0x53,
            DOWNSIZE_CTRL = 0x54,
            DOWNSIZE_STATUS = 0x55,
            GET_FLASH_SIZE = 0x60,
            ERASE_FLASH_SECTOR = 0x61,
            ERASE_FLASH_ALL = 0x62,
            READ_LOGO = 0x70,
            SET_BITMAP = 0x71,
            BATCH_WRITE = 0x80
        }

        protected enum FrameCommand : byte
        {
            STOPCURRENTFRAME = 0x0,
            STOPNEXTFRAME = 0x1,
            RESUMEFRAME = 0x3,
            STEPFRAME = 0x2,
            MOTIONCONTROL = 0x0,
            UARTMOTION = 0x01,
            ACTIVATEMOTION = 0x01,
        }

        #endregion

        #region fields

        private const int MaxCommandLength = 20;
        private byte[] _command = new byte[MaxCommandLength];
        private byte[] _response = new byte[MaxCommandLength + 1];

        private ISerialPort _serialPort;
        //ToDo replace _dataReceivedEvent
      //  private ManualResetEvent _dataReceivedEvent = new ManualResetEvent(false);

        private const int _bufferSize = 120;
        private byte[] _frameBuffer = new byte[_bufferSize];

        private const byte CameraSerial = 0;
        private const byte CommandSend = 0x56;
        private const byte CommandReply = 0x76;
        private const byte ReplyIndex = 0;
        private const byte ReplySerialIndex = 1;
        private const byte ReplyCommandIndex = 2;
        private const byte ReplyStatusIndex = 3;

        #endregion

        public void Initialize(ISerialPort comPort)
        {
            _serialPort = comPort;

            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
       //     _comPort.ErrorReceived += new SerialErrorReceivedEventHandler(ErrorReceivedHandler);
        }

        public void SetComPort(ISerialPort comPort)
        {
            _serialPort = comPort;
        }

        public void SetMotionDetection(bool flag)
        {
            SetMotionStatus(FrameCommand.MOTIONCONTROL, FrameCommand.UARTMOTION, FrameCommand.ACTIVATEMOTION);
            RunCommand(Command.COMM_MOTION_CTRL, new byte[] { (flag) ? (byte)1 : (byte)0 }, 5);
        }

        public bool GetMotionDetectionStatus()
        {
            RunCommand(Command.COMM_MOTION_STATUS, null, 6);

            return (_response[5] == 1) ? true : false;
        }

        public bool IsMotionDetected()
        {
            if (ReadResponse(4, false) != 0)
                return ValidateCommandResponse(Command.COMM_MOTION_DETECTED);

            return false;
        }

        protected void SetMotionStatus(FrameCommand x, FrameCommand d1, FrameCommand d2)
        {
            RunCommand(Command.MOTION_CTRL, new byte[] { (byte)x, (byte)d1, (byte)d2 }, 5);
        }

        public void SetColorControl(byte colorControl)
        {
            RunCommand(Command.COLOR_CTRL, new byte[] { 0x1, colorControl }, 5);
        }

        public byte GetColorStatus()
        {
            RunCommand(Command.COLOR_STATUS, new byte[] { 0x1 }, 8);
            return _response[7];
            //showMode = _response[6];
        }

        public byte GetImageSize()
        {
            RunCommand(Command.READ_DATA, new byte[] { 0x4, 0x1, 0x00, 0x19 }, 6);
            return _response[5];
        }

        public void SetImageSize(byte size)
        {
            RunCommand(Command.WRITE_DATA, new byte[] { 0x04, 0x01, 0x00, 0x19, size }, 5);
            Reset();
        }

        public void GetDownSize(out byte width, out byte height)
        {
            RunCommand(Command.DOWNSIZE_STATUS, null, 6);

            var temp = _response[5];
            temp &= 0x3;
            width = temp;

            temp = _response[5];
            temp >>= 4;
            temp &= 0x3;
            height = temp;
        }

        public byte GetDownSize()
        {
            RunCommand(Command.DOWNSIZE_STATUS, null, 6);
            return _response[5];
        }

        public void SetDownSize(byte width, byte height)
        {
            byte size = (byte)((height & 0x3) << 4);
            size |= (byte)(width & 0x3);

            SetDownSize(size);
        }

        public void SetDownSize(byte size)
        {
            RunCommand(Command.DOWNSIZE_CTRL, new byte[] { size }, 5);
        }

        public string GetVersion()
        {
            RunCommand(Command.GEN_VERSION, new byte[] { 0x01 }, 16);

            Array.Copy(_response, 5, _response, 0, 11);
            _response[12] = _response[13] = 0;

            return new string(new UTF8Encoding().GetChars(_response));
        }

        public byte GetCompression()
        {
            RunCommand(Command.READ_DATA, new byte[] { 0x1, 0x1, 0x12, 0x04 }, 6);
            return _response[5];
        }

        public void SetCompression(byte compression)
        {
            RunCommand(Command.WRITE_DATA, new byte[] { 0x1, 0x1, 0x12, 0x04, compression }, 5);
        }

        public void SetPortSpeed(short speed)
        {
            RunCommand(Command.SET_PORT, new byte[] { 0x1, (byte)((short)speed >> 8), (byte)((short)speed & 0xFF) }, 5);
        }

        public void SetPanTiltZoom(short zoomWidth, short zoomHeight, short pan, short tilt)
        {
            RunCommand(Command.DOWNSIZE_SIZE_SET, new byte[]
            {
                (byte)(zoomWidth >> 8),
                (byte)(zoomWidth & 0xFF),
                (byte)(zoomHeight >> 8),
                (byte)(zoomWidth & 0xFF),
                (byte)(pan>>8),
                (byte)(pan & 0xFF),
                (byte)(tilt>>8),
                (byte)(tilt & 0xFF) }, 5);
        }

        public void GetPanTiltZoom(out ushort width, out ushort height, out ushort zoomWidth, out ushort zoomHeight, out ushort pan, out ushort tilt)
        {
            width = height = zoomWidth = zoomHeight = pan = tilt = 0;
            RunCommand(Command.DOWNSIZE_SIZE_GET, null, 16);
            width = _response[5];
            width <<= 8;
            width |= _response[6];
            height = _response[7];
            height <<= 8;
            height |= _response[8];
            zoomWidth = _response[9];
            zoomWidth <<= 8;
            zoomWidth |= _response[10];
            zoomHeight = _response[11];
            zoomHeight <<= 8;
            zoomHeight |= _response[12];
            pan = _response[13];
            pan <<= 8;
            pan |= _response[14];
            tilt = _response[15];
            tilt <<= 8;
            tilt |= _response[16];
        }

        protected void ControlFrame(FrameCommand command)
        {
            RunCommand(Command.FBUF_CTRL, new byte[] { (byte)command }, 5);
        }

        protected int GetFrameLength()
        {
            RunCommand(Command.GET_FBUF_LEN, new byte[] { 0x00 }, 9);
            int length = _response[5];
            length <<= 8;
            length |= _response[6];
            length <<= 8;
            length |= _response[7];
            length <<= 8;
            length |= _response[8];
            return length;
        }

        public short CameraDelayMilliSec
        {
            get { return _cameraDelayMilliSec; }
            set
            {
                _cameraDelayMilliSec = value;
                _serialPort.ReadTimeout = _cameraDelayMilliSec * 2;
             //   _comPort.WriteTimeout = _cameraDelayMilliSec * 2;
            }
        }
        private short _cameraDelayMilliSec;

        public void TakePicture(string path)
        {
            ControlFrame(FrameCommand.STOPCURRENTFRAME);

            var frameAddress = 0;
            var frameLength = GetFrameLength();
            Console.WriteLine("Frame length: " + frameLength.ToString());

            using (var picFile = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                var totalBytesRead = 0;
                while (frameLength > 0)
                {
                    var segmentLength = System.Math.Min(frameLength, _frameBuffer.Length);
                    totalBytesRead += ReadFrameSegment(segmentLength, _frameBuffer, frameAddress);
                    picFile.Write(_frameBuffer, 0, segmentLength);
                    frameLength -= segmentLength;
                    frameAddress += segmentLength;
                }

                Console.WriteLine("Total bytes read: " + totalBytesRead.ToString());

                picFile.Flush();
                picFile.Close();
            }
            ReadResponse(5);

            ControlFrame(FrameCommand.RESUMEFRAME);
        }

        protected int ReadFrameSegment(int segmentLength, byte[] frameBuffer, int frameAddress)
        {
            RunCommand(Command.READ_FBUF,
                new byte[] { 0x0, 0x0A,
                    (byte)((frameAddress >> 24) & 0xFF), (byte)((frameAddress >> 16) & 0xFF), (byte)((frameAddress >> 8) & 0xFF), (byte)(frameAddress & 0xFF),
                    (byte)((segmentLength >> 24) & 0xFF), (byte)((segmentLength >> 16) & 0xFF), (byte)((segmentLength >> 8) & 0xFF), (byte)(segmentLength & 0xFF),
                    (byte)((CameraDelayMilliSec >> 8) & 0xFF), (byte)(CameraDelayMilliSec & 0xFF)
                }, 5);

            var totalBytesRead = 0;

            while (segmentLength > 0)
            {
                if (WaitForIncomingData(GetTimeoutMilliSec(segmentLength)) == false) throw new ApplicationException("Timeout");
                var bytesToRead = System.Math.Min(_serialPort.BytesToRead, segmentLength);
                var bytesRead = _serialPort.Read(frameBuffer, 0, bytesToRead);
                segmentLength -= bytesRead;
                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        private bool WaitForIncomingData(int timeout)
        {
         //   var result = _dataReceivedEvent.WaitOne(timeout, false);
         //   if (result)
         //   {
         //       _dataReceivedEvent.Reset();
         //   }

            if (_serialPort.BytesToRead > 0)
            {
                return true;
            }

            return false;
        }

        private const int TimeoutToleranceMilliSec = 50;

        private int GetTimeoutMilliSec(int bufferLength)
        {
            var timeout = ((bufferLength / (_serialPort.BaudRate >> 3)) * 1000) + CameraDelayMilliSec + TimeoutToleranceMilliSec;
            return timeout;
        }

        public void TvOutput(bool enable)
        {
            RunCommand(Command.TV_OUT_CTRL, new byte[] { (enable) ? (byte)0x01 : (byte)0x00 }, 5);
        }

        protected void Reset(int delayMS = 1000)
        {
            RunCommand(Command.SYSTEM_RESET, null, 5);
            Thread.Sleep(delayMS);
        }

        public void Dispose()
        {
            _command = null;
            _response = null;
            _frameBuffer = null;
         //   _dataReceivedEvent = null;
        }

        protected void RunCommand(Command command, byte[] args, int expectedResponseLength, bool flushComPort = true)
        {
            RunCommand(command, args, expectedResponseLength, _serialPort, flushComPort);
        }

        protected void RunCommand(Command command, byte[] args, int expectedResponseLength, ISerialPort comPort, bool flushComPort = true)
        {
            if (flushComPort)
            { 
            //    comPort.DiscardInBuffer();
            }

            SendCommandToCamera(command, args);
            ReadResponse(expectedResponseLength);
            ValidateCommandResponse(command);
        }

        protected int ReadResponse(int expectedResponseLength, bool throwExceptionOnTimeout = true)
        {
            var responseLength = 0;
            var responseIndex = 0;

            while (expectedResponseLength > 0)
            {
                if (WaitForIncomingData(GetTimeoutMilliSec(_response.Length)) == false)
                {
                    if (throwExceptionOnTimeout) throw new ApplicationException("Timeout");
                    else break;
                }
                var bytesRead = _serialPort.Read(_response, responseIndex, expectedResponseLength);
                responseLength += bytesRead;
                expectedResponseLength -= bytesRead;
                responseIndex += bytesRead;
            }
            return responseLength;
        }

        protected bool ValidateCommandResponse(Command command)
        {
            if ((_response[ReplyIndex] != CommandReply) ||
                (_response[ReplySerialIndex] != CameraSerial) ||
                (_response[ReplyCommandIndex] != (byte)command) ||
                (_response[ReplyStatusIndex] != 0x0))
            {
                switch (_response[ReplyStatusIndex])
                {
                    case 1:
                        throw new ApplicationException("Command not received");
                    case 2:
                        throw new ApplicationException("Invalid data length");
                    case 3:
                        throw new ApplicationException("Invalid data format");
                    case 4:
                        throw new ApplicationException("Command cannot be executed now");
                    case 5:
                        throw new ApplicationException("Command executed incorrectly");
                }
            }
            return true;
        }

        protected void SendCommandToCamera(Command command, byte[] args)
        {
            var commandIndex = 0;
            _command[commandIndex++] = CommandSend;
            _command[commandIndex++] = CameraSerial;
            _command[commandIndex++] = (byte)command;
            var argsLength = 0;

            if (args != null)
            {
                argsLength = args.Length;
                _command[commandIndex++] = (byte)argsLength;
                Array.Copy(args, 0, _command, commandIndex, argsLength);
            }
            else
            {
                _command[commandIndex++] = 0;
            }
            _serialPort.Write(_command, 0, argsLength + 4);
        }

        protected virtual void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
          //ToDo  _dataReceivedEvent.Set();
        }

        protected virtual void ErrorReceivedHandler(object sender, SerialErrorReceivedEventArgs e)
        {
        }
    }
}
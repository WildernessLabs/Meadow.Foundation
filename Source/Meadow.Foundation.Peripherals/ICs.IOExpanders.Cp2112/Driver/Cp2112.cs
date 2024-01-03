using Meadow.Hardware;
using System;
using System.Linq;
using static Meadow.Foundation.ICs.IOExpanders.Native;

#nullable enable

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents a CP2112 USB IO Expander
    /// </summary>
    public partial class Cp2112 :
        IDisposable,
        IDigitalInputOutputController,
        IDigitalOutputController,
        II2cController
    {
        private bool _isDisposed;

        private IntPtr _handle;
        private int _deviceNumber;
        private ushort _vid;
        private ushort _pid;
        private byte _direction;
        private byte _mode;
        private byte _function;
        private byte _clockDivisor;

        internal Cp2112(int deviceNumber, ushort vid, ushort pid)
        {
            _deviceNumber = deviceNumber;
            _vid = vid;
            _pid = pid;

            Pins = new PinDefinitions(this);
        }

        /// <summary>
        /// The pins
        /// </summary>
        public PinDefinitions Pins { get; }

        private bool IsOpen()
        {
            if (_handle == IntPtr.Zero) return false; ;

            int isOpen = 0;
            CheckStatus(Functions.HidSmbus_IsOpened(_handle, ref isOpen));

            return isOpen != 0;
        }

        private void Open()
        {
            if (IsOpen()) return;

            CheckStatus(Functions.HidSmbus_Open(ref _handle, _deviceNumber, _vid, _pid));
            CheckStatus(Functions.HidSmbus_GetGpioConfig(_handle, ref _direction, ref _mode, ref _function, ref _clockDivisor));
        }

        private void Close()
        {
            if (!IsOpen()) return;

            CheckStatus(Native.Functions.HidSmbus_Close(_handle));
            _handle = IntPtr.Zero;
        }

        public II2cBus CreateI2cBus(int busNumber = 0)
        {
            return CreateI2cBus(busNumber, I2cBusSpeed.Standard);
        }

        public II2cBus CreateI2cBus(IPin[] pins, I2cBusSpeed busSpeed)
        {
            return CreateI2cBus(0, busSpeed);
        }

        public II2cBus CreateI2cBus(IPin clock, IPin data, I2cBusSpeed busSpeed)
        {
            return CreateI2cBus(0, busSpeed);
        }


        public II2cBus CreateI2cBus(int busNumber, I2cBusSpeed busSpeed)
        {
            // TODO: only allow this once
            // TODO: convert frequency
            // TODO: lock out access to GPIO0,1,5 and 7

            //HID_SMBUS_STATUS HidSmbus_SetSmbusConfig(HID_SMBUS_DEVICE device,DWORD bitRate, BYTE address, BOOL autoReadRespond, WORD writeTimeout,WORD readTimeout, BOOL sclLowTimeout, WORD transferRetries)
            //            Functions.HidSmbus_SetSmbusConfig(_handle, 100000, 0x02, 0, 100, 100, 0, 2);
            //HID_SMBUS_STATUS HidSmbus_SetGpioConfig(HID_SMBUS_DEVICE device,BYTE direction, BYTE mode, BYTE special, BYTE clkDiv)
            //            Functions.HidSmbus_SetGpioConfig(_handle, 0x20, 0x20, 0x13, 0xFF);   //GPIO5 output/push-pull/GPIO0,1,7 special function/clkDiv=48MHz/(2x255)
            //HID_SMBUS_STATUS HidSmbus_WriteLatch(HID_SMBUS_DEVICE device,BYTE latchValue, BYTE latchMask)
            //            Functions.HidSmbus_WriteLatch(_handle, 0, 0x20);     //"Low" active for GPIO5

            Open();

            return new Cp2112I2cBus(this, busSpeed);
        }

        internal void I2CWrite(byte peripheralAddress, Span<byte> writeBuffer)
        {
            CheckStatus(Functions.HidSmbus_WriteRequest(_handle, peripheralAddress, writeBuffer.ToArray(), (byte)writeBuffer.Length));
        }

        internal void SetState(byte pinMask)
        {
            CheckStatus(Functions.HidSmbus_WriteLatch(_handle, pinMask, pinMask));
        }

        internal void ClearState(byte pinMask)
        {
            CheckStatus(Functions.HidSmbus_WriteLatch(_handle, (byte)~pinMask, pinMask));
        }

        public IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode)
        {
            throw new NotImplementedException();
        }

        public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
        {
            // TODO: check if pin is in use already
            Open();

            var d = _direction;
            d |= (byte)pin.Key;

            var mode = _mode;
            if (initialOutputType == OutputType.PushPull)
            {
                mode |= (byte)pin.Key;
            }

            CheckStatus(Functions.HidSmbus_SetGpioConfig(_handle, d, mode, _function, _clockDivisor));
            _direction = d;
            _mode = mode;
            //            _stateMask |= (byte)pin.Key;

            return new Cp2112DigitalOutputPort(pin, pin.SupportedChannels.First(c => c is DigitalChannelInfo) as IDigitalChannelInfo, initialState, initialOutputType, this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                Close();

                _isDisposed = true;
            }
        }

        ~Cp2112()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
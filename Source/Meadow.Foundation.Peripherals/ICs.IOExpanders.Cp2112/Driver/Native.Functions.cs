using System;
using System.Runtime.InteropServices;

namespace Meadow.Foundation.ICs.IOExpanders
{
    internal static partial class Native
    {

        public class Functions
        {
            private const string HIDtoSMB = "SLABHIDtoSMBus";

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_GetNumDevices(ref uint numDevices, ushort vid, ushort pid);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_Open(ref IntPtr device, int deviceNum, ushort vid, ushort pid);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_Close(IntPtr device);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_IsOpened(IntPtr device, ref int opened);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_GetGpioConfig(IntPtr device, ref byte direction, ref byte mode, ref byte function, ref byte clockDivisor);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_SetGpioConfig(IntPtr device, byte direction, byte mode, byte function, byte clockDivisor);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_ReadLatch(IntPtr device, out byte value);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_WriteLatch(IntPtr device, byte value, byte mask);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_ReadRequest(IntPtr device, byte slaveAddress, byte numBytesToRead);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_GetReadResponse(IntPtr device, byte statusS0, byte[] buffer, byte bufferSize, out byte bytesRead);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_AddressReadRequest(IntPtr device, byte slaveAddress, short numBytesToRead, byte targetAddressSize, byte[] targetAddress);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_GetSmbusConfig(IntPtr device, out uint bitRate, out byte address, out int autoReadRespond, out short writeTimeout, out short readTimeout, out int sclLowtimeout, out short transferRetries);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_SetSmbusConfig(IntPtr device, uint bitRate, byte address, int autoReadRespond, short writeTimeout, short readTimeout, int sclLowTimeout, short transferRetries);

            // HID_SMBUS_STATUS HidSmbus_WriteRequest (HID_SMBUS_DEVICE device, BYTE slaveAddress, BYTE* buffer, BYTE numBytesToWrite)
            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_WriteRequest(IntPtr device, byte slaveAddress, byte[] buffer, byte numBytes);
        }
    }
}
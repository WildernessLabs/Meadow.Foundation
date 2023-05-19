using System;
using System.Runtime.InteropServices;
using System.Text;

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
            public static extern HID_SMBUS_STATUS HidSmbus_GetString(uint deviceNum, ushort vid, ushort pid, StringBuilder deviceString, uint options);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_GetOpenedString(IntPtr device, StringBuilder deviceString, uint options);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_GetIndexedString(uint deviceNum, ushort vid, ushort pid, uint stringIndex, StringBuilder deviceString);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_GetOpenedIndexedString(IntPtr device, uint stringIndex, StringBuilder deviceString);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_GetAttributes(uint deviceNum, ushort vid, ushort pid, ref ushort deviceVid, ref ushort devicePid, ref ushort deviceReleaseNumber);

            [DllImport(HIDtoSMB)]
            public static extern HID_SMBUS_STATUS HidSmbus_GetOpenedAttributes(IntPtr device, ref ushort deviceVid, ref ushort devicePid, ref ushort deviceReleaseNumber);

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
        }
    }
}
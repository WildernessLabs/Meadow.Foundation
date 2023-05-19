using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    internal static partial class Native
    {
        public static bool CheckStatus(Native.HID_SMBUS_STATUS status)
        {
            if (status == Native.HID_SMBUS_STATUS.HID_SMBUS_SUCCESS)
            {
                return true;
            }

            throw new Exception($"Native error: {status}");
        }

        public enum HID_SMBUS_STATUS
        {
            HID_SMBUS_SUCCESS = 0x00,
            HID_SMBUS_DEVICE_NOT_FOUND = 0x01,
            HID_SMBUS_INVALID_HANDLE = 0x02,
            HID_SMBUS_INVALID_DEVICE_OBJECT = 0x03,
            HID_SMBUS_INVALID_PARAMETER = 0x04,
            HID_SMBUS_INVALID_REQUEST_LENGTH = 0x05,
            HID_SMBUS_READ_ERROR = 0x10,
            HID_SMBUS_WRITE_ERROR = 0x11,
            HID_SMBUS_READ_TIMED_OUT = 0x12,
            HID_SMBUS_WRITE_TIMED_OUT = 0x13,
            HID_SMBUS_DEVICE_IO_FAILED = 0x14,
            HID_SMBUS_DEVICE_ACCESS_ERROR = 0x15,
            HID_SMBUS_DEVICE_NOT_SUPPORTED = 0x16,
            HID_SMBUS_UNKNOWN_ERROR = 0xFF,
        }

        public static class UsbParameters
        {
            public const ushort SG_VID = 0x10C4;
            public const ushort CP2112_PID = 0xEA90;
        }

        public const byte HID_SMBUS_S0_IDLE = 0x00;
        public const byte HID_SMBUS_S0_BUSY = 0x01;
        public const byte HID_SMBUS_S0_COMPLETE = 0x02;
        public const byte HID_SMBUS_S0_ERROR = 0x03;

        // HID_SMBUS_TRANSFER_S0 = HID_SMBUS_S0_BUSY
        public const byte HID_SMBUS_S1_BUSY_ADDRESS_ACKED = 0x00;
        public const byte HID_SMBUS_S1_BUSY_ADDRESS_NACKED = 0x01;
        public const byte HID_SMBUS_S1_BUSY_READING = 0x02;
        public const byte HID_SMBUS_S1_BUSY_WRITING = 0x03;

        // HID_SMBUS_TRANSFER_S0 = HID_SMBUS_S0_ERROR
        public const byte HID_SMBUS_S1_ERROR_TIMEOUT_NACK = 0x00;
        public const byte HID_SMBUS_S1_ERROR_TIMEOUT_BUS_NOT_FREE = 0x01;
        public const byte HID_SMBUS_S1_ERROR_ARB_LOST = 0x02;
        public const byte HID_SMBUS_S1_ERROR_READ_INCOMPLETE = 0x03;
        public const byte HID_SMBUS_S1_ERROR_WRITE_INCOMPLETE = 0x04;
        public const byte HID_SMBUS_S1_ERROR_SUCCESS_AFTER_RETRY = 0x05;
    }
}
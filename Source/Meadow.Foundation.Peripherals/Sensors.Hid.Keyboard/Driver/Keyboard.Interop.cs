using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Meadow.Foundation.Sensors.Hid;

public partial class Keyboard
{
    internal class Interop
    {
        [Flags]
        internal enum DosDefineFlags
        {
            DDD_RAW_TARGET_PATH = 1,
            DDD_REMOVE_DEFINITION = 2,
            DDD_EXACT_MATCH_ON_REMOVE = 4,
            DDD_NO_BROADCAST_SYSTEM = 8
        }

        [Flags]
        public enum Indicators : ushort
        {
            KEYBOARD_KANA_LOCK_ON = 8,
            KEYBOARD_CAPS_LOCK_ON = 4,
            KEYBOARD_NUM_LOCK_ON = 2,
            KEYBOARD_SCROLL_LOCK_ON = 1
        }

        public struct KEYBOARD_INDICATOR_PARAMETERS
        {
            public ushort UnitId;
            public Indicators LedFlags;
        }

        // IOCTL_KEYBOARD_QUERY_INDICATORS      CTL_CODE(FILE_DEVICE_KEYBOARD, 0x0010, METHOD_BUFFERED, FILE_ANY_ACCESS)
        // IOCTL_KEYBOARD_SET_INDICATORS        CTL_CODE(FILE_DEVICE_KEYBOARD, 0x0002, METHOD_BUFFERED, FILE_ANY_ACCESS)        
        // FILE_DEVICE_KEYBOARD   0x0000000b
        // METHOD_BUFFERED   0
        // FILE_ANY_ACCESS   0
        // CTL_CODE( DeviceType, Function, Method, Access ) (((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method)
        // (0x0b << 16) | (( 0 ) << 14) | (0x0010 << 2) | (0)
        public const int IOCTL_KEYBOARD_QUERY_INDICATORS = (0x0b << 16) | ((0) << 14) | (0x0010 << 2) | (0);
        public const int IOCTL_KEYBOARD_SET_INDICATORS = (0x0b << 16) | ((0) << 14) | (0x0002 << 2) | (0);

        private const string USER32 = "user32.dll";
        private const string KERNEL32 = "kernel32.dll";

        [DllImport(USER32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DefineDosDeviceW(DosDefineFlags dwFlags,
            string lpDeviceName, string? lpTargetPath);
        /*
        BOOL DefineDosDeviceW(
          [in] DWORD dwFlags,
          [in] LPCWSTR lpDeviceName,
          [in, optional] LPCWSTR lpTargetPath
        );
        */

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
             [MarshalAs(UnmanagedType.LPTStr)] string filename,
             [MarshalAs(UnmanagedType.U4)] FileAccess access,
             [MarshalAs(UnmanagedType.U4)] FileShare share,
             IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
             [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
             [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
             IntPtr templateFile);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice, uint dwIoControlCode,
            ref KEYBOARD_INDICATOR_PARAMETERS lpInBuffer, int nInBufferSize,
            ref KEYBOARD_INDICATOR_PARAMETERS lpOutBuffer, int nOutBufferSize,
            out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeviceIoControl(
            IntPtr hDevice, uint dwIoControlCode,
            ref KEYBOARD_INDICATOR_PARAMETERS lpInBuffer, int nInBufferSize,
            IntPtr lpOutBuffer, int nOutBufferSize,
            out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport(KERNEL32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);
    }
}


using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Meadow.Foundation.Sensors.Hid;

public partial class Keyboard
{
    internal class InteropMac
    {
        public enum CGEventSourceStateID : int
        {
            hidSystemState = 1
        }

        // CGEventFlags CGEventSourceFlagsState(CGEventSourceStateID stateID);
        [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
        public static extern long CGEventSourceFlagsState(CGEventSourceStateID stateID);

        // bool CGEventSourceKeyState(CGEventSourceStateID stateID, CGKeyCode key);
        [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
        public static extern int CGEventSourceKeyState(CGEventSourceStateID stateID, MacKeyCodes keyCode);

        public enum MacKeyCodes : ushort
        {
            kVK_ANSI_A = 0x00,
            kVK_ANSI_S = 0x01,
            kVK_ANSI_D = 0x02,
            kVK_ANSI_F = 0x03,
            kVK_ANSI_H = 0x04,
            kVK_ANSI_G = 0x05,
            kVK_ANSI_Z = 0x06,
            kVK_ANSI_X = 0x07,
            kVK_ANSI_C = 0x08,
            kVK_ANSI_V = 0x09,
            kVK_ANSI_B = 0x0B,
            kVK_ANSI_Q = 0x0C,
            kVK_ANSI_W = 0x0D,
            kVK_ANSI_E = 0x0E,
            kVK_ANSI_R = 0x0F,
            kVK_ANSI_Y = 0x10,
            kVK_ANSI_T = 0x11,
            kVK_ANSI_1 = 0x12,
            kVK_ANSI_2 = 0x13,
            kVK_ANSI_3 = 0x14,
            kVK_ANSI_4 = 0x15,
            kVK_ANSI_6 = 0x16,
            kVK_ANSI_5 = 0x17,
            kVK_ANSI_Equal = 0x18,
            kVK_ANSI_9 = 0x19,
            kVK_ANSI_7 = 0x1A,
            kVK_ANSI_Minus = 0x1B,
            kVK_ANSI_8 = 0x1C,
            kVK_ANSI_0 = 0x1D,
            kVK_ANSI_RightBracket = 0x1E,
            kVK_ANSI_O = 0x1F,
            kVK_ANSI_U = 0x20,
            kVK_ANSI_LeftBracket = 0x21,
            kVK_ANSI_I = 0x22,
            kVK_ANSI_P = 0x23,
            kVK_ANSI_L = 0x25,
            kVK_ANSI_J = 0x26,
            kVK_ANSI_Quote = 0x27,
            kVK_ANSI_K = 0x28,
            kVK_ANSI_Semicolon = 0x29,
            kVK_ANSI_Backslash = 0x2A,
            kVK_ANSI_Comma = 0x2B,
            kVK_ANSI_Slash = 0x2C,
            kVK_ANSI_N = 0x2D,
            kVK_ANSI_M = 0x2E,
            kVK_ANSI_Period = 0x2F,
            kVK_ANSI_Grave = 0x32,
            kVK_ANSI_KeypadDecimal = 0x41,
            kVK_ANSI_KeypadMultiply = 0x43,
            kVK_ANSI_KeypadPlus = 0x45,
            kVK_ANSI_KeypadClear = 0x47,
            kVK_ANSI_KeypadDivide = 0x4B,
            kVK_ANSI_KeypadEnter = 0x4C,
            kVK_ANSI_KeypadMinus = 0x4E,
            kVK_ANSI_KeypadEquals = 0x51,
            kVK_ANSI_Keypad0 = 0x52,
            kVK_ANSI_Keypad1 = 0x53,
            kVK_ANSI_Keypad2 = 0x54,
            kVK_ANSI_Keypad3 = 0x55,
            kVK_ANSI_Keypad4 = 0x56,
            kVK_ANSI_Keypad5 = 0x57,
            kVK_ANSI_Keypad6 = 0x58,
            kVK_ANSI_Keypad7 = 0x59,
            kVK_ANSI_Keypad8 = 0x5B,
            kVK_ANSI_Keypad9 = 0x5C,
            kVK_Return = 0x24,
            kVK_Tab = 0x30,
            kVK_Space = 0x31,
            kVK_Delete = 0x33,
            kVK_Escape = 0x35,
            kVK_Command = 0x37,
            kVK_Shift = 0x38,
            kVK_CapsLock = 0x39,
            kVK_Option = 0x3A,
            kVK_Control = 0x3B,
            kVK_RightShift = 0x3C,
            kVK_RightOption = 0x3D,
            kVK_RightControl = 0x3E,
            kVK_Function = 0x3F,
            kVK_F17 = 0x40,
            kVK_VolumeUp = 0x48,
            kVK_VolumeDown = 0x49,
            kVK_Mute = 0x4A,
            kVK_F18 = 0x4F,
            kVK_F19 = 0x50,
            kVK_F20 = 0x5A,
            kVK_F5 = 0x60,
            kVK_F6 = 0x61,
            kVK_F7 = 0x62,
            kVK_F3 = 0x63,
            kVK_F8 = 0x64,
            kVK_F9 = 0x65,
            kVK_F11 = 0x67,
            kVK_F13 = 0x69,
            kVK_F16 = 0x6A,
            kVK_F14 = 0x6B,
            kVK_F10 = 0x6D,
            kVK_F12 = 0x6F,
            kVK_F15 = 0x71,
            kVK_Help = 0x72,
            kVK_Home = 0x73,
            kVK_PageUp = 0x74,
            kVK_ForwardDelete = 0x75,
            kVK_F4 = 0x76,
            kVK_End = 0x77,
            kVK_F2 = 0x78,
            kVK_PageDown = 0x79,
            kVK_F1 = 0x7A,
            kVK_LeftArrow = 0x7B,
            kVK_RightArrow = 0x7C,
            kVK_DownArrow = 0x7D,
            kVK_UpArrow = 0x7E
        }
    }

    internal class InteropWindows
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


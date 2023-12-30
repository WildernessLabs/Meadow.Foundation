using System;
using System.Runtime.InteropServices;
using System.Threading;
using static Meadow.Foundation.Sensors.Hid.Keyboard.InteropWindows;

namespace Meadow.Foundation.Sensors.Hid;

public partial class Keyboard
{
    private void WindowsKeyScanner()
    {
        _keepScanning = true;

        // if you're wondering why this method, we cannot use a keyboard hook because we don't have a message pump
        while (_keepScanning)
        {
            foreach (var key in _keys)
            {
                var state = InteropWindows.GetAsyncKeyState(key.Key);

                if ((state & 0x8000) != 0)
                {
                    // key is currently down
                    key.Value.SetState(true);
                }
                else if ((state & 0x0001) != 0)
                {
                    // state was down since last  call (is now up)
                    key.Value.SetState(true);
                    key.Value.SetState(false);
                }
                else
                {
                    key.Value.SetState(false);
                }

            }
            Thread.Sleep(10);
        }
    }


    private void OpenKeyboardDeviceWindows()
    {
        if (!InteropWindows.DefineDosDeviceW(
            InteropWindows.DosDefineFlags.DDD_RAW_TARGET_PATH,
            KeyboardDeviceName,
            $"\\Device\\KeyboardClass{_keyboardNumber}"))
        {
            var e = Marshal.GetLastPInvokeError();
            throw new NativeException($"Unable to define native keyboard device (Error {e})");
        }

        var handle = InteropWindows.CreateFile(
            @"\\.\Kbd",
            System.IO.FileAccess.Write,
            System.IO.FileShare.ReadWrite,
            IntPtr.Zero,
            System.IO.FileMode.Open,
            0,
            IntPtr.Zero);

        if (handle == IntPtr.Zero || handle == new IntPtr(-1))
        {
            var e = Marshal.GetLastPInvokeError();
            throw new NativeException($"Unable to open keyboard device (Error {e})");
        }

        _deviceHandle = handle;
    }

    private void CloseKeyboardDeviceWindows()
    {
        if (_deviceHandle != null)
        {
            if (!InteropWindows.DefineDosDeviceW(
                InteropWindows.DosDefineFlags.DDD_REMOVE_DEFINITION,
                KeyboardDeviceName,
                null))
            {
                // TODO: log this?
                var e = Marshal.GetLastPInvokeError();
            }

            InteropWindows.CloseHandle(_deviceHandle.Value);
            _deviceHandle = null;
        }
    }

    private bool GetIndicatorStateWindows(Indicators indicator)
    {
        var input = new KEYBOARD_INDICATOR_PARAMETERS();
        var output = new KEYBOARD_INDICATOR_PARAMETERS();

        if (_deviceHandle == null) return false;

        if (!InteropWindows.DeviceIoControl(
            _deviceHandle.Value,
            IOCTL_KEYBOARD_QUERY_INDICATORS,
            ref input,
            Marshal.SizeOf(input),
            ref output,
            Marshal.SizeOf(output),
            out uint returned,
            IntPtr.Zero))
        {
            var e = Marshal.GetLastPInvokeError();
            throw new NativeException("Unable to query keyboard indicator", e);
        }

        return (output.LedFlags & indicator) != 0;
    }

    private void SetIndicatorStateWindows(Indicators indicator, bool state)
    {
        if (_deviceHandle == null) return;

        var input = new KEYBOARD_INDICATOR_PARAMETERS();
        var output = new KEYBOARD_INDICATOR_PARAMETERS();

        // read current state
        if (!InteropWindows.DeviceIoControl(
            _deviceHandle.Value,
            IOCTL_KEYBOARD_QUERY_INDICATORS,
            ref input,
            Marshal.SizeOf(input),
            ref output,
            Marshal.SizeOf(output),
            out uint returned,
            IntPtr.Zero))
        {
            var e = Marshal.GetLastPInvokeError();
            throw new NativeException("Unable to query keyboard indicator", e);
        }

        if (state)
        {
            output.LedFlags |= indicator;
        }
        else
        {
            output.LedFlags &= ~indicator;
        }

        // toggle
        if (!InteropWindows.DeviceIoControl(
            _deviceHandle.Value,
            IOCTL_KEYBOARD_SET_INDICATORS,
            ref output,
            Marshal.SizeOf(output),
            IntPtr.Zero,
            0,
            out returned,
            IntPtr.Zero))
        {
            var e = Marshal.GetLastPInvokeError();
            throw new NativeException("Unable to set keyboard indicator", e);
        }
    }
}

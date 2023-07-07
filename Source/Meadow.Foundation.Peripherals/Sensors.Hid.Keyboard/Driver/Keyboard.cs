using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static Meadow.Foundation.Sensors.Hid.Keyboard.Interop;

namespace Meadow.Foundation.Sensors.Hid;

/// <summary>
/// Encapsulates a standard 108-key keyboard as a Meadow IO Extender
/// </summary>
public partial class Keyboard : IDigitalInterruptController, IDigitalOutputController, IDisposable
{
    private static Thread? _thread = null;
    private static Dictionary<char, KeyboardKey> _keys = new Dictionary<char, KeyboardKey>();

    private bool _keepScanning = false;
    private bool _isDisposed = false;
    private int _keyboardNumber;
    private IntPtr? _deviceHandle;
    private const string KeyboardDeviceName = "Kbd";

    /// <summary>
    /// Creates a Keyboard instance
    /// </summary>
    /// <param name="keyboardNumber"></param>
    public Keyboard(int keyboardNumber = 0)
    {
        _keyboardNumber = keyboardNumber;
        Pins = new PinDefinitions(this);
    }

    private void Install()
    {
        if (_thread != null) return;

        _thread = new Thread(KeyScanner);
        _thread.Start();
    }

    private void KeyScanner()
    {
        _keepScanning = true;

        // if you're wondering why this method, we cannot use a keyboard hook because we don't have a message pump
        while (_keepScanning)
        {
            foreach (var key in _keys)
            {
                var state = Interop.GetAsyncKeyState(key.Key);

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

    /// <summary>
    /// Creates an input for a keyboard key
    /// </summary>
    /// <param name="pin"></param>
    public IDigitalInterruptPort CreateDigitalInterruptPort(IPin pin)
    {
        return CreateDigitalInterruptPort(pin, InterruptMode.None);
    }

    /// <summary>
    /// Creates an input for a keyboard key
    /// </summary>
    /// <param name="pin"></param>
    /// <param name="interruptMode"></param>
    public IDigitalInterruptPort CreateDigitalInterruptPort(IPin pin, InterruptMode interruptMode)
    {
        return CreateDigitalInterruptPort(pin, interruptMode, ResistorMode.Disabled, TimeSpan.Zero, TimeSpan.Zero);
    }

    /// <summary>
    /// Creates an input for a keyboard key
    /// </summary>
    /// <param name="pin"></param>
    /// <param name="interruptMode"></param>
    /// <param name="resistorMode"></param>
    /// <param name="debounceDuration"></param>
    /// <param name="glitchDuration"></param>
    public IDigitalInterruptPort CreateDigitalInterruptPort(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, TimeSpan debounceDuration, TimeSpan glitchDuration)
    {
        var kp = pin as KeyboardKeyPin;

        if (kp == null)
        {
            throw new ArgumentException("Input Pin must be a KeyboardKeyPin");
        }

        var key = char.ToUpper(kp.Key);
        if (_keys.ContainsKey(key)) throw new Exception("Key is already hooked");

        Install();

        var ci = kp.SupportedChannels?.First() as IDigitalChannelInfo ?? throw new ArgumentException("Pin is not a Digital channel");

        var kbk = new KeyboardKey(kp, ci, interruptMode);
        _keys.Add(key, kbk);
        return kbk;
    }

    /// <summary>d
    /// The pins
    /// </summary>
    public PinDefinitions Pins { get; }

    /// <summary>
    /// Releases resources created by the Keyboard instance
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                CloseKeyboardDevice();
                _keepScanning = false;
            }

            _isDisposed = true;
        }
    }

    /// <summary>
    /// Releases resources created by the Keyboard instance
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Creates an output for a Keyboard indicator
    /// </summary>
    /// <param name="pin"></param>
    /// <param name="initialState"></param>
    /// <param name="initialOutputType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        var kp = pin as KeyboardIndicatorPin;

        if (kp == null)
        {
            throw new ArgumentException("Output Pin must be a KeyboardIndicatorPin");
        }

        if (_deviceHandle == null)
        {
            OpenKeyboardDevice();
        }

        var ci = kp.SupportedChannels?.First() as IDigitalChannelInfo ?? throw new ArgumentException("Pin is not a Digital channel");

        return new KeyboardIndicator(pin, ci, initialState ? true : null);
        throw new NotImplementedException();
    }

    private void OpenKeyboardDevice()
    {
        if (!Interop.DefineDosDeviceW(
            Interop.DosDefineFlags.DDD_RAW_TARGET_PATH,
            KeyboardDeviceName,
            $"\\Device\\KeyboardClass{_keyboardNumber}"))
        {
            var e = Marshal.GetLastPInvokeError();
            throw new NativeException($"Unable to define native keyboard device (Error {e})");
        }

        var handle = Interop.CreateFile(
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

    private void CloseKeyboardDevice()
    {
        if (_deviceHandle != null)
        {
            if (!Interop.DefineDosDeviceW(
                Interop.DosDefineFlags.DDD_REMOVE_DEFINITION,
                KeyboardDeviceName,
                null))
            {
                // TODO: log this?
                var e = Marshal.GetLastPInvokeError();
            }

            Interop.CloseHandle(_deviceHandle.Value);
            _deviceHandle = null;
        }
    }

    private bool GetIndicatorState(Indicators indicator)
    {
        var input = new KEYBOARD_INDICATOR_PARAMETERS();
        var output = new KEYBOARD_INDICATOR_PARAMETERS();

        if (_deviceHandle == null) return false;

        if (!Interop.DeviceIoControl(
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

    private void SetIndicatorState(Indicators indicator, bool state)
    {
        if (_deviceHandle == null) return;

        var input = new KEYBOARD_INDICATOR_PARAMETERS();
        var output = new KEYBOARD_INDICATOR_PARAMETERS();

        // read current state
        if (!Interop.DeviceIoControl(
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
        if (!Interop.DeviceIoControl(
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

using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        if (OperatingSystem.IsMacOS())
        {
            _thread = new Thread(MacKeyScanner);
        }
        else if (OperatingSystem.IsWindows())
        {
            _thread = new Thread(WindowsKeyScanner);
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        _thread.Start();
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
    /// Dispose of the object
    /// </summary>
    /// <param name="disposing">Is disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                if (OperatingSystem.IsWindows())
                {
                    CloseKeyboardDeviceWindows();
                }
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
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException();
        }

        var kp = pin as KeyboardIndicatorPin;

        if (kp == null)
        {
            throw new ArgumentException("Output Pin must be a KeyboardIndicatorPin");
        }

        if (_deviceHandle == null)
        {
            OpenKeyboardDeviceWindows();
        }

        var ci = kp.SupportedChannels?.First() as IDigitalChannelInfo ?? throw new ArgumentException("Pin is not a Digital channel");

        return new KeyboardIndicator(pin, ci, initialState ? true : null);
    }
}

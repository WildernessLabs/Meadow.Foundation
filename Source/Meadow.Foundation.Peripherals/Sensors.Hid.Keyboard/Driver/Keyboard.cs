using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.Sensors.Hid;

public partial class Keyboard : IDigitalInputController, IDisposable
{
    private static Thread? _thread = null;
    private static Dictionary<char, KeyboardKey> _keys = new Dictionary<char, KeyboardKey>();

    private bool _keepScanning = false;
    private bool _isDisposed = false;

    public Keyboard()
    {
        Pins.Controller = this;
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

                if (((int)state & 0x8000) != 0)
                {
                    // key is currently down
                    key.Value.SetState(true);
                }
                else if (((int)state & 0x0001) != 0)
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

    public IDigitalInputPort CreateDigitalInputPort(IPin pin)
    {
        return CreateDigitalInputPort(pin, InterruptMode.None);
    }

    public IDigitalInputPort CreateDigitalInputPort(IPin pin, InterruptMode interruptMode)
    {
        return CreateDigitalInputPort(pin, interruptMode, ResistorMode.Disabled, TimeSpan.Zero, TimeSpan.Zero);
    }

    public IDigitalInputPort CreateDigitalInputPort(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, TimeSpan debounceDuration, TimeSpan glitchDuration)
    {
        var kp = pin as KeyboardPin;

        if (kp == null)
        {
            throw new ArgumentException("Pin must be a KeyboardPin");
        }

        var key = char.ToUpper(kp.Key);
        if (_keys.ContainsKey(key)) throw new Exception("Key is already hooked");

        Install();

        var kbk = new KeyboardKey(kp, kp.SupportedChannels.First() as IDigitalChannelInfo, interruptMode);
        _keys.Add(key, kbk);
        return kbk;
    }

    /// <summary>d
    /// The pins
    /// </summary>
    public PinDefinitions Pins { get; } = new PinDefinitions();

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _keepScanning = false;
            }

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

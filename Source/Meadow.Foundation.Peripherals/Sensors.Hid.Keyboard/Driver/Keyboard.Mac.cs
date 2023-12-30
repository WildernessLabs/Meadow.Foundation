using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Hid;

public partial class Keyboard : IDigitalInterruptController, IDigitalOutputController, IDisposable
{
    private void MacKeyScanner()
    {
        _keepScanning = true;

        // if you're wondering why this method, we cannot use a keyboard hook because we don't have a message pump
        while (_keepScanning)
        {
            foreach (var key in _keys)
            {
                var keycode = (key.Value.Pin as KeyboardKeyPin)?.MacKeyCode;
                if (keycode != null)
                {
                    var state = InteropMac.CGEventSourceKeyState(
                        InteropMac.CGEventSourceStateID.hidSystemState,
                        keycode.Value) != 0;

                    key.Value.SetState(state);
                }

            }
            Thread.Sleep(10);
        }
    }
}

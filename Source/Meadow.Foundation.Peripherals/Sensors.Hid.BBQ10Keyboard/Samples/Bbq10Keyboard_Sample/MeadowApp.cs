using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System.Threading.Tasks;

namespace Bbq10Keyboard_Sample;

public class MeadowApp : App<F7FeatherV2>
{
    BBQ10Keyboard keyboard = default!;

    //<!=SNIP=>

    public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        var i2cBus = Device.CreateI2cBus(0);

        // Interrupt-driven mode: pass the interrupt pin and key events
        // fire automatically via the OnKeyEvent event.
        keyboard = new BBQ10Keyboard(i2cBus, Device.Pins.D10);

        // Without an interrupt pin, use polling instead:
        // keyboard = new BBQ10Keyboard(i2cBus);
        // keyboard.StartPolling(intervalMs: 50);

        keyboard.OnKeyEvent += Keyboard_OnKeyEvent;

        return Task.CompletedTask;
    }

    private void Keyboard_OnKeyEvent(object? sender, BBQ10Keyboard.KeyEvent e)
    {
        if (e.KeyState == BBQ10Keyboard.KeyState.StatePress)
        {
            Resolver.Log.Info($"Key pressed: '{e.AsciiValue}' (0x{(byte)e.AsciiValue:X2})");
        }
        else if (e.KeyState == BBQ10Keyboard.KeyState.StateRelease)
        {
            Resolver.Log.Info($"Key released: '{e.AsciiValue}'");
        }
    }

    //<!=SNOP=>
}

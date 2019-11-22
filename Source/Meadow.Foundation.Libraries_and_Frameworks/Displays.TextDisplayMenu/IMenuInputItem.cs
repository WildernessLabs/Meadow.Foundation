using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Sensors.Buttons;
using Meadow.Peripherals.Sensors.Rotary;

namespace Meadow.Foundation.Displays.TextDisplayMenu
{
    public interface IMenuInputItem
    {
        void Init(ITextDisplay display, IRotaryEncoder encoder, IButton buttonSelect);
        void Init(ITextDisplay display, IButton buttonNext, IButton buttonPrevious, IButton buttonSelect);
        void GetInput(string itemID, object currentValue);
        event ValueChangedHandler ValueChanged;
    }
}
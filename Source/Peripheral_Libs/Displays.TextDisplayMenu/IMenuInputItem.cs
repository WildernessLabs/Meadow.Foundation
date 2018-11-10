using System;

using Meadow.Foundation.Sensors.Rotary;
using static Meadow.Foundation.Displays.TextDisplayMenu.InputTypes.Time;
using Meadow.Foundation.Sensors.Buttons;

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

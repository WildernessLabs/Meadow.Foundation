using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.Sensors.Hid;
public partial class Keyboard
{
    /// <summary>
    /// The collection of IPins that a keyboard can use
    /// </summary>
    public class PinDefinitions : IPinDefinitions
    {
        private List<IPin> _keys = new List<IPin>();

        /// <summary>
        /// Enumerates all pins in the keyboard
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// A list of all pins in the keyboard
        /// </summary>
        public IList<IPin> AllPins => _keys;

        /// <summary>
        /// The Keyboard associated with the pin collection
        /// </summary>
        public IPinController Controller { get; set; }

        internal PinDefinitions(Keyboard controller)
        {
            Controller = controller;
            _keys.Add(A);
        }

        public IPin Back => new KeyboardKeyPin(Controller, "Back", '\u0008');
        public IPin Tab => new KeyboardKeyPin(Controller, "Tab", '\u0009');
        public IPin Enter => new KeyboardKeyPin(Controller, "Enter", '\u000d');
        public IPin Shift => new KeyboardKeyPin(Controller, "Shift", '\u0010');
        public IPin Control => new KeyboardKeyPin(Controller, "Control", '\u0011');
        public IPin Escape => new KeyboardKeyPin(Controller, "Escape", '\u001b');
        public IPin Space => new KeyboardKeyPin(Controller, "Space", '\u0020');
        public IPin PageUp => new KeyboardKeyPin(Controller, "PageUp", '\u0021');
        public IPin PageDown => new KeyboardKeyPin(Controller, "PageDown", '\u0022');
        public IPin End => new KeyboardKeyPin(Controller, "End", '\u0023');
        public IPin Home => new KeyboardKeyPin(Controller, "Home", '\u0024');
        public IPin Left => new KeyboardKeyPin(Controller, "Left", '\u0025');
        public IPin Up => new KeyboardKeyPin(Controller, "Up", '\u0026');
        public IPin Right => new KeyboardKeyPin(Controller, "Right", '\u0027');
        public IPin Down => new KeyboardKeyPin(Controller, "Down", '\u0028');
        public IPin Insert => new KeyboardKeyPin(Controller, "Insert", '\u002d');
        public IPin Delete => new KeyboardKeyPin(Controller, "Delete", '\u002e');
        public IPin Tilde => new KeyboardKeyPin(Controller, "Tilde", '\u00c0');

        public IPin Semicolon => new KeyboardKeyPin(Controller, "Semicolon", '\u00ba');
        public IPin Plus => new KeyboardKeyPin(Controller, "Plus", '\u00bb');
        public IPin Minus => new KeyboardKeyPin(Controller, "Minus", '\u00bd');
        public IPin Comma => new KeyboardKeyPin(Controller, "Comma", '\u00bc');
        public IPin Period => new KeyboardKeyPin(Controller, "Period", '\u00be');
        public IPin ForwardSlash => new KeyboardKeyPin(Controller, "ForwardSlash", '\u00bf');

        public IPin BackSlash => new KeyboardKeyPin(Controller, "BackSlash", '\u00dc');
        public IPin OpenBracket => new KeyboardKeyPin(Controller, "OpenBracket", '\u00db');
        public IPin CloseBracket => new KeyboardKeyPin(Controller, "CloseBracket", '\u00dd');

        public IPin Num0 => new KeyboardKeyPin(Controller, "0", '0');
        public IPin Num1 => new KeyboardKeyPin(Controller, "1", '1');
        public IPin Num2 => new KeyboardKeyPin(Controller, "2", '2');
        public IPin Num3 => new KeyboardKeyPin(Controller, "3", '3');
        public IPin Num4 => new KeyboardKeyPin(Controller, "4", '4');
        public IPin Num5 => new KeyboardKeyPin(Controller, "5", '5');
        public IPin Num6 => new KeyboardKeyPin(Controller, "6", '6');
        public IPin Num7 => new KeyboardKeyPin(Controller, "7", '7');
        public IPin Num8 => new KeyboardKeyPin(Controller, "8", '8');
        public IPin Num9 => new KeyboardKeyPin(Controller, "9", '9');

        public IPin A => new KeyboardKeyPin(Controller, "A", 'A');
        public IPin B => new KeyboardKeyPin(Controller, "B", 'B');
        public IPin C => new KeyboardKeyPin(Controller, "C", 'C');
        public IPin D => new KeyboardKeyPin(Controller, "D", 'D');
        public IPin E => new KeyboardKeyPin(Controller, "E", 'E');
        public IPin F => new KeyboardKeyPin(Controller, "F", 'F');
        public IPin G => new KeyboardKeyPin(Controller, "G", 'G');
        public IPin H => new KeyboardKeyPin(Controller, "H", 'H');
        public IPin I => new KeyboardKeyPin(Controller, "I", 'I');
        public IPin J => new KeyboardKeyPin(Controller, "J", 'J');
        public IPin K => new KeyboardKeyPin(Controller, "K", 'K');
        public IPin L => new KeyboardKeyPin(Controller, "L", 'L');
        public IPin M => new KeyboardKeyPin(Controller, "M", 'M');
        public IPin N => new KeyboardKeyPin(Controller, "N", 'N');
        public IPin O => new KeyboardKeyPin(Controller, "O", 'O');
        public IPin P => new KeyboardKeyPin(Controller, "P", 'P');
        public IPin Q => new KeyboardKeyPin(Controller, "Q", 'Q');
        public IPin R => new KeyboardKeyPin(Controller, "R", 'R');
        public IPin S => new KeyboardKeyPin(Controller, "S", 'S');
        public IPin T => new KeyboardKeyPin(Controller, "T", 'T');
        public IPin U => new KeyboardKeyPin(Controller, "U", 'U');
        public IPin V => new KeyboardKeyPin(Controller, "V", 'V');
        public IPin W => new KeyboardKeyPin(Controller, "W", 'W');
        public IPin X => new KeyboardKeyPin(Controller, "X", 'X');
        public IPin Y => new KeyboardKeyPin(Controller, "Y", 'Y');
        public IPin Z => new KeyboardKeyPin(Controller, "Z", 'Z');

        public IPin NumPad0 => new KeyboardKeyPin(Controller, "NumPad0", '\u0060');
        public IPin NumPad1 => new KeyboardKeyPin(Controller, "NumPad1", '\u0061');
        public IPin NumPad2 => new KeyboardKeyPin(Controller, "NumPad2", '\u0062');
        public IPin NumPad3 => new KeyboardKeyPin(Controller, "NumPad3", '\u0063');
        public IPin NumPad4 => new KeyboardKeyPin(Controller, "NumPad4", '\u0064');
        public IPin NumPad5 => new KeyboardKeyPin(Controller, "NumPad5", '\u0065');
        public IPin NumPad6 => new KeyboardKeyPin(Controller, "NumPad6", '\u0066');
        public IPin NumPad7 => new KeyboardKeyPin(Controller, "NumPad7", '\u0067');
        public IPin NumPad8 => new KeyboardKeyPin(Controller, "NumPad8", '\u0068');
        public IPin NumPad9 => new KeyboardKeyPin(Controller, "NumPad9", '\u0069');

        public IPin CapsLock => new KeyboardIndicatorPin(Controller, "CapsLock", Interop.Indicators.KEYBOARD_CAPS_LOCK_ON);
        public IPin NumLock => new KeyboardIndicatorPin(Controller, "NumLock", Interop.Indicators.KEYBOARD_NUM_LOCK_ON);
        public IPin ScrollLock => new KeyboardIndicatorPin(Controller, "ScrollLock", Interop.Indicators.KEYBOARD_SCROLL_LOCK_ON);
        public IPin KanaLock => new KeyboardIndicatorPin(Controller, "KanaLock", Interop.Indicators.KEYBOARD_KANA_LOCK_ON);
    }
}

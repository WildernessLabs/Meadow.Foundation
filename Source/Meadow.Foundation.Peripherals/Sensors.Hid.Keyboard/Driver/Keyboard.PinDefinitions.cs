using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.Sensors.Hid;
public partial class Keyboard
{
    public class PinDefinitions : IPinDefinitions
    {
        private List<IPin> _keys = new List<IPin>();

        public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IList<IPin> AllPins => _keys;

        public IPinController Controller { get; set; }

        internal PinDefinitions()
        {
            _keys.Add(A);
        }

        public IPin Back => new KeyboardPin(Controller, "Back", '\u0008');
        public IPin Tab => new KeyboardPin(Controller, "Tab", '\u0009');
        public IPin Enter => new KeyboardPin(Controller, "Enter", '\u000d');
        public IPin Shift => new KeyboardPin(Controller, "Shift", '\u0010');
        public IPin Control => new KeyboardPin(Controller, "Control", '\u0011');
        public IPin Escape => new KeyboardPin(Controller, "Escape", '\u001b');
        public IPin Space => new KeyboardPin(Controller, "Space", '\u0020');
        public IPin PageUp => new KeyboardPin(Controller, "PageUp", '\u0021');
        public IPin PageDown => new KeyboardPin(Controller, "PageDown", '\u0022');
        public IPin End => new KeyboardPin(Controller, "End", '\u0023');
        public IPin Home => new KeyboardPin(Controller, "Home", '\u0024');
        public IPin Left => new KeyboardPin(Controller, "Left", '\u0025');
        public IPin Up => new KeyboardPin(Controller, "Up", '\u0026');
        public IPin Right => new KeyboardPin(Controller, "Right", '\u0027');
        public IPin Down => new KeyboardPin(Controller, "Down", '\u0028');
        public IPin Insert => new KeyboardPin(Controller, "Insert", '\u002d');
        public IPin Delete => new KeyboardPin(Controller, "Delete", '\u002e');
        public IPin Tilde => new KeyboardPin(Controller, "Tilde", '\u00c0');

        public IPin Semicolon => new KeyboardPin(Controller, "Semicolon", '\u00ba');
        public IPin Plus => new KeyboardPin(Controller, "Plus", '\u00bb');
        public IPin Minus => new KeyboardPin(Controller, "Minus", '\u00bd');
        public IPin Comma => new KeyboardPin(Controller, "Comma", '\u00bc');
        public IPin Period => new KeyboardPin(Controller, "Period", '\u00be');
        public IPin ForwardSlash => new KeyboardPin(Controller, "ForwardSlash", '\u00bf');

        public IPin BackSlash => new KeyboardPin(Controller, "BackSlash", '\u00dc');
        public IPin OpenBracket => new KeyboardPin(Controller, "OpenBracket", '\u00db');
        public IPin CloseBracket => new KeyboardPin(Controller, "CloseBracket", '\u00dd');

        public IPin Num0 => new KeyboardPin(Controller, "0", '0');
        public IPin Num1 => new KeyboardPin(Controller, "1", '1');
        public IPin Num2 => new KeyboardPin(Controller, "2", '2');
        public IPin Num3 => new KeyboardPin(Controller, "3", '3');
        public IPin Num4 => new KeyboardPin(Controller, "4", '4');
        public IPin Num5 => new KeyboardPin(Controller, "5", '5');
        public IPin Num6 => new KeyboardPin(Controller, "6", '6');
        public IPin Num7 => new KeyboardPin(Controller, "7", '7');
        public IPin Num8 => new KeyboardPin(Controller, "8", '8');
        public IPin Num9 => new KeyboardPin(Controller, "9", '9');

        public IPin A => new KeyboardPin(Controller, "A", 'A');
        public IPin B => new KeyboardPin(Controller, "B", 'B');
        public IPin C => new KeyboardPin(Controller, "C", 'C');
        public IPin D => new KeyboardPin(Controller, "D", 'D');
        public IPin E => new KeyboardPin(Controller, "E", 'E');
        public IPin F => new KeyboardPin(Controller, "F", 'F');
        public IPin G => new KeyboardPin(Controller, "G", 'G');
        public IPin H => new KeyboardPin(Controller, "H", 'H');
        public IPin I => new KeyboardPin(Controller, "I", 'I');
        public IPin J => new KeyboardPin(Controller, "J", 'J');
        public IPin K => new KeyboardPin(Controller, "K", 'K');
        public IPin L => new KeyboardPin(Controller, "L", 'L');
        public IPin M => new KeyboardPin(Controller, "M", 'M');
        public IPin N => new KeyboardPin(Controller, "N", 'N');
        public IPin O => new KeyboardPin(Controller, "O", 'O');
        public IPin P => new KeyboardPin(Controller, "P", 'P');
        public IPin Q => new KeyboardPin(Controller, "Q", 'Q');
        public IPin R => new KeyboardPin(Controller, "R", 'R');
        public IPin S => new KeyboardPin(Controller, "S", 'S');
        public IPin T => new KeyboardPin(Controller, "T", 'T');
        public IPin U => new KeyboardPin(Controller, "U", 'U');
        public IPin V => new KeyboardPin(Controller, "V", 'V');
        public IPin W => new KeyboardPin(Controller, "W", 'W');
        public IPin X => new KeyboardPin(Controller, "X", 'X');
        public IPin Y => new KeyboardPin(Controller, "Y", 'Y');
        public IPin Z => new KeyboardPin(Controller, "Z", 'Z');

        public IPin NumPad0 => new KeyboardPin(Controller, "NumPad0", '\u0060');
        public IPin NumPad1 => new KeyboardPin(Controller, "NumPad1", '\u0061');
        public IPin NumPad2 => new KeyboardPin(Controller, "NumPad2", '\u0062');
        public IPin NumPad3 => new KeyboardPin(Controller, "NumPad3", '\u0063');
        public IPin NumPad4 => new KeyboardPin(Controller, "NumPad4", '\u0064');
        public IPin NumPad5 => new KeyboardPin(Controller, "NumPad5", '\u0065');
        public IPin NumPad6 => new KeyboardPin(Controller, "NumPad6", '\u0066');
        public IPin NumPad7 => new KeyboardPin(Controller, "NumPad7", '\u0067');
        public IPin NumPad8 => new KeyboardPin(Controller, "NumPad8", '\u0068');
        public IPin NumPad9 => new KeyboardPin(Controller, "NumPad9", '\u0069');
    }
}

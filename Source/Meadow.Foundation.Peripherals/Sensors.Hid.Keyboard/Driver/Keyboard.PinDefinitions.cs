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

        internal PinDefinitions()
        {
            _keys.Add(A);
        }

        public readonly IPin Back = new KeyboardPin("Back", '\u0008');
        public readonly IPin Tab = new KeyboardPin("Tab", '\u0009');
        public readonly IPin Enter = new KeyboardPin("Enter", '\u000d');
        public readonly IPin Shift = new KeyboardPin("Shift", '\u0010');
        public readonly IPin Control = new KeyboardPin("Control", '\u0011');
        public readonly IPin Escape = new KeyboardPin("Escape", '\u001b');
        public readonly IPin Space = new KeyboardPin("Space", '\u0020');
        public readonly IPin PageUp = new KeyboardPin("PageUp", '\u0021');
        public readonly IPin PageDown = new KeyboardPin("PageDown", '\u0022');
        public readonly IPin End = new KeyboardPin("End", '\u0023');
        public readonly IPin Home = new KeyboardPin("Home", '\u0024');
        public readonly IPin Left = new KeyboardPin("Left", '\u0025');
        public readonly IPin Up = new KeyboardPin("Up", '\u0026');
        public readonly IPin Right = new KeyboardPin("Right", '\u0027');
        public readonly IPin Down = new KeyboardPin("Down", '\u0028');
        public readonly IPin Insert = new KeyboardPin("Insert", '\u002d');
        public readonly IPin Delete = new KeyboardPin("Delete", '\u002e');
        public readonly IPin Tilde = new KeyboardPin("Tilde", '\u00c0');

        public readonly IPin Semicolon = new KeyboardPin("Semicolon", '\u00ba');
        public readonly IPin Plus = new KeyboardPin("Plus", '\u00bb');
        public readonly IPin Minus = new KeyboardPin("Minus", '\u00bd');
        public readonly IPin Comma = new KeyboardPin("Comma", '\u00bc');
        public readonly IPin Period = new KeyboardPin("Period", '\u00be');
        public readonly IPin ForwardSlash = new KeyboardPin("ForwardSlash", '\u00bf');

        public readonly IPin BackSlash = new KeyboardPin("BackSlash", '\u00dc');
        public readonly IPin OpenBracket = new KeyboardPin("OpenBracket", '\u00db');
        public readonly IPin CloseBracket = new KeyboardPin("CloseBracket", '\u00dd');

        public readonly IPin Num0 = new KeyboardPin("0", '0');
        public readonly IPin Num1 = new KeyboardPin("1", '1');
        public readonly IPin Num2 = new KeyboardPin("2", '2');
        public readonly IPin Num3 = new KeyboardPin("3", '3');
        public readonly IPin Num4 = new KeyboardPin("4", '4');
        public readonly IPin Num5 = new KeyboardPin("5", '5');
        public readonly IPin Num6 = new KeyboardPin("6", '6');
        public readonly IPin Num7 = new KeyboardPin("7", '7');
        public readonly IPin Num8 = new KeyboardPin("8", '8');
        public readonly IPin Num9 = new KeyboardPin("9", '9');

        public readonly IPin A = new KeyboardPin("A", 'A');
        public readonly IPin B = new KeyboardPin("B", 'B');
        public readonly IPin C = new KeyboardPin("C", 'C');
        public readonly IPin D = new KeyboardPin("D", 'D');
        public readonly IPin E = new KeyboardPin("E", 'E');
        public readonly IPin F = new KeyboardPin("F", 'F');
        public readonly IPin G = new KeyboardPin("G", 'G');
        public readonly IPin H = new KeyboardPin("H", 'H');
        public readonly IPin I = new KeyboardPin("I", 'I');
        public readonly IPin J = new KeyboardPin("J", 'J');
        public readonly IPin K = new KeyboardPin("K", 'K');
        public readonly IPin L = new KeyboardPin("L", 'L');
        public readonly IPin M = new KeyboardPin("M", 'M');
        public readonly IPin N = new KeyboardPin("N", 'N');
        public readonly IPin O = new KeyboardPin("O", 'O');
        public readonly IPin P = new KeyboardPin("P", 'P');
        public readonly IPin Q = new KeyboardPin("Q", 'Q');
        public readonly IPin R = new KeyboardPin("R", 'R');
        public readonly IPin S = new KeyboardPin("S", 'S');
        public readonly IPin T = new KeyboardPin("T", 'T');
        public readonly IPin U = new KeyboardPin("U", 'U');
        public readonly IPin V = new KeyboardPin("V", 'V');
        public readonly IPin W = new KeyboardPin("W", 'W');
        public readonly IPin X = new KeyboardPin("X", 'X');
        public readonly IPin Y = new KeyboardPin("Y", 'Y');
        public readonly IPin Z = new KeyboardPin("Z", 'Z');

        public readonly IPin NumPad0 = new KeyboardPin("NumPad0", '\u0060');
        public readonly IPin NumPad1 = new KeyboardPin("NumPad1", '\u0061');
        public readonly IPin NumPad2 = new KeyboardPin("NumPad2", '\u0062');
        public readonly IPin NumPad3 = new KeyboardPin("NumPad3", '\u0063');
        public readonly IPin NumPad4 = new KeyboardPin("NumPad4", '\u0064');
        public readonly IPin NumPad5 = new KeyboardPin("NumPad5", '\u0065');
        public readonly IPin NumPad6 = new KeyboardPin("NumPad6", '\u0066');
        public readonly IPin NumPad7 = new KeyboardPin("NumPad7", '\u0067');
        public readonly IPin NumPad8 = new KeyboardPin("NumPad8", '\u0068');
        public readonly IPin NumPad9 = new KeyboardPin("NumPad9", '\u0069');
    }
}

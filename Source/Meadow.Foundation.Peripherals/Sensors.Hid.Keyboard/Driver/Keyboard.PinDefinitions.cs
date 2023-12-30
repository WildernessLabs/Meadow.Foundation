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
        private readonly List<IPin> _keys = new List<IPin>();

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
        public IPinController? Controller { get; set; }

        internal PinDefinitions(Keyboard controller)
        {
            Controller = controller;
            _keys.Add(A);
        }

        /// <summary>
        /// An input Pin for the Back key
        /// </summary>
        public IPin Back => new KeyboardKeyPin(Controller, "Back", '\u0008', InteropMac.MacKeyCodes.kVK_Delete);
        /// <summary>
        /// An input Pin for the Tab key
        /// </summary>
        public IPin Tab => new KeyboardKeyPin(Controller, "Tab", '\u0009', InteropMac.MacKeyCodes.kVK_Tab);
        /// <summary>
        /// An input Pin for the Enter key
        /// </summary>
        public IPin Enter => new KeyboardKeyPin(Controller, "Enter", '\u000d', InteropMac.MacKeyCodes.kVK_Return);
        /// <summary>
        /// An input Pin for the Shift key
        /// </summary>
        public IPin Shift => new KeyboardKeyPin(Controller, "Shift", '\u0010', InteropMac.MacKeyCodes.kVK_Shift);
        /// <summary>
        /// An input Pin for the Control key
        /// </summary>
        public IPin Control => new KeyboardKeyPin(Controller, "Control", '\u0011', InteropMac.MacKeyCodes.kVK_Control);
        /// <summary>
        /// An input Pin for the Escape key
        /// </summary>
        public IPin Escape => new KeyboardKeyPin(Controller, "Escape", '\u001b', InteropMac.MacKeyCodes.kVK_Escape);
        /// <summary>
        /// An input Pin for the Space bar
        /// </summary>
        public IPin Space => new KeyboardKeyPin(Controller, "Space", '\u0020', InteropMac.MacKeyCodes.kVK_Space);
        /// <summary>
        /// An input Pin for the Page Up key
        /// </summary>
        public IPin PageUp => new KeyboardKeyPin(Controller, "PageUp", '\u0021', InteropMac.MacKeyCodes.kVK_PageUp);
        /// <summary>
        /// An input Pin for the Page Down key
        /// </summary>
        public IPin PageDown => new KeyboardKeyPin(Controller, "PageDown", '\u0022', InteropMac.MacKeyCodes.kVK_PageDown);
        /// <summary>
        /// An input Pin for the End key
        /// </summary>
        public IPin End => new KeyboardKeyPin(Controller, "End", '\u0023', InteropMac.MacKeyCodes.kVK_End);
        /// <summary>
        /// An input Pin for the Home key
        /// </summary>
        public IPin Home => new KeyboardKeyPin(Controller, "Home", '\u0024', InteropMac.MacKeyCodes.kVK_Home);
        /// <summary>
        /// An input Pin for the Left Arrow key
        /// </summary>
        public IPin Left => new KeyboardKeyPin(Controller, "Left", '\u0025', InteropMac.MacKeyCodes.kVK_LeftArrow);
        /// <summary>
        /// An input Pin for the Up Arrow key
        /// </summary>
        public IPin Up => new KeyboardKeyPin(Controller, "Up", '\u0026', InteropMac.MacKeyCodes.kVK_UpArrow);
        /// <summary>
        /// An input Pin for the Right Arrow key
        /// </summary>
        public IPin Right => new KeyboardKeyPin(Controller, "Right", '\u0027', InteropMac.MacKeyCodes.kVK_RightArrow);
        /// <summary>
        /// An input Pin for the Down Arrow key
        /// </summary>
        public IPin Down => new KeyboardKeyPin(Controller, "Down", '\u0028', InteropMac.MacKeyCodes.kVK_DownArrow);
        /// <summary>
        /// An input Pin for the Insert key
        /// </summary>
        public IPin Insert => new KeyboardKeyPin(Controller, "Insert", '\u002d', null);
        /// <summary>
        /// An input Pin for the Delete key
        /// </summary>
        public IPin Delete => new KeyboardKeyPin(Controller, "Delete", '\u002e', InteropMac.MacKeyCodes.kVK_Delete);
        /// <summary>
        /// An input Pin for the Back-tick/Tilde key
        /// </summary>
        public IPin Tilde => new KeyboardKeyPin(Controller, "Tilde", '\u00c0', null);
        /// <summary>
        /// An input Pin for the Semicolon key
        /// </summary>
        public IPin Semicolon => new KeyboardKeyPin(Controller, "Semicolon", '\u00ba', InteropMac.MacKeyCodes.kVK_ANSI_Semicolon);
        /// <summary>
        /// An input Pin for the +/= key
        /// </summary>
        public IPin Plus => new KeyboardKeyPin(Controller, "Plus", '\u00bb', InteropMac.MacKeyCodes.kVK_ANSI_Equal);
        /// <summary>
        /// An input Pin for the -/_ key
        /// </summary>
        public IPin Minus => new KeyboardKeyPin(Controller, "Minus", '\u00bd', InteropMac.MacKeyCodes.kVK_ANSI_Minus);
        /// <summary>
        /// An input Pin for the Comma key
        /// </summary>
        public IPin Comma => new KeyboardKeyPin(Controller, "Comma", '\u00bc', InteropMac.MacKeyCodes.kVK_ANSI_Comma);
        /// <summary>
        /// An input Pin for the Period key
        /// </summary>
        public IPin Period => new KeyboardKeyPin(Controller, "Period", '\u00be', InteropMac.MacKeyCodes.kVK_ANSI_Period);
        /// <summary>
        /// An input Pin for the Forward Slash key
        /// </summary>
        public IPin ForwardSlash => new KeyboardKeyPin(Controller, "ForwardSlash", '\u00bf', InteropMac.MacKeyCodes.kVK_ANSI_Slash);
        /// <summary>
        /// An input Pin for the Back Slash key
        /// </summary>
        public IPin BackSlash => new KeyboardKeyPin(Controller, "BackSlash", '\u00dc', InteropMac.MacKeyCodes.kVK_ANSI_Backslash);
        /// <summary>
        /// An input Pin for the Open Bracket key
        /// </summary>
        public IPin OpenBracket => new KeyboardKeyPin(Controller, "OpenBracket", '\u00db', InteropMac.MacKeyCodes.kVK_ANSI_LeftBracket);
        /// <summary>
        /// An input Pin for the Close Bracket key
        /// </summary>
        public IPin CloseBracket => new KeyboardKeyPin(Controller, "CloseBracket", '\u00dd', InteropMac.MacKeyCodes.kVK_ANSI_RightBracket);
        /// <summary>
        /// An input Pin for the 0 key
        /// </summary>
        public IPin Num0 => new KeyboardKeyPin(Controller, "0", '0', InteropMac.MacKeyCodes.kVK_ANSI_0);
        /// <summary>
        /// An input Pin for the 1 key
        /// </summary>
        public IPin Num1 => new KeyboardKeyPin(Controller, "1", '1', InteropMac.MacKeyCodes.kVK_ANSI_1);
        /// <summary>
        /// An input Pin for the 2 key
        /// </summary>
        public IPin Num2 => new KeyboardKeyPin(Controller, "2", '2', InteropMac.MacKeyCodes.kVK_ANSI_2);
        /// <summary>
        /// An input Pin for the 3 key
        /// </summary>
        public IPin Num3 => new KeyboardKeyPin(Controller, "3", '3', InteropMac.MacKeyCodes.kVK_ANSI_3);
        /// <summary>
        /// An input Pin for the 4 key
        /// </summary>
        public IPin Num4 => new KeyboardKeyPin(Controller, "4", '4', InteropMac.MacKeyCodes.kVK_ANSI_4);
        /// <summary>
        /// An input Pin for the 5 key
        /// </summary>
        public IPin Num5 => new KeyboardKeyPin(Controller, "5", '5', InteropMac.MacKeyCodes.kVK_ANSI_5);
        /// <summary>
        /// An input Pin for the 6 key
        /// </summary>
        public IPin Num6 => new KeyboardKeyPin(Controller, "6", '6', InteropMac.MacKeyCodes.kVK_ANSI_6);
        /// <summary>
        /// An input Pin for the 7 key
        /// </summary>
        public IPin Num7 => new KeyboardKeyPin(Controller, "7", '7', InteropMac.MacKeyCodes.kVK_ANSI_7);
        /// <summary>
        /// An input Pin for the 8 key
        /// </summary>
        public IPin Num8 => new KeyboardKeyPin(Controller, "8", '8', InteropMac.MacKeyCodes.kVK_ANSI_8);
        /// <summary>
        /// An input Pin for the 9 key
        /// </summary>
        public IPin Num9 => new KeyboardKeyPin(Controller, "9", '9', InteropMac.MacKeyCodes.kVK_ANSI_9);
        /// <summary>
        /// An input Pin for the A key
        /// </summary>
        public IPin A => new KeyboardKeyPin(Controller, "A", 'A', InteropMac.MacKeyCodes.kVK_ANSI_A);
        /// <summary>
        /// An input Pin for the B key
        /// </summary>
        public IPin B => new KeyboardKeyPin(Controller, "B", 'B', InteropMac.MacKeyCodes.kVK_ANSI_B);
        /// <summary>
        /// An input Pin for the C key
        /// </summary>
        public IPin C => new KeyboardKeyPin(Controller, "C", 'C', InteropMac.MacKeyCodes.kVK_ANSI_C);
        /// <summary>
        /// An input Pin for the D key
        /// </summary>
        public IPin D => new KeyboardKeyPin(Controller, "D", 'D', InteropMac.MacKeyCodes.kVK_ANSI_D);
        /// <summary>
        /// An input Pin for the E key
        /// </summary>
        public IPin E => new KeyboardKeyPin(Controller, "E", 'E', InteropMac.MacKeyCodes.kVK_ANSI_E);
        /// <summary>
        /// An input Pin for the F key
        /// </summary>
        public IPin F => new KeyboardKeyPin(Controller, "F", 'F', InteropMac.MacKeyCodes.kVK_ANSI_F);
        /// <summary>
        /// An input Pin for the G key
        /// </summary>
        public IPin G => new KeyboardKeyPin(Controller, "G", 'G', InteropMac.MacKeyCodes.kVK_ANSI_G);
        /// <summary>
        /// An input Pin for the H key
        /// </summary>
        public IPin H => new KeyboardKeyPin(Controller, "H", 'H', InteropMac.MacKeyCodes.kVK_ANSI_H);
        /// <summary>
        /// An input Pin for the I key
        /// </summary>
        public IPin I => new KeyboardKeyPin(Controller, "I", 'I', InteropMac.MacKeyCodes.kVK_ANSI_I);
        /// <summary>
        /// An input Pin for the J key
        /// </summary>
        public IPin J => new KeyboardKeyPin(Controller, "J", 'J', InteropMac.MacKeyCodes.kVK_ANSI_J);
        /// <summary>
        /// An input Pin for the K key
        /// </summary>
        public IPin K => new KeyboardKeyPin(Controller, "K", 'K', InteropMac.MacKeyCodes.kVK_ANSI_K);
        /// <summary>
        /// An input Pin for the L key
        /// </summary>
        public IPin L => new KeyboardKeyPin(Controller, "L", 'L', InteropMac.MacKeyCodes.kVK_ANSI_L);
        /// <summary>
        /// An input Pin for the M key
        /// </summary>
        public IPin M => new KeyboardKeyPin(Controller, "M", 'M', InteropMac.MacKeyCodes.kVK_ANSI_M);
        /// <summary>
        /// An input Pin for the N key
        /// </summary>
        public IPin N => new KeyboardKeyPin(Controller, "N", 'N', InteropMac.MacKeyCodes.kVK_ANSI_N);
        /// <summary>
        /// An input Pin for the O key
        /// </summary>
        public IPin O => new KeyboardKeyPin(Controller, "O", 'O', InteropMac.MacKeyCodes.kVK_ANSI_O);
        /// <summary>
        /// An input Pin for the P key
        /// </summary>
        public IPin P => new KeyboardKeyPin(Controller, "P", 'P', InteropMac.MacKeyCodes.kVK_ANSI_P);
        /// <summary>
        /// An input Pin for the Q key
        /// </summary>
        public IPin Q => new KeyboardKeyPin(Controller, "Q", 'Q', InteropMac.MacKeyCodes.kVK_ANSI_Q);
        /// <summary>
        /// An input Pin for the R key
        /// </summary>
        public IPin R => new KeyboardKeyPin(Controller, "R", 'R', InteropMac.MacKeyCodes.kVK_ANSI_R);
        /// <summary>
        /// An input Pin for the S key
        /// </summary>
        public IPin S => new KeyboardKeyPin(Controller, "S", 'S', InteropMac.MacKeyCodes.kVK_ANSI_S);
        /// <summary>
        /// An input Pin for the T key
        /// </summary>
        public IPin T => new KeyboardKeyPin(Controller, "T", 'T', InteropMac.MacKeyCodes.kVK_ANSI_T);
        /// <summary>
        /// An input Pin for the U key
        /// </summary>
        public IPin U => new KeyboardKeyPin(Controller, "U", 'U', InteropMac.MacKeyCodes.kVK_ANSI_U);
        /// <summary>
        /// An input Pin for the V key
        /// </summary>
        public IPin V => new KeyboardKeyPin(Controller, "V", 'V', InteropMac.MacKeyCodes.kVK_ANSI_V);
        /// <summary>
        /// An input Pin for the W key
        /// </summary>
        public IPin W => new KeyboardKeyPin(Controller, "W", 'W', InteropMac.MacKeyCodes.kVK_ANSI_W);
        /// <summary>
        /// An input Pin for the X key
        /// </summary>
        public IPin X => new KeyboardKeyPin(Controller, "X", 'X', InteropMac.MacKeyCodes.kVK_ANSI_X);
        /// <summary>
        /// An input Pin for the Y key
        /// </summary>
        public IPin Y => new KeyboardKeyPin(Controller, "Y", 'Y', InteropMac.MacKeyCodes.kVK_ANSI_Y);
        /// <summary>
        /// An input Pin for the Z key
        /// </summary>
        public IPin Z => new KeyboardKeyPin(Controller, "Z", 'Z', InteropMac.MacKeyCodes.kVK_ANSI_Z);
        /// <summary>
        /// An input Pin for Number Pad 0
        /// </summary>
        public IPin NumPad0 => new KeyboardKeyPin(Controller, "NumPad0", '\u0060', InteropMac.MacKeyCodes.kVK_ANSI_Keypad0);
        /// <summary>
        /// An input Pin for Number Pad 1
        /// </summary>
        public IPin NumPad1 => new KeyboardKeyPin(Controller, "NumPad1", '\u0061', InteropMac.MacKeyCodes.kVK_ANSI_Keypad1);
        /// <summary>
        /// An input Pin for Number Pad 2
        /// </summary>
        public IPin NumPad2 => new KeyboardKeyPin(Controller, "NumPad2", '\u0062', InteropMac.MacKeyCodes.kVK_ANSI_Keypad2);
        /// <summary>
        /// An input Pin for Number Pad 3
        /// </summary>
        public IPin NumPad3 => new KeyboardKeyPin(Controller, "NumPad3", '\u0063', InteropMac.MacKeyCodes.kVK_ANSI_Keypad3);
        /// <summary>
        /// An input Pin for Number Pad 4
        /// </summary>
        public IPin NumPad4 => new KeyboardKeyPin(Controller, "NumPad4", '\u0064', InteropMac.MacKeyCodes.kVK_ANSI_Keypad4);
        /// <summary>
        /// An input Pin for Number Pad 5
        /// </summary>
        public IPin NumPad5 => new KeyboardKeyPin(Controller, "NumPad5", '\u0065', InteropMac.MacKeyCodes.kVK_ANSI_Keypad5);
        /// <summary>
        /// An input Pin for Number Pad 6
        /// </summary>
        public IPin NumPad6 => new KeyboardKeyPin(Controller, "NumPad6", '\u0066', InteropMac.MacKeyCodes.kVK_ANSI_Keypad6);
        /// <summary>
        /// An input Pin for Number Pad 7
        /// </summary>
        public IPin NumPad7 => new KeyboardKeyPin(Controller, "NumPad7", '\u0067', InteropMac.MacKeyCodes.kVK_ANSI_Keypad7);
        /// <summary>
        /// An input Pin for Number Pad 8
        /// </summary>
        public IPin NumPad8 => new KeyboardKeyPin(Controller, "NumPad8", '\u0068', InteropMac.MacKeyCodes.kVK_ANSI_Keypad8);
        /// <summary>
        /// An input Pin for Number Pad 9
        /// </summary>
        public IPin NumPad9 => new KeyboardKeyPin(Controller, "NumPad9", '\u0069', InteropMac.MacKeyCodes.kVK_ANSI_Keypad9);

        /// <summary>
        /// An output Pin for Caps Lock indicator
        /// </summary>
        public IPin CapsLock => new KeyboardIndicatorPin(Controller, "CapsLock", InteropWindows.Indicators.KEYBOARD_CAPS_LOCK_ON);
        /// <summary>
        /// An output Pin for Number Lock indicator
        /// </summary>
        public IPin NumLock => new KeyboardIndicatorPin(Controller, "NumLock", InteropWindows.Indicators.KEYBOARD_NUM_LOCK_ON);
        /// <summary>
        /// An output Pin for Scroll Lock indicator
        /// </summary>
        public IPin ScrollLock => new KeyboardIndicatorPin(Controller, "ScrollLock", InteropWindows.Indicators.KEYBOARD_SCROLL_LOCK_ON);
        /// <summary>
        /// An output Pin for Kana Lock indicator
        /// </summary>
        public IPin KanaLock => new KeyboardIndicatorPin(Controller, "KanaLock", InteropWindows.Indicators.KEYBOARD_KANA_LOCK_ON);
    }
}

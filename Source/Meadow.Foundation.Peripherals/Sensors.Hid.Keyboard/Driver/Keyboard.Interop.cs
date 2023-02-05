using System.Runtime.InteropServices;

namespace Meadow.Foundation.Sensors.Hid;

public partial class Keyboard
{
    internal class Interop
    {
        private const string USER32 = "user32.dll";

        [DllImport(USER32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern short GetAsyncKeyState(int vKey);
    }
}


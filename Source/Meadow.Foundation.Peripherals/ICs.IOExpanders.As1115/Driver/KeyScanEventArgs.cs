using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public class KeyScanEventArgs : EventArgs
    {
        public As1115.KeyScanButton Button { get; protected set; }

        public byte KeyA { get; protected set; }
        public byte KeyB { get; protected set; }

        public KeyScanEventArgs(As1115.KeyScanButton button, byte keyA, byte keyB)
        {
            Button = button;
            KeyA = keyA;
            KeyB = keyB;
        }
    }
}
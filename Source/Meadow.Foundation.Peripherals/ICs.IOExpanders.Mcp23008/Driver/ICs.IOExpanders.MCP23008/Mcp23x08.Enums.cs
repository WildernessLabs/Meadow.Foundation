using System;
namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        // what's a good way to do this? maybe constants? how to name?
        public enum ValidSpeeds : ushort
        {
            /// <summary>
            /// 100kHz
            /// </summary>
            kHz100 = 100,
            /// <summary>
            /// 400kHz
            /// </summary>
            kHz400 = 400,
            /// <summary>
            /// 1.7mHz
            /// </summary>
            kHz1700 = 17000,
        }
    }
}

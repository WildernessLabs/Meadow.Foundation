using System;
using System.Collections.Generic;
using System.Text;

namespace Meadow.Foundation.ICs.FRAM
{
    public partial class MB85RSxx
    {
        /// <summary>
        /// Operation Codes
        /// </summary>
        public enum OperationCodes : byte
        {
            /// <summary>
            /// Write Enable Latch
            /// </summary>
            OPCODE_WREN = 0b0000_0110,
            /// <summary>
            /// Reset Write Enable Latch
            /// </summary>
            OPCODE_WRDI = 0b0000_0100,
            /// <summary>
            /// Read Status Register
            /// </summary>
            OPCODE_RDSR = 0b0000_0101,
            /// <summary>
            /// Write Status Register
            /// </summary>
            OPCODE_WRSR = 0b0000_0001,
            /// <summary>
            /// Read Memory
            /// </summary>
            OPCODE_READ = 0b0000_0011,
            /// <summary>
            /// Write Memory
            /// </summary>
            OPCODE_WRITE = 0b0000_0010,
            /// <summary>
            /// Read Device ID
            /// </summary>
            OPCODE_RDID = 0b1001_1111
        }
    }
}

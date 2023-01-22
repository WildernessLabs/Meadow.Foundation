using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ft232h
    {
        [Flags]
        internal enum SPITransferOptions
        {
            /* transferOptions-Bit0: If this bit is 0 then it means that the transfer size provided is in bytes */
            SIZE_IN_BYTES = 0x00000000,
            /* transferOptions-Bit0: If this bit is 1 then it means that the transfer size provided is in bytes */
            SIZE_IN_BITS = 0x00000001,
            /* transferOptions-Bit1: if BIT1 is 1 then CHIP_SELECT line will be enabled at start of transfer */
            CHIPSELECT_ENABLE = 0x00000002,
            /* transferOptions-Bit2: if BIT2 is 1 then CHIP_SELECT line will be disabled at end of transfer */
            CHIPSELECT_DISABLE = 0x00000004,
            /* transferOptions-Bit3: if BIT3 is 1 then LSB will be processed first */
            LSB_FIRST = 0x00000008
        }
    }
}
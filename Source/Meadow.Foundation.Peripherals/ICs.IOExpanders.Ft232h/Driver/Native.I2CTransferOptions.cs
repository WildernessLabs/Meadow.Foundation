using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    internal static partial class Native
    {
        [Flags]
        public enum I2CTransferOptions
        {
            /// <summary>
            /// Generate start condition before transmitting.
            /// </summary>
            START_BIT = 0x00000001,

            /// <summary>
            /// Generate stop condition before transmitting.
            /// </summary>
            STOP_BIT = 0x00000002,

            /// <summary>
            /// Continue transmitting data in bulk without caring about Ack or nAck from device if this bit
            /// is not set. If this bit is set then stop transferring the data in the buffer when the device
            /// nACKs.
            /// </summary>
            BREAK_ON_NACK = 0x00000004,

            /// <summary>
            /// libMPSSE-I2C generates an ACKs for every byte read. Some I2C slaves require the I2C
            /// master to generate a nACK for the last data byte read. Setting this bit enables working with
            /// such I2C slaves.
            /// </summary>
            NACK_LAST_BYTE = 0x00000008,

            /// <summary>
            /// Fast transfers prepare a buffer containing commands to generate START/STOP/ADDRESS
            /// conditions and commands to read/write data. This buffer is sent to the MPSSE in one shot,
            /// hence delays between different phases of the I2C transfer are eliminated. Fast transfers
            /// can have data length in terms of bits or bytes. The user application should call
            /// I2C_DeviceWrite or I2C_DeviceRead with either
            /// I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES or
            /// I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BITS bit set to perform a fast transfer.
            /// IC_TRANSFER_OPTIONS_START_BIT and I2C_TRANSFER_OPTIONS_STOP_BIT have
            /// their usual meanings when used in fast transfers, however
            /// IC_TRANSFER_OPTIONS_BREAK_ON_NACK and
            /// I2C_TRANSFER_OPTIONS_NACK_LAST_BYTE are not applicable in fast transfers.
            /// </summary>
            FAST_TRANSFER = 0x00000030, /*not visible to user*/

            /// <summary>
            /// When the user calls I2C_DeviceWrite or I2C_DeviceRead with this bit set then libMPSSE
            /// packs commands to transfer sizeToTransfer number of bytes, and to read/write
            /// izeToTransfer number of ack bits. If data is written then the read ack bits are ignored, if
            /// data is being read then an acknowledgment bit(SDA=LOW) is given to the I2C slave            ///after each byte read.
            /// </summary>

            FAST_TRANSFER_BYTES = 0x00000010,

            /// <summary>
            /// When the user calls I2C_DeviceWrite or I2C_DeviceRead with this bit set then libMPSSE
            /// packs commands to transfer sizeToTransfer number of bits. There is no ACK phase when
            /// this bit is set.
            /// </summary>
            FAST_TRANSFER_BITS = 0x00000020,

            /// <summary>
            /// The address parameter is ignored in transfers if this bit is set. This would mean that
            /// the address is either a part of the data or this is a special I2C frame that doesn't require
            /// an address. However if this bit is not set then 7bit address and 1bit direction will be
            /// written to the I2C bus each time I2C_DeviceWrite or I2C_DeviceRead is called and a
            /// 1bit acknowledgment will be read after that.
            /// </summary>
            NO_ADDRESS = 0x00000040
        }
    }
}
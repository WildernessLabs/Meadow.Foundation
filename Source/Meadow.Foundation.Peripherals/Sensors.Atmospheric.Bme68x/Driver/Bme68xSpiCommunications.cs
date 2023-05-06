using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Bme68xSpiCommunications : SpiCommunications
    {
        /// <summary>
        /// Register pages (for SPI only)
        /// Page 0 for 0x80 to 0xFF
        /// Page 1 for 0x00 to 0x7F
        /// Controlled via bit 4 on register 0x73
        /// and is the only bit used on that register
        /// </summary>
        internal enum SpiRegisterPage : byte
        {
            /// <summary>
            /// SPI page for registers 0x80 to 0xFF
            /// Enabled by setting bit 4 to 1 on 0x73
            /// </summary>
            Page0 = 0x10,
            /// <summary>
            /// SPI page for registers 0x00 to 0x7F
            /// Enabled by setting bit 4 to 1 on 0x73
            /// </summary>
            Page1 = 0x00,
        }

        SpiRegisterPage currentPage = SpiRegisterPage.Page1;

        internal Bme68xSpiCommunications(ISpiBus spiBus, IDigitalOutputPort? chipSelect, Frequency busSpeed, SpiClockConfiguration.Mode busMode)
            : base(spiBus, chipSelect, busSpeed, busMode, 32, 32)
        {
        }

        public override byte ReadRegister(byte address)
        {
            SetPageForRegister(address);

            //adjust register for paging
            if (address > 0x7F) { address -= 0x7F; }

            return base.ReadRegister(address);
        }

        public override ushort ReadRegisterAsUShort(byte address, ByteOrder order = ByteOrder.LittleEndian)
        {
            SetPageForRegister(address);

            //adjust register for paging
            if (address > 0x7F) { address -= 0x7F; }

            return base.ReadRegisterAsUShort(address, order);
        }

        public override void ReadRegister(byte startRegister, Span<byte> readBuffer)
        {
            SetPageForRegister(startRegister);

            //adjust register for paging
            if (startRegister > 0x7F) { startRegister -= 0x7F; }

            base.ReadRegister(startRegister, readBuffer);
        }

        public override void WriteRegister(byte register, byte value)
        {
            SetPageForRegister(register);

            //adjust register for paging
            if (register > 0x7F) { register -= 0x7F; }

            base.WriteRegister(register, value);
        }

        void SetPageForRegister(byte register)
        {
            if (currentPage != GetPageForRegister(register))
            {
                Resolver.Log.Info($"Switching page for register {register}");
                //swap the page
                currentPage = (currentPage == SpiRegisterPage.Page0) ? SpiRegisterPage.Page1 : SpiRegisterPage.Page0;
                //write the page to the status register
                base.WriteRegister(0x73, (byte)currentPage);
            }
        }

        SpiRegisterPage GetPageForRegister(byte register) => (register < 0x80) ? SpiRegisterPage.Page1 : SpiRegisterPage.Page0;
    }
}
using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal class Bme68xSPI : Bme68xComms
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

        ISpiPeripheral spiPeripheral;

        SpiRegisterPage currentPage = SpiRegisterPage.Page1;

        internal Bme68xSPI(ISpiBus spi, IDigitalOutputPort? chipSelect = null)
        {
            spiPeripheral = new SpiPeripheral(spi, chipSelect, 32, 32);
        }

        public override byte ReadRegister(byte address)
        {
            SetPageForRegister(address);

            //adjust register for paging
            if (address > 0x7F) { address -= 0x7F; }

            return spiPeripheral.ReadRegister(address);
        }

        public override ushort ReadRegisterAsUShort(byte address, ByteOrder order = ByteOrder.LittleEndian)
        {
            SetPageForRegister(address);

            //adjust register for paging
            if (address > 0x7F) { address -= 0x7F; }

            return spiPeripheral.ReadRegisterAsUShort(address, order);
        }

        public override void ReadRegister(byte startRegister, Span<byte> readBuffer)
        {
            SetPageForRegister(startRegister);

            //adjust register for paging
            if (startRegister > 0x7F) { startRegister -= 0x7F; }

            spiPeripheral.ReadRegister(startRegister, readBuffer);

            return;
        }

        public override void WriteRegister(byte register, byte value)
        {
            SetPageForRegister(register);

            //adjust register for paging
            if (register > 0x7F) { register -= 0x7F; }

            spiPeripheral.WriteRegister(register, value);
        }

        void SetPageForRegister(byte register)
        {
            if (currentPage != GetPageForRegister(register))
            {
                Console.WriteLine($"Switching page for register {register}");
                //swap the page
                currentPage = (currentPage == SpiRegisterPage.Page0) ? SpiRegisterPage.Page1 : SpiRegisterPage.Page0;
                //write the page to the status register
                spiPeripheral.WriteRegister(0x73, (byte)currentPage);
            }
        }

        SpiRegisterPage GetPageForRegister(byte register) => (register < 0x80) ? SpiRegisterPage.Page1 : SpiRegisterPage.Page0;
    }
}
using System;
using Meadow.Hardware;
using Meadow.Units;
using System.Linq;

namespace Meadow.Foundation.ICs.FRAM
{
    /// <summary>
    /// Encapsulation for FRAMs based upon the MB85RSxx family of chips
    /// </summary>
    public partial class MB85RSxx : ISpiPeripheral
    {
        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        private readonly ISpiCommunications _spiComms = default!;

        private readonly IDigitalOutputPort _chipSelectPort;
        private readonly IDigitalOutputPort? _holdPort;
        private readonly IDigitalOutputPort? _writeProtectPort;



        /// <summary>
        /// Gets the default SPI bus mode (Mode0).
        /// MB85RSxx supports SPI modes 0 (CPOL = 0, CPHA = 0) & 3 (CPOL = 1, CPHA = 1).
        /// </summary>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// Gets the default SPI bus speed (20 MHz).
        /// MB85RSxx supports SPI speeds upto 20 MHz.
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new Frequency(20, Frequency.UnitType.Megahertz);

        /// <summary>
        /// Gets or sets the SPI bus mode.
        /// MB85RSxx supports SPI modes 0 (CPOL = 0, CPHA = 0) & 3 (CPOL = 1, CPHA = 1).
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode { get; set; }

        /// <summary>
        /// Gets or sets the SPI bus speed.
        /// MB85RSxx supports SPI speeds upto 20 MHz.
        /// </summary>
        public Frequency SpiBusSpeed { get; set; }

        /// <summary>
        /// MB85RSxx varient (e.g. MB85RS64 for 64 kbit)
        /// </summary>
        public Varient ChipVarient { get; private set; }

        private Memory<byte> ReadBuffer { get; set; }
        private Memory<byte> WriteBuffer { get; set; }


        /// <summary>
        /// Initializes a new instance of the MB85RSxx class.
        /// </summary>
        /// <param name="spiBus">The SPI bus.</param>
        /// <param name="chipSelectPort">The chip select port.</param>
        public MB85RSxx(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
        {
            _spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);
            _chipSelectPort = chipSelectPort;

            SpiBusMode = DefaultSpiBusMode;
            SpiBusSpeed = DefaultSpiBusSpeed;

            ReadBuffer = new byte[8]; // always 1 byte but we allow for upto 8 bytes to be returned per transaction)
            WriteBuffer = new byte[5]; // upto 32 bits

            Varient? varient = GetDeviceVarient();
            if (varient == null)
                throw new DeviceConfigurationException("MB85RSxx device type unknown");
            else
                ChipVarient = varient;

            Resolver.Log.Trace($"FRAM chip is {ChipVarient.Name} with {ChipVarient.Size} bytes of data");
        }

        /// <summary>
        /// Initializes a new instance of the MB85RSxx class with Write Protect port (Not Implimented).
        /// </summary>
        /// <param name="spiBus">The SPI bus.</param>
        /// <param name="chipSelectPort">The chip select port.</param>
        /// <param name="writeProtectPort">Write Protect port. This port does not affect write protection for the entire chip, only the status register.</param>
        public MB85RSxx(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalOutputPort writeProtectPort)
        {
            _spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);
            _chipSelectPort = chipSelectPort;
            _writeProtectPort = writeProtectPort;

            _writeProtectPort.State = false;

            SpiBusMode = DefaultSpiBusMode;
            SpiBusSpeed = DefaultSpiBusSpeed;

            ReadBuffer = new byte[8]; // always 1 byte but we allow for upto 8 bytes to be returned per transaction)
            WriteBuffer = new byte[5]; // upto 32 bits

            Varient? varient = GetDeviceVarient();
            if (varient == null)
                throw new DeviceConfigurationException("MB85RSxx device type unknown");
            else
                ChipVarient = varient;

            Resolver.Log.Trace($"FRAM chip is {ChipVarient.Name} with {ChipVarient.Size} bytes of data");
        }

        /// <summary>
        /// Initializes a new instance of the MB85RSxx class with Write Protect port (Not Implimented) and Hold port (Not Implimented).
        /// </summary>
        /// <param name="spiBus">The SPI bus.</param>
        /// <param name="chipSelectPort">The chip select port.</param>
        /// <param name="writeProtectPort">Write Protect port. This port does not affect write protection for the entire chip, only the status register.</param>
        /// <param name="holdPort">Wait port for the SPI bus. When low, it puts the SPI bus on hold. </param>
        public MB85RSxx(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalOutputPort writeProtectPort, IDigitalOutputPort holdPort)
        {
            _spiComms = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);
            _chipSelectPort = chipSelectPort;
            _holdPort = holdPort;
            _writeProtectPort = writeProtectPort;

            _holdPort.State = true;
            _writeProtectPort.State = false;

            SpiBusMode = DefaultSpiBusMode;
            SpiBusSpeed = DefaultSpiBusSpeed;

            ReadBuffer = new byte[8]; // always 1 byte but we allow for upto 8 bytes to be returned per transaction)
            WriteBuffer = new byte[5]; // upto 32 bits

            Varient? varient = GetDeviceVarient();
            if (varient == null)
                throw new DeviceConfigurationException("MB85RSxx device type unknown");
            else
                ChipVarient = varient;

            Resolver.Log.Trace($"FRAM chip is {ChipVarient.Name} with {ChipVarient.Size} bytes of data");
        }

        /// <summary>
        /// Read data from FRAM.
        /// </summary>
        /// <param name="startAddress"></param>
        /// <param name="ammount"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte[] Read(uint startAddress, byte ammount)
        {
            if (ammount > 8)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ammount), "Only upto 8 bytes can be read in one block");
            }

            CheckAddress(startAddress, ammount);

            for (ushort index = 0; index < ammount; index++)
            {
                var address = (uint)(startAddress + index);
                Span<byte> dataToSend = Address(OperationCodes.OPCODE_READ, address);
                _spiComms.Exchange(dataToSend, ReadBuffer[index..(index + 1)].Span);
                //Thread.Sleep(10);
            }
            return ReadBuffer.Slice(0, ammount).ToArray();
        }

        /// <summary>
        /// Write data to FRAM.
        /// </summary>
        /// <param name="startAddress">The address to write to in FRAM memory.</param>
        /// <param name="data">Data to be written to the address.</param>
        public void Write(uint startAddress, byte[] data)
        {
            CheckAddress(startAddress, data.Length);
            for (ushort index = 0; index < data.Length; index++)
            {
                WriteEnable(true);
                var address = (uint)(startAddress + index);
                Span<byte> dataToSend = Address(OperationCodes.OPCODE_WRITE, address);
                dataToSend[dataToSend.Length - 1] = data[index];
                _spiComms.Write(dataToSend);
                Resolver.Log.Trace($"  FRAM Written Dat:{data[index]} to Adr:{address}");
                //Thread.Sleep(10);
            }
            WriteEnable(false);
        }

        /// <summary>
        /// Reads the device type and varient
        /// </summary>
        /// <returns></returns>
        public Varient? GetDeviceVarient()
        {
            Span<byte> dataToSend = WriteBuffer.Span[0..1];
            dataToSend[0] = (byte)OperationCodes.OPCODE_RDID;
            Span<byte> dataRecieved = ReadBuffer[0..4].Span;
            _spiComms.Exchange(dataToSend, dataRecieved);

            byte manId = 0x00;
            ushort prodId = 0x00;

            var read = dataRecieved.ToArray();
            if (read[1] == 0x7f)
            {
                // Device with continuation code (0x7F) in their second byte
                // Manufacturers ID (1 byte) - 0x7F - Product ID (2 bytes)
                manId = (read[0]);
                prodId = (ushort)((read[2] << 8) + read[3]);
            }
            else
            {
                // Device without continuation code
                // Manufacturers ID (1 byte) - Product ID (2 bytes)
                manId = (read[0]);
                prodId = (ushort)((read[1] << 8) + read[2]);
            }

            return VarientsTable.Varients.Where(x => x.ManufacturerId == manId && x.ProductId == prodId).FirstOrDefault();
        }

        /// <summary>
        /// Disables Write Protection
        /// </summary>
        /// <param name="enable"></param>
        private void WriteEnable(bool enable)
        {
            Resolver.Log.Trace($"  FRAM Sending Write Enable[{enable}]");
            if (enable)
                _spiComms.Write((byte)OperationCodes.OPCODE_WREN);
            else
                _spiComms.Write((byte)OperationCodes.OPCODE_WRDI);
            Resolver.Log.Trace($"  FRAM completed WriteEnable[{enable}]");
            //Thread.Sleep(10);
        }

        /// <summary>
        /// Checks the startAddress and the amount of data being accessed to make sure that the
        /// address and the startAddress plus the amount remain within the bounds of the memory chip.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="amount"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void CheckAddress(uint address, int amount)
        {
            if (address > ChipVarient.Size)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(address), "startAddress should be less than the amount of memory in the module");
            }
            if ((address + amount) > ChipVarient.Size)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(address), "startAddress + amount should be less than the amount of memory in the module");
            }
        }


        /// <summary>
        /// Sets the correct address depending on the address size.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="address"></param>
        /// <returns>Span<byte> to send</returns>
        private Span<byte> Address(OperationCodes code, uint address)
        {
            Span<byte> data = WriteBuffer.Span[0..5];
            data[0] = (byte)code;
            byte length = 3;

            if (ChipVarient.Size <= 32 * 1024)
            {
                data[1] = (byte)(address >> 8);
                data[2] = (byte)(address & 0xff);

                length = 3;
            }
            else
            {
                data[1] = (byte)(address >> 16);
                data[2] = (byte)(address >> 8);
                data[3] = (byte)(address & 0xff);

                length = 4;
            }


            if (code == OperationCodes.OPCODE_WRITE)
                length++;
            return data.Slice(0, length);
        }





    }
}

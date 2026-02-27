using Meadow.Hardware;
using Meadow.Utilities;
using System;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// A TCA9548A i2c multiplexer
    /// </summary>
    public partial class Tca9548a : II2cPeripheral
    {
        /// <summary>
        /// Gets the specified I2C bus (0-7) connected to the multiplexer output.
        /// </summary>
        public II2cBus this[byte index]
        {
            get
            {
                if (index > 7)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Bus index must be 0..7.");
                }
                return i2cBuses[index];
            }
        }

        /// <summary>
        /// The address of this device on the <see cref="I2cBus"/>
        /// </summary>
        public byte Address { get; }

        /// <summary>
        /// The <see cref="II2cBus"/> this device is connected to.
        /// </summary>
        public II2cBus I2cBus { get; }

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD0/SC0
        /// </summary>
        public II2cBus Bus0 => i2cBuses[0];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD1/SC1
        /// </summary>
        public II2cBus Bus1 => i2cBuses[1];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD2/SC2
        /// </summary>
        public II2cBus Bus2 => i2cBuses[2];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD3/SC3
        /// </summary>
        public II2cBus Bus3 => i2cBuses[3];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD4/SC4
        /// </summary>
        public II2cBus Bus4 => i2cBuses[4];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD5/SC5
        /// </summary>
        public II2cBus Bus5 => i2cBuses[5];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD6/SC6
        /// </summary>
        public II2cBus Bus6 => i2cBuses[6];

        /// <summary>
        /// The <see cref="II2cBus"/> connected to SD7/SC7
        /// </summary>
        public II2cBus Bus7 => i2cBuses[7];

        /// <inheritdoc/>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        private readonly II2cBus[] i2cBuses;
        private readonly I2cCommunications i2cComms;
        private byte selectedBus = 0xff;

        internal SemaphoreSlim BusSelectorSemaphore = new(1, 1);

        /// <summary>
        /// Create a <see cref="Tca9548a"/> i2c multiplexer
        /// </summary>
        /// <param name="i2cBus">The <see cref="II2cBus"/> the device is attached to</param>
        /// <param name="address">The address of the device on the specified <paramref name="i2cBus"/></param>
        /// <exception cref="ArgumentOutOfRangeException">The device address was invalid</exception>
        /// <exception cref="ArgumentNullException">The i2cBus was null</exception>
        public Tca9548a(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            I2cBus = i2cBus;
            Address = address;
            i2cComms = new I2cCommunications(i2cBus, address);

            i2cBuses = Enumerable.Range(0, 8).Select(i => new Tca9548AI2cBus(this, (byte)i) as II2cBus).ToArray();
        }

        /// <summary>
        /// Create a <see cref="Tca9548a"/> i2c multiplexer.
        /// </summary>
        /// <param name="i2cBus">The I2cBus the device is attached to</param>
        /// <param name="a0">The logic high/low state of pin A0</param>
        /// <param name="a1">The logic high/low state of pin A1</param>
        /// <param name="a2">The logic high/low state of pin A2</param>
        public Tca9548a(II2cBus i2cBus, bool a0, bool a1, bool a2)
            : this(i2cBus, TcaAddressTable.GetAddressFromPins(a0, a1, a2))
        { }

        /// <summary>
        /// Activate the specified i2cBus
        /// </summary>
        /// <param name="busIndex"></param>
        internal void SelectBus(byte busIndex)
        {
            if (busIndex > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(busIndex), busIndex, "Bus index must be 0..7.");
            }

            if (selectedBus == busIndex) { return; }

            byte mask = BitHelpers.SetBit(0x00, busIndex, true);
            i2cComms.Write(new byte[] { mask });
            selectedBus = busIndex;
        }

    }
}
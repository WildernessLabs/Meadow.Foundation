using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Driver for the AK975x family of 4-channel human presence IR sensors (AK9750, AK9753)
    /// </summary>
    /// <remarks>
    /// The AK9753 has four IR channels arranged as a directional presence sensor
    /// (Down, Left, Up, Right). It communicates over I2C and requires reading a dummy
    /// register (ST2) after each data read to trigger the next measurement cycle.
    /// </remarks>
    public partial class Ak9753 : ByteCommsSensorBase<Ak9753SensorData>, II2cPeripheral
    {
        /// <inheritdoc/>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>IR channel 1 (Down) from the last reading</summary>
        public short Ir1 => Conditions.Ir1;

        /// <summary>IR channel 2 (Left) from the last reading</summary>
        public short Ir2 => Conditions.Ir2;

        /// <summary>IR channel 3 (Up) from the last reading</summary>
        public short Ir3 => Conditions.Ir3;

        /// <summary>IR channel 4 (Right) from the last reading</summary>
        public short Ir4 => Conditions.Ir4;

        /// <summary>Internal temperature from the last reading</summary>
        public Meadow.Units.Temperature Temperature => Conditions.Temperature;

        /// <summary>
        /// Creates a new Ak9753 driver
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the sensor</param>
        /// <param name="address">I2C address of the sensor (default 0x64)</param>
        public Ak9753(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address, readBufferSize: 10, writeBufferSize: 2)
        {
            Initialize();
        }

        private void Initialize()
        {
            var deviceId = ReadRegister(Registers.WIA2);
            if (deviceId != 0x13)
                throw new Exception($"AK9753: unexpected device ID 0x{deviceId:X2} (expected 0x13)");

            SetMode(Mode.Continuous0);
            SetFilterFrequency(FilterFrequency.Hz_8_8);

            // Read ST2 dummy register to start the first measurement cycle
            ReadRegister(Registers.ST2);
        }

        /// <summary>
        /// Writes the desired operating mode (e.g., continuous, single-shot, or standby) to the ECNTL1 register.
        /// </summary>
        /// <param name="mode">Desired operating mode</param>
        public void SetMode(Mode mode)
        {
            byte current = ReadRegister(Registers.ECNTL1);
            current = (byte)((current & 0b11111000) | (byte)mode);
            WriteRegister(Registers.ECNTL1, current);
        }

        /// <summary>
        /// Sets the digital filter cutoff frequency
        /// </summary>
        /// <param name="frequency">Desired cutoff frequency</param>
        public void SetFilterFrequency(FilterFrequency frequency)
        {
            byte current = ReadRegister(Registers.ECNTL1);
            current = (byte)((current & 0b11000111) | ((byte)frequency << 3));
            WriteRegister(Registers.ECNTL1, current);
        }

        /// <summary>
        /// Returns true if new data is ready to be read
        /// </summary>
        public bool IsDataReady() => (ReadRegister(Registers.ST1) & 0x01) != 0;

        /// <summary>
        /// Returns true if a data overrun has occurred (new data arrived before the previous read)
        /// </summary>
        public bool IsDataOverrun() => (ReadRegister(Registers.ST1) & 0x02) != 0;

        /// <summary>
        /// Performs a software reset and re-initializes the sensor
        /// </summary>
        public void SoftReset()
        {
            // SRST is bit 0 only; auto-clears after reset completes
            WriteRegister(Registers.CNTL2, 0x01);
            Thread.Sleep(10);
            Initialize();
        }

        /// <inheritdoc/>
        protected override Task<Ak9753SensorData> ReadSensor()
        {
            // Wait for data ready, 100ms timeout
            int timeout = 100;
            while (!IsDataReady() && --timeout > 0)
                Thread.Sleep(1);

            // Burst read 10 bytes: IR1L, IR1H, IR2L, IR2H, IR3L, IR3H, IR4L, IR4H, TMPL, TMPH
            BusComms!.ReadRegister((byte)Registers.IR1L, ReadBuffer.Span[0..10]);

            var ir1 = (short)(ReadBuffer.Span[0] | (ReadBuffer.Span[1] << 8));
            var ir2 = (short)(ReadBuffer.Span[2] | (ReadBuffer.Span[3] << 8));
            var ir3 = (short)(ReadBuffer.Span[4] | (ReadBuffer.Span[5] << 8));
            var ir4 = (short)(ReadBuffer.Span[6] | (ReadBuffer.Span[7] << 8));

            // Temperature is 10-bit two's complement; bits 5:0 are fixed at 0.
            // Cast to short before shifting to preserve sign for sub-ambient readings.
            var rawTemp = (short)(ReadBuffer.Span[8] | (ReadBuffer.Span[9] << 8));
            double tempC = 26.75 + (rawTemp >> 6) * 0.125;

            // Must read ST2 to acknowledge the measurement and trigger the next cycle
            ReadRegister(Registers.ST2);

            return Task.FromResult(new Ak9753SensorData
            {
                Ir1 = ir1,
                Ir2 = ir2,
                Ir3 = ir3,
                Ir4 = ir4,
                Temperature = new Meadow.Units.Temperature(tempC, Meadow.Units.Temperature.UnitType.Celsius),
            });
        }

        private byte ReadRegister(Registers register)
        {
            BusComms!.ReadRegister((byte)register, ReadBuffer.Span[0..1]);
            return ReadBuffer.Span[0];
        }

        private void WriteRegister(Registers register, byte value)
        {
            WriteBuffer.Span[0] = (byte)register;
            WriteBuffer.Span[1] = value;
            BusComms!.Write(WriteBuffer.Span[0..2]);
        }
    }

    /// <summary>
    /// Represents a single reading from the AK9753 sensor
    /// </summary>
    public struct Ak9753SensorData
    {
        /// <summary>IR channel 1 — Down</summary>
        public short Ir1 { get; set; }
        /// <summary>IR channel 2 — Left</summary>
        public short Ir2 { get; set; }
        /// <summary>IR channel 3 — Up</summary>
        public short Ir3 { get; set; }
        /// <summary>IR channel 4 — Right</summary>
        public short Ir4 { get; set; }
        /// <summary>Internal temperature</summary>
        public Meadow.Units.Temperature Temperature { get; set; }
    }
}

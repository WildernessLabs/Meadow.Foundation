using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.DigiPots
{
    public interface IPotentiometer
    {
        Resistance Resistance { get; set; }
    }

    public class Mcp4162 : Mcp4xx2
    {
        private const int MaxSteps = 257;

        public override Potentiometer[] Rheostats { get; }

        public Mcp4162(ISpiBus spiBus, IDigitalOutputPort? chipSelect, Resistance maxResistance)
            : base(spiBus, chipSelect, 1)
        {
            Rheostats = new Potentiometer[]
            {
                new Potentiometer(
                    Resistors[0],
                    maxResistance,
                    MaxSteps)
            };
        }


    }

    /// <summary>
    /// Represents a Mcp4xx1 digital potentiometer
    /// </summary>
    public abstract class Mcp4xx1 : Mcp4xxx
    {
        public Mcp4xx1(ISpiBus spiBus, IDigitalOutputPort? chipSelect, int resistorCount)
            : base(spiBus, chipSelect, resistorCount)
        {
        }
    }

    /// <summary>
    /// Represents a Mcp4xx2 digital rheostat
    /// </summary>
    public abstract class Mcp4xx2 : Mcp4xxx
    {
        public abstract Potentiometer[] Rheostats { get; }

        public Mcp4xx2(ISpiBus spiBus, IDigitalOutputPort? chipSelect, int resistorCount)
            : base(spiBus, chipSelect, resistorCount)
        {
        }
    }

    /// <summary>
    /// Represents a Mcp4xxx digital potentimeter or rheostat
    /// </summary>
    public abstract class Mcp4xxx : ISpiPeripheral
    {
        protected ISpiCommunications SpiComms { get; }

        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;
        public Frequency DefaultSpiBusSpeed => new Frequency(5, Frequency.UnitType.Megahertz);

        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => SpiComms.BusMode;
            set => SpiComms.BusMode = value;
        }

        public Frequency SpiBusSpeed
        {
            get => SpiComms.BusSpeed;
            set => SpiComms.BusSpeed = value;
        }

        internal Resistor[] Resistors { get; }

        public Mcp4xxx(ISpiBus spiBus, IDigitalOutputPort? chipSelect, int resistorCount)
        {
            if (resistorCount <= 0 || resistorCount > 2) throw new ArgumentException();


            SpiComms = new SpiCommunications(spiBus, chipSelect, DefaultSpiBusSpeed);

            Resistors = new Resistor[resistorCount];
            for (var i = 0; i < resistorCount; i++)
            {
                Resistors[i] = new Resistor(i, SpiComms);
            }
        }

        private const int ReadData = (0x03 << 10);
        private const int WriteData = (0x00 << 10);
        private const int Increment = (0x01 << 2);
        private const int Decrement = (0x01 << 3);
        private const int DataMask8 = 0x03; // 2-bit data
        private const int DataMask16 = (0x01 << 9) - 1; // 9-bit data
        private const int Address_Wiper0 = 0 << 12;
        private const int Address_Wiper1 = 1 << 12;
        private const int Address_TCON = 4 << 12; // terminal control
        private const int Address_Status = 5 << 12;


        public class Potentiometer : IPotentiometer
        {
            private Resistor _resistor;
            private Resistance _maxResistance;
            private int _maxSteps;

            internal Potentiometer(Resistor resistor, Resistance maxResistance, int maxSteps)
            {
                _resistor = resistor;
                _maxResistance = maxResistance;
                _maxSteps = maxSteps;
            }

            public Resistance Resistance
            {
                get
                {
                    var w = _resistor.GetWiper();
                    Resolver.Log.Info($"read w = {w}");
                    return new Resistance((_maxResistance.Ohms / _maxSteps) * w, Resistance.UnitType.Ohms);
                }
                set
                {
                    var w = (short)((_maxSteps / _maxResistance.Ohms) * value.Ohms);
                    _resistor.SetWiper(w);
                }
            }
        }

        internal class Resistor
        {
            private int _index;
            private ISpiCommunications _spiComms;

            public Resistor(int index, ISpiCommunications spiComms)
            {
                _index = index;
                _spiComms = spiComms;
            }

            public short GetWiper()
            {
                short command = (short)(ReadData | (_index == 0 ? Address_Wiper0 : Address_Wiper1) | 0x0ff);

                Span<byte> txBuffer = stackalloc byte[2];
                txBuffer[1] = (byte)(command & 0xff);
                txBuffer[0] = (byte)(command >> 8);
                Span<byte> rxBuffer = stackalloc byte[2];

                // IMPORTANT - the receive data comes in in full duplex (well one bit overlap)
                _spiComms.Exchange(txBuffer, rxBuffer, DuplexType.Full);

                var wiper = (short)((rxBuffer[1] | (rxBuffer[0] << 8)) & DataMask16);
                return wiper;
            }

            public void SetWiper(short value)
            {
                short command = (short)((value & DataMask16) | WriteData | (_index == 0 ? Address_Wiper0 : Address_Wiper1));

                Span<byte> txBuffer = stackalloc byte[2];
                txBuffer[1] = (byte)(command & 0xff); // little endianness shenanigans
                txBuffer[0] = (byte)(command >> 8);

                _spiComms.Write(txBuffer);
            }

            private byte ReadTcon()
            {
                short command = ReadData | Address_TCON;

                Span<byte> txBuffer = stackalloc byte[2];
                txBuffer[1] = (byte)(command & 0xff);
                txBuffer[0] = (byte)(command >> 8);
                Span<byte> rxBuffer = stackalloc byte[2];

                _spiComms.Exchange(txBuffer, rxBuffer);

                return rxBuffer[1];
            }

            private void WriteTcon(byte value)
            {
                short command = (short)(value | WriteData | Address_TCON);

                Span<byte> txBuffer = stackalloc byte[2];
                txBuffer[1] = (byte)(command & 0xff);
                txBuffer[0] = (byte)(command >> 8);

                _spiComms.Write(txBuffer);
            }

            public void EnableA(bool enable)
            {
                var tcon = ReadTcon();

                if (enable)
                {
                    tcon |= (byte)(1 << _index == 0 ? 2 : 6);
                }
                else
                {
                    tcon &= (byte)~(1 << _index == 0 ? 2 : 6);
                }
                WriteTcon(tcon);
            }

            public void EnableB(bool enable)
            {
                var tcon = ReadTcon();

                if (enable)
                {
                    tcon |= (byte)(1 << _index == 0 ? 0 : 4);
                }
                else
                {
                    tcon &= (byte)~(1 << _index == 0 ? 0 : 4);
                }
                WriteTcon(tcon);
            }

            public void EnableWiper(bool enable)
            {
                var tcon = ReadTcon();

                if (enable)
                {
                    tcon |= (byte)(1 << _index == 0 ? 1 : 5);
                }
                else
                {
                    tcon &= (byte)~(1 << _index == 0 ? 1 : 5);
                }
                WriteTcon(tcon);
            }
        }
    }
}
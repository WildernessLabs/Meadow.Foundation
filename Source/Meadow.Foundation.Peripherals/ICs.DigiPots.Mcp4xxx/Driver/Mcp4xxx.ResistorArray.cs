using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.DigiPots;

public abstract partial class Mcp4xxx
{
    /// <summary>
    /// Represents a resistor array in an MCP4xxx digital potentiometer or rheostat.
    /// </summary>
    public class ResistorArray : IPotentiometer, IRheostat
    {
        private const int ReadData = (0x03 << 10);
        private const int WriteData = (0x00 << 10);
        private const int DataMask16 = (0x01 << 9) - 1; // 9-bit data
        private const int Address_Wiper0 = 0 << 12;
        private const int Address_Wiper1 = 1 << 12;

        private Mcp4xxx _parent;
        private int _index;
        private ISpiCommunications _spiComms;
        private EventHandler<IChangeResult<Resistance>>? _changedEvent;
        private Timer? _pollTimer;
        private Resistance? _lastValue;

        /// <inheritdoc/>
        public Resistance MaxResistance => _parent.MaxResistance;

        /// <inheritdoc/>
        public Resistance Resistance
        {
            get
            {
                var w = GetWiper();
                return new Resistance((_parent.MaxResistance.Ohms / _parent.MaxSteps) * w, Resistance.UnitType.Ohms);
            }
            set
            {
                var w = (short)((_parent.MaxSteps / _parent.MaxResistance.Ohms) * value.Ohms);
                SetWiper(w);
            }
        }

        /// <inheritdoc/>
        public event EventHandler<IChangeResult<Resistance>>? Changed
        {
            remove
            {
                _changedEvent -= value;

                if (_changedEvent?.GetInvocationList().Length == 0)
                {
                    _pollTimer?.Dispose();
                    _pollTimer = null;
                }
            }
            add
            {
                _changedEvent += value;
                if (_pollTimer == null)
                {
                    _pollTimer = new Timer(async (_) =>
                    {
                        var r = Resistance;
                        _changedEvent?.Invoke(this, new ChangeResult<Resistance>(r, _lastValue));
                        _lastValue = r;
                        await Task.Delay(500);
                    });
                }
            }
        }

        internal ResistorArray(Mcp4xxx parent, int index, ISpiCommunications spiComms)
        {
            if (index is < 0 or > 1) throw new ArgumentException();

            _parent = parent;
            _index = index;
            _spiComms = spiComms;
        }

        internal void SetWiper(short value)
        {
            var command = (ushort)((value & DataMask16) | WriteData | (_index == 0 ? Address_Wiper0 : Address_Wiper1));

            Span<byte> txBuffer = stackalloc byte[2];
            txBuffer[0] = (byte)(command >> 8);
            txBuffer[1] = (byte)(command & 0xff);

            _spiComms.Write(txBuffer);
        }

        internal short GetWiper()
        {
            var command = (ushort)(ReadData | (_index == 0 ? Address_Wiper0 : Address_Wiper1) | 0xff);

            Span<byte> txBuffer = stackalloc byte[2];
            txBuffer[0] = (byte)(command >> 8);
            txBuffer[1] = (byte)(command & 0xff);
            Span<byte> rxBuffer = stackalloc byte[2];

            _spiComms.Exchange(txBuffer, rxBuffer, DuplexType.Full);

            return (short)((rxBuffer[1] | rxBuffer[0] << 8) & DataMask16);
        }
    }
}

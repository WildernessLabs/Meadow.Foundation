using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Light
{
    /// <summary>
    /// 24-Bit Dual-Channel ADC For Bridge Sensors
    /// </summary>
    public class Hx711 : FilterableChangeObservableBase<FloatChangeResult, float>,  IDisposable
    {
        private const uint GPIO_BASE = 0x40020000;
        private const uint IDR_OFFSET = 0x10;
        private const uint BSSR_OFFSET = 0x18;

        private uint _sckAddress;
        private uint _doutAddress;
        private uint _sck_set;
        private uint _sck_clear;
        private uint _dout_mask;

        private IDigitalOutputPort _sck;
        private IDigitalInputPort _dout;

        private bool _isDisposed;

        private object SyncRoot { get; } = new object();

        public bool IsSleeping { get; private set; }

        /// <summary>
        /// Creates an instance of the NAU7802 Driver class
        /// </summary>
        /// <param name="bus"></param>
        public Hx711(IIODevice device, IPin sck, IPin dout)
        {
            _sck = device.CreateDigitalOutputPort(sck);
            _dout = device.CreateDigitalInputPort(dout);

            CalculateRegisterValues(sck, dout);
            Start();
        }

        public Hx711(IDigitalOutputPort sck, IDigitalInputPort dout)
        {
            _sck = sck;
            _dout = dout;

            CalculateRegisterValues(sck.Pin, dout.Pin);
            Start();
        }

        private void Start()
        {
            ClockLow(); // this resets the chip
            IsSleeping = false;
            _ = ReadProc();
        }

        public void Sleep()
        {
            if (IsSleeping) return;

            lock (SyncRoot)
            {
                ClockHigh();
                IsSleeping = true;
            }
        }

        public void Wake()
        {
            if (!IsSleeping) return;

            lock (SyncRoot)
            {
                ClockHigh();
                IsSleeping = false;
            }
        }

        private async Task ReadProc()
        {
            while(!_isDisposed)
            {
                if(IsSleeping || _dout.State)
                {
                    await Task.Delay(2000);
                }
                else
                {
                    // data is ready
                    var count = ReadData(24);

                    Console.WriteLine($"Count: 0x{count:X8}");
                    await Task.Delay(1000);
                }
            }
        }

        private void CalculateRegisterValues(IPin sck, IPin dout)
        {
            // GPIO_BASE = 0x4002 0000
            // BSSR = 0x18
            // Bits 31:16 clear
            // Bits 15:0  set
            // Port offset = 0x0400 * index (with A being index 0)
            int gpio_port = sck.Key.ToString()[1] - 'A';
            int gpio_pin = int.Parse(sck.Key.ToString().Substring(2));
            _sckAddress = GPIO_BASE | (0x400u * (uint)gpio_port) | BSSR_OFFSET;
            _sck_set = 1u << gpio_pin;
            _sck_clear = 1u << (16 + gpio_pin);

            gpio_port = dout.Key.ToString()[1] - 'A';
            gpio_pin = int.Parse(dout.Key.ToString().Substring(2));
            _doutAddress = GPIO_BASE | (0x400u * (uint)gpio_port) | IDR_OFFSET;
            _dout_mask = 1u << gpio_pin;
        }

        private unsafe void ClockLow()
        {
            *(uint*)_sckAddress = _sck_clear; // low
        }

        private unsafe void ClockHigh()
        {
            *(uint*)_sckAddress = _sck_set; // high
        }

        private unsafe ulong ReadData(int cycles)
        {
            ulong count = 0;

            lock (SyncRoot)
            {
                for (int i = 0; i < cycles; i++)
                {
                    ClockHigh();
                    ClockHigh();
                    ClockHigh();
                    count = count << 1;
                    ClockLow();
                    ClockLow();
                    ClockLow();

                    if ((*(uint*)_doutAddress & _dout_mask) != 0) // read DOUT state
                    {
                        count++;
                    }
                }
            }
            count = count ^ 0x800000;
            return count;
        }

        protected virtual void Dispose(bool disposing)
        {
            _isDisposed = true;
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
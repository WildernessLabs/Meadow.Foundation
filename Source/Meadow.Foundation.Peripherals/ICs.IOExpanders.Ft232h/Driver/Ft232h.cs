using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents a DS3502 digital potentiometer
    /// </summary>
    public partial class Ft232h : IDisposable, IIoDevice
    {
        private bool _isDisposed;
        private static int _instanceCount = 0;
        private Dictionary<int, Ft232I2cBus> _i2cBuses = new Dictionary<int, Ft232I2cBus>();
        private Dictionary<int, Ft232SpiBus> _spiBuses = new Dictionary<int, Ft232SpiBus>();
        private IFt232Bus? _activeBus = null;

        static Ft232h()
        {
            if (Interlocked.Increment(ref _instanceCount) == 1)
            {
                // only do this one time (no matter how many instances are created instances)
                Native.Functions.Init_libMPSSE();
            }
        }

        public Ft232h()
        {
            EnumerateBuses();
        }

        /// <summary>
        /// The pins
        /// </summary>
        public PinDefinitions Pins { get; } = new PinDefinitions()

        ; private void EnumerateBuses()
        {
            _i2cBuses = GetI2CBuses();
            _spiBuses = GetSpiBuses();
        }

        private Dictionary<int, Ft232I2cBus> GetI2CBuses()
        {
            Dictionary<int, Ft232I2cBus> result = new Dictionary<int, Ft232I2cBus>();

            if (Native.CheckStatus(Native.Functions.I2C_GetNumChannels(out int channels)))
            {
                if (channels > 0)
                {
                    for (var c = 0; c < channels; c++)
                    {
                        if (Native.CheckStatus(Native.Functions.I2C_GetChannelInfo(c, out Native.FT_DEVICE_LIST_INFO_NODE info)))
                        {
                            result.Add(c, new Ft232I2cBus(c, info));
                        }
                    }
                }
            }

            return result;
        }

        private Dictionary<int, Ft232SpiBus> GetSpiBuses()
        {
            Dictionary<int, Ft232SpiBus> result = new Dictionary<int, Ft232SpiBus>();

            if (Native.CheckStatus(Native.Functions.SPI_GetNumChannels(out int channels)))
            {
                if (channels > 0)
                {
                    for (var c = 0; c < channels; c++)
                    {
                        if (Native.CheckStatus(Native.Functions.SPI_GetChannelInfo(c, out Native.FT_DEVICE_LIST_INFO_NODE info)))
                        {
                            result.Add(c, new Ft232SpiBus(c, info));
                        }
                    }
                }
            }

            return result;
        }

        public II2cBus CreateI2cBus(int busNumber = 0)
        {
            return CreateI2cBus(busNumber, I2CClockRate.Standard);
        }

        public II2cBus CreateI2cBus(int busNumber, Frequency frequency)
        {
            // TODO: convert frequency
            return CreateI2cBus(busNumber, I2CClockRate.Standard);
        }

        public II2cBus CreateI2cBus(IPin[] pins, Frequency frequency)
        {
            // TODO: map the pins to the bus number
            // TODO: convert frequency
            return CreateI2cBus(0, I2CClockRate.Standard);
        }

        public II2cBus CreateI2cBus(IPin clock, IPin data, Frequency frequency)
        {
            // TODO: map the pins to the bus number
            // TODO: convert frequency
            return CreateI2cBus(0, I2CClockRate.Standard);
        }

        private II2cBus CreateI2cBus(int busNumber, I2CClockRate clock)
        {
            if (_activeBus != null)
            {
                throw new InvalidOperationException("The FT232 allows only one bus to be active at a time.");
            }

            if (!_i2cBuses.ContainsKey(busNumber)) throw new ArgumentOutOfRangeException(nameof(busNumber));

            var bus = _i2cBuses[busNumber];
            if (!bus.IsOpen)
            {
                bus.Open(clock);
            }

            _activeBus = bus;

            return bus;
        }

        public ISpiBus CreateSpiBus()
        {
            return CreateSpiBus(0, DefaultClockConfiguration);
        }

        public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, SpiClockConfiguration config)
        {
            if (!clock.Supports<ISpiChannelInfo>(c => c.LineTypes.HasFlag(SpiLineType.Clock)))
            {
                throw new ArgumentException("Invalid Clock line");
            }

            // TODO: map the pins to the bus number
            return CreateSpiBus(0, config);
        }

        public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, Frequency speed)
        {
            // TODO: map the pins to the bus number
            var config = new SpiClockConfiguration(speed);
            return CreateSpiBus(0, config);
        }

        public static SpiClockConfiguration DefaultClockConfiguration
        {
            get => new SpiClockConfiguration(
                 new Frequency(Ft232SpiBus.DefaultClockRate, Frequency.UnitType.Hertz));
        }

        private ISpiBus CreateSpiBus(int busNumber, SpiClockConfiguration config)
        {
            if (_activeBus != null)
            {
                throw new InvalidOperationException("The FT232 allows only one bus to be active at a time.");
            }

            if (!_spiBuses.ContainsKey(busNumber)) throw new ArgumentOutOfRangeException(nameof(busNumber));

            var bus = _spiBuses[busNumber];
            if (!bus.IsOpen)
            {
                bus.Open(config);
            }

            _activeBus = bus;

            return bus;
        }

        public IDigitalInputPort CreateDigitalInputPort(IPin pin)
        {
            return CreateDigitalInputPort(pin, InterruptMode.None, ResistorMode.Disabled, TimeSpan.Zero, TimeSpan.Zero);
        }

        private bool _spiBusAutoCreated = false;

        public IDigitalInputPort CreateDigitalInputPort(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, TimeSpan debounceDuration, TimeSpan glitchDuration)
        {
            if (interruptMode != InterruptMode.None)
            {
                throw new NotSupportedException("The FT232 does not support interrupts");
            }

            // MPSSE requires a bus, it can be either I2C or SPI, but that bus must be created before you can use GPIO
            // if no bus is yet open, we'll default to a SPI bus.
            // If this is created before an I2C comms bus, we need to let the caller know to create the comms bus first

            if (_activeBus == null)
            {
                var bus = CreateSpiBus(0, DefaultClockConfiguration);
                _spiBusAutoCreated = true;
                _activeBus = bus as IFt232Bus;
            }

            // TODO: do we need to set the direction make (see outpuuts) or are they defaulted to input?

            var info = pin.SupportedChannels?.FirstOrDefault(c => c is IDigitalChannelInfo) as IDigitalChannelInfo;
            return new Ft232DigitalInputPort(pin, info, _activeBus);
        }

        public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
        {
            // MPSSE requires a bus, it can be either I2C or SPI, but that bus must be created before you can use GPIO
            // if no bus is yet open, we'll default to a SPI bus.
            // If this is created before an I2C comms bus, we need to let the caller know to create the comms bus first

            if (_activeBus == null)
            {
                var bus = CreateSpiBus(0, DefaultClockConfiguration);
                _spiBusAutoCreated = true;
                _activeBus = bus as IFt232Bus;
            }

            // update the global mask to make this an output
            _activeBus.GpioDirectionMask |= (byte)(1 << (byte)pin.Key);

            // update the direction
            Native.Functions.FT_WriteGPIO(_activeBus.Handle, _activeBus.GpioDirectionMask, 0);

            var info = pin.SupportedChannels?.FirstOrDefault(c => c is IDigitalChannelInfo) as IDigitalChannelInfo;
            return new Ft232DigitalOutputPort(pin, info, initialState, initialOutputType, _activeBus);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var bus in _i2cBuses)
                {
                    bus.Value?.Dispose();
                }

                if (Interlocked.Decrement(ref _instanceCount) == 0)
                {
                    // last instance was disposed, clean house
                    Native.Functions.Cleanup_libMPSSE();
                }

                _isDisposed = true;
            }
        }

        ~Ft232h()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
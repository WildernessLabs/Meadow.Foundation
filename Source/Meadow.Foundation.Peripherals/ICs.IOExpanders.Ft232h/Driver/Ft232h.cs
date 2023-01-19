using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
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
        private Dictionary<int, Ft232I2cBus> _i2cBusses = new Dictionary<int, Ft232I2cBus>();

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
            EnumerateBusses();
        }

        /// <summary>
        /// The pins
        /// </summary>
        public PinDefinitions Pins { get; } = new PinDefinitions()

        ; private void EnumerateBusses()
        {
            _i2cBusses = GetI2CChannels();
        }

        private Dictionary<int, Ft232I2cBus> GetI2CChannels()
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

        public II2cBus CreateI2cBus(int busNumber = 0)
        {
            if (!_i2cBusses.ContainsKey(busNumber)) throw new ArgumentOutOfRangeException(nameof(busNumber));

            var bus = _i2cBusses[busNumber];
            if (!bus.IsOpen)
            {
                bus.Open();
            }
            return bus;
        }

        public IDigitalInputPort CreateDigitalInputPort(IPin pin)
        {
            return CreateDigitalInputPort(pin, InterruptMode.None, ResistorMode.Disabled, TimeSpan.Zero, TimeSpan.Zero);
        }










        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                foreach (var bus in _i2cBusses)
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

        public SpiChannel[] GetSpiChannels()
        {
            SpiChannel[] result;

            if (Native.CheckStatus(Native.Functions.SPI_GetNumChannels(out int channels)))
            {
                result = new SpiChannel[channels];
                if (channels > 0)
                {
                    for (var c = 0; c < channels; c++)
                    {
                        if (Native.CheckStatus(Native.Functions.SPI_GetChannelInfo(c, out Native.FT_DEVICE_LIST_INFO_NODE info)))
                        {
                            result[c] = new SpiChannel(c, info);
                        }
                    }
                }
            }
            else
            {
                result = new SpiChannel[0];
            }

            return result;
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

        public II2cBus CreateI2cBus(int busNumber, Frequency frequency)
        {
            throw new NotImplementedException();
        }

        public II2cBus CreateI2cBus(IPin[] pins, Frequency frequency)
        {
            throw new NotImplementedException();
        }

        public II2cBus CreateI2cBus(IPin clock, IPin data, Frequency frequency)
        {
            throw new NotImplementedException();
        }

        public IDigitalInputPort CreateDigitalInputPort(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, TimeSpan debounceDuration, TimeSpan glitchDuration)
        {
            throw new NotImplementedException();
        }

        public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, SpiClockConfiguration config)
        {
            throw new NotImplementedException();
        }

        public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, Frequency speed)
        {
            throw new NotImplementedException();
        }

        public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
        {
            throw new NotImplementedException();
        }
    }
}
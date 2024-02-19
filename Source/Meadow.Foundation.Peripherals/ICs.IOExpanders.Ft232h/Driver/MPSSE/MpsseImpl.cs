using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders;

internal class MpsseImpl : IFtdiImpl
{
    private static int _instanceCount = 0;
    private bool _isDisposed = false;
    private Dictionary<int, MpsseI2cBus> _i2cBuses = new Dictionary<int, MpsseI2cBus>();
    private Dictionary<int, MpsseSpiBus> _spiBuses = new Dictionary<int, MpsseSpiBus>();
    private bool _spiBusAutoCreated = false;

    private IFt232Bus? _activeBus = null;

    public void Initialize()
    {
        if (Interlocked.Increment(ref _instanceCount) == 1)
        {
            // only do this one time (no matter how many instances are created instances)
            Native.Mpsse.Init_libMPSSE();
        }

        EnumerateBuses();
    }

    private void EnumerateBuses()
    {
        _i2cBuses = GetI2CBuses();
        _spiBuses = GetSpiBuses();
    }

    private Dictionary<int, MpsseI2cBus> GetI2CBuses()
    {
        Dictionary<int, MpsseI2cBus> result = new Dictionary<int, MpsseI2cBus>();

        if (Native.CheckStatus(Native.Mpsse.I2C_GetNumChannels(out int channels)))
        {
            if (channels > 0)
            {
                for (var c = 0; c < channels; c++)
                {
                    if (Native.CheckStatus(Native.Mpsse.I2C_GetChannelInfo(c, out Native.FT_DEVICE_LIST_INFO_NODE info)))
                    {
                        result.Add(c, new MpsseI2cBus(c, info));
                    }
                }
            }
        }

        return result;
    }

    private Dictionary<int, MpsseSpiBus> GetSpiBuses()
    {
        Dictionary<int, MpsseSpiBus> result = new Dictionary<int, MpsseSpiBus>();

        if (Native.CheckStatus(Native.Mpsse.SPI_GetNumChannels(out int channels)))
        {
            if (channels > 0)
            {
                for (var c = 0; c < channels; c++)
                {
                    if (Native.CheckStatus(Native.Mpsse.SPI_GetChannelInfo(c, out Native.FT_DEVICE_LIST_INFO_NODE info)))
                    {
                        result.Add(c, new MpsseSpiBus(c, info));
                    }
                }
            }
        }

        return result;
    }

    public II2cBus CreateI2cBus(int busNumber, I2CClockRate clock)
    {
        // dev note: this fails on WIndows in all my testing
        //           it's a bug in the MPSSE DLL delivered by FTDI
        //           even using their C example, compiling it myself, it fails

        if (_activeBus != null)
        {
            throw new InvalidOperationException("The FT232 allows only one bus to be active at a time.");
        }

        if (_i2cBuses.Count == 0)
        {
            throw new InvalidOperationException("No I2C Busses found! Is the FT232 properly connected?");
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

    public ISpiBus CreateSpiBus(int busNumber, SpiClockConfiguration config)
    {
        if (_activeBus != null)
        {
            throw new InvalidOperationException("The FT232 allows only one bus to be active at a time.");
        }

        if (_spiBuses.Count == 0)
        {
            throw new InvalidOperationException("No SPI Busses found! Is the FT232 properly connected?");
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

    public IDigitalInputPort CreateDigitalInputPort(int channel, IPin pin, ResistorMode resistorMode)
    {
        // MPSSE requires a bus, it can be either I2C or SPI, but that bus must be created before you can use GPIO
        // if no bus is yet open, we'll default to a SPI bus.
        // If this is created before an I2C comms bus, we need to let the caller know to create the comms bus first

        if (_activeBus == null)
        {
            var bus = CreateSpiBus(channel, Ft232h.DefaultClockConfiguration);
            _spiBusAutoCreated = true;
            _activeBus = bus as IFt232Bus;
        }

        // TODO: do we need to set the direction make (see outpuuts) or are they defaulted to input?

        var info = pin.SupportedChannels?.FirstOrDefault(c => c is IDigitalChannelInfo) as IDigitalChannelInfo;
        return new MpsseDigitalInputPort(pin, info!, _activeBus!);
    }

    public IDigitalOutputPort CreateDigitalOutputPort(int channel, IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        // MPSSE requires a bus, it can be either I2C or SPI, but that bus must be created before you can use GPIO
        // if no bus is yet open, we'll default to a SPI bus.
        // If this is created before an I2C comms bus, we need to let the caller know to create the comms bus first

        if (_activeBus == null)
        {
            var bus = CreateSpiBus(channel, Ft232h.DefaultClockConfiguration);
            _spiBusAutoCreated = true;
            _activeBus = bus as IFt232Bus;
        }

        // update the global mask to make this an output
        _activeBus!.GpioDirectionMask |= (byte)pin.Key;

        // update the direction
        Native.Mpsse.FT_WriteGPIO(_activeBus.Handle, _activeBus.GpioDirectionMask, 0);

        var info = pin.SupportedChannels?.FirstOrDefault(c => c is IDigitalChannelInfo) as IDigitalChannelInfo;
        return new MpsseDigitalOutputPort(pin, info!, initialState, initialOutputType, _activeBus);
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            foreach (var bus in _spiBuses)
            {
                bus.Value?.Dispose();
            }

            if (Interlocked.Decrement(ref _instanceCount) == 0)
            {
                if (_spiBusAutoCreated)
                {
                    // TODO:
                }

                // last instance was disposed, clean house
                Native.Mpsse.Cleanup_libMPSSE();
            }

            _isDisposed = true;
        }
    }
}

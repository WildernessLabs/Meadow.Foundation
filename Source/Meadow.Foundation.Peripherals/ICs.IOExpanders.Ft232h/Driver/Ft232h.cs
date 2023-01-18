using System;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents a DS3502 digital potentiometer
    /// </summary>
    public partial class Ft232h : IDisposable
    {
        private bool _isDisposed;
        private static int _instanceCount = 0;

        static Ft232h()
        {
            if (Interlocked.Increment(ref _instanceCount) == 1)
            {
                // only do this one time (no matter how many instances are created instances)
                Native.Functions.Init_libMPSSE();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (Interlocked.Decrement(ref _instanceCount) == 0)
                {
                    // last instance was disposed, clean house
                    Native.Functions.Cleanup_libMPSSE();
                }
                _isDisposed = true;
            }
        }

        public I2cChannel[] GetI2CChannels()
        {
            I2cChannel[] result;

            if (Native.CheckStatus(Native.Functions.I2C_GetNumChannels(out int channels)))
            {
                result = new I2cChannel[channels];
                if (channels > 0)
                {
                    for (var c = 0; c < channels; c++)
                    {
                        if (Native.CheckStatus(Native.Functions.I2C_GetChannelInfo(c, out Native.FT_DEVICE_LIST_INFO_NODE info)))
                        {
                            result[c] = new I2cChannel(c, info);
                        }
                    }
                }
            }
            else
            {
                result = new I2cChannel[0];
            }

            return result;
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
    }
}
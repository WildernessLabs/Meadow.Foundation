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

        public void EnumI2CBus()
        {
            if (CheckStatus(Native.Functions.I2C_GetNumChannels(out int channels)))
            {
                Console.WriteLine($"{channels} I2C Channels");

                for (var c = 0; c < channels; c++)
                {
                    if (CheckStatus(Native.Functions.I2C_GetChannelInfo(c, out Native.FT_DEVICE_LIST_INFO_NODE info)))
                    {
                        Console.WriteLine($"Serial #:       {info.SerialNumber}");
                        Console.WriteLine($"Description #:  {info.Description}");
                    }
                }
            }
        }

        private bool CheckStatus(Native.FT_STATUS status)
        {
            if (status == Native.FT_STATUS.FT_OK)
            {
                return true;
            }

            throw new Exception($"Native error: {status}");
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
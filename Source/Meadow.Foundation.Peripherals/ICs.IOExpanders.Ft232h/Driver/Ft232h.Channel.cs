using System;
using static Meadow.Foundation.ICs.IOExpanders.Native;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ft232h
    {
        public abstract class Channel : IDisposable
        {
            protected IntPtr Handle { get; set; }

            protected int ChannelNumber { get; }

            private FT_DEVICE_LIST_INFO_NODE _infoNode;
            private bool _isDisposed;

            internal Channel(int channelNumber, FT_DEVICE_LIST_INFO_NODE infoNode)
            {
                _infoNode = infoNode;
                ChannelNumber = channelNumber;
            }

            public string SerialNumber => _infoNode.SerialNumber;
            public string Description => _infoNode.Description;

            protected abstract void CloseChannel();

            private void Dispose(bool disposing)
            {
                if (!_isDisposed)
                {
                    CloseChannel();
                    _isDisposed = true;
                }
            }

            ~Channel()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(false);
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
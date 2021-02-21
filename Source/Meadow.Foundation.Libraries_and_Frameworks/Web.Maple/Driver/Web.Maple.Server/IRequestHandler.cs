using System;
using System.Net;

namespace Meadow.Foundation.Maple
{
    public interface IRequestHandler : IDisposable
    {
        HttpListenerContext Context { get; set; }
    }
}

using System;
using System.Net;

namespace Meadow.Foundation.Web.Maple.Server
{
    public interface IRequestHandler : IDisposable
    {
        HttpListenerContext Context { get; set; }
    }
}

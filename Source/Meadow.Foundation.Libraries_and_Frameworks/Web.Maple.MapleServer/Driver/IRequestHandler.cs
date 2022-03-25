using System;
using System.Net;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple
{
    public interface IRequestHandler : IDisposable
    {
        HttpListenerContext Context { get; set; }
        bool IsReusable { get; }
    }
}

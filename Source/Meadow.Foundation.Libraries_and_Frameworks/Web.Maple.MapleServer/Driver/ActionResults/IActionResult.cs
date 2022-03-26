using System.Net;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple
{
    public interface IActionResult
    {
        void ExecuteResult(HttpListenerContext context);
        Task ExecuteResultAsync(HttpListenerContext context);
    }
}
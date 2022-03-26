using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple
{
    public class ErrorPageGenerator
    {
        public async Task SendErrorPage(HttpListenerContext context, int statusCode)
        {
            // TODO: load up custom error page templates
            var body = $"<head><body>{statusCode}</body><head>";

            await SendPage(context, statusCode, body);
        }

        public async Task SendErrorPage(HttpListenerContext context, int statusCode, string message)
        {
            // TODO: load up custom error page templates
            var body = $"<head><body>{statusCode}<p>{message}</body><head>";

            await SendPage(context, statusCode, body);
        }

        public async Task SendErrorPage(HttpListenerContext context, int statusCode, Exception ex)
        {
            // TODO: load up custom error page templates
            var body = $"<head><body>{statusCode}<p>{ex}</body><head>";

            await SendPage(context, statusCode, body);
        }

        private async Task SendPage(HttpListenerContext context, int statusCode, string body)
        {
            byte[] data = Encoding.UTF8.GetBytes(body);
            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = data.LongLength;
            context.Response.StatusCode = statusCode;
            await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
            context.Response.Close();
        }
    }
}
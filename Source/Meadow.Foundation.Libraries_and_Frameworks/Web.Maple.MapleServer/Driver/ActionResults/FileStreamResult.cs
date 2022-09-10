using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple
{
    public class FileStreamResult : FileResult
    {
        private Stream _fileStream;

        public FileStreamResult(Stream fileStream, string contentType)
                : base(contentType)
        {
            _fileStream = fileStream;
        }

        /// <summary>
        /// Gets or sets the file contents.
        /// </summary>
        public Stream FileStream
        {
            get => _fileStream;
            set => _fileStream = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override void ExecuteResult(HttpListenerContext context)
        {
            ExecuteResultAsync(context).Wait();
        }

        public override async Task ExecuteResultAsync(HttpListenerContext context)
        {
            context.Response.ContentType = ContentType;
            await WriteOutputStream(context, _fileStream);
        }

        protected async Task WriteOutputStream(HttpListenerContext context, Stream data)
        {
            context.Response.ContentEncoding = Encoding.UTF8;
            if (data == null)
            {
                context.Response.ContentLength64 = 0;
            }
            else
            {
                context.Response.ContentLength64 = data.Length;
                await data.CopyToAsync(context.Response.OutputStream);
            }
            context.Response.Close();
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple
{
    public class FileContentResult : FileResult
    {
        private byte[] _fileContents;

        public FileContentResult(byte[] fileContents, string contentType)
                : base(contentType)
        {
            _fileContents = fileContents;
        }

        /// <summary>
        /// Gets or sets the file contents.
        /// </summary>
        public byte[] FileContents
        {
            get => _fileContents;
            set => _fileContents = value ?? throw new ArgumentNullException(nameof(value));

        }

        public override void ExecuteResult(HttpListenerContext context)
        {
            ExecuteResultAsync(context).Wait();
        }

        public override async Task ExecuteResultAsync(HttpListenerContext context)
        {
            context.Response.ContentType = ContentType;
            await WriteOutputStream(context, _fileContents);
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
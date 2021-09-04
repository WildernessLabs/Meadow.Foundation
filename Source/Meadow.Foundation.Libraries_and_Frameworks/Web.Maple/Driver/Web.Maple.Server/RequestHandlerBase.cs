using System;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple.Server
{
    // TODO:
    // This class hasn't had a full porting pass to modern .NET.
    //  * Hashtable -> IDictionary? StringDictionary?
    //  * buffer -> Span<byte>?
    public abstract class RequestHandlerBase : IRequestHandler
    {
        private const int bufferSize = 4096;

        private HttpListenerContext _context;
        private bool _disposedValue;

        protected StringDictionary QueryString { get; private set; }
        protected StringDictionary FormFields { get; set; }
        protected string Body { get; set; }
        public virtual bool IsReusable { get; } = false;

        protected RequestHandlerBase()
        {
            Body = string.Empty;
            QueryString = new StringDictionary();
            FormFields = new StringDictionary();
        }

        public HttpListenerContext Context
        {
            get => _context;
            set
            {
                _context = value;

                if (_context.Request.RawUrl.Split(new char[] { '?' }).Length > 1)
                {
                    var q = _context.Request.RawUrl.Split(new char[] { '?' })[1];
                    QueryString = ParseUrlPairs(q);
                }

                switch (_context.Request.ContentType)
                {
                    case ContentTypes.Application_Text:
                        Body = ReadInputStream();
                        break;
                    case ContentTypes.Application_Form_UrlEncoded:
                        FormFields = ParseUrlPairs(ReadInputStream());
                        break;
                    case ContentTypes.Application_Json:
                        Body = ReadInputStream();
                        break;
                    default:
                        Body = ReadInputStream();
                        break;
                }
            }
        }

        private int Min(int a, int b)
        {
            return ((a <= b) ? a : b);
        }

        private string ReadInputStream()
        {
            var len = (int)Context.Request.ContentLength64;
            var buffer = new byte[bufferSize];
            var result = string.Empty;

            int i = 0;
            while (i * bufferSize <= len)
            {
                int min = Min(bufferSize, (len - (i * bufferSize)));
                Context.Request.InputStream.Read(buffer, 0, min);
                result += new String(Encoding.UTF8.GetChars(buffer, 0, min));
                i++;
            }

            return result;
        }

        private async Task WriteOutputStream(byte[] data)
        {
            Context.Response.ContentEncoding = Encoding.UTF8;
            Context.Response.ContentLength64 = data.LongLength;

            await Context.Response.OutputStream.WriteAsync(data, 0, data.Length);
            Context.Response.Close();
        }

        private StringDictionary ParseUrlPairs(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            var pairs = s.Split(new char[] { '&' });
            var result = new StringDictionary();
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split(new char[] { '=' });
                result.Add(keyValue[0], keyValue[1]);
            }
            return result;
        }

        public async void Send()
        {
            await Send(null);
        }

        public async Task Send(object output)
        {
            if (Context.Response.ContentType == ContentTypes.Application_Json)
            {
                var json = SimpleJsonSerializer.JsonSerializer.SerializeObject(output);
                await WriteOutputStream(Encoding.UTF8.GetBytes(json));
            }
            else
            {
                await WriteOutputStream(Encoding.UTF8.GetBytes(output != null ? output.ToString() : string.Empty));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~RequestHandlerBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
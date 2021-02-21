using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple.Server
{
    public abstract class RequestHandlerBase : IRequestHandler
    {
        private const int bufferSize = 4096;
        protected Hashtable QueryString { get; set; }
        protected Hashtable Form { get; set; }
        protected Hashtable Body { get; set; }
        protected bool disposedValue;

        protected RequestHandlerBase()
        {
            Body = new Hashtable();
            QueryString = new Hashtable();
            Form = new Hashtable();
        }

        public HttpListenerContext Context {
            get { return _context; }
            set {
                _context = value;

                if (_context.Request.RawUrl.Split('?').Length > 1) {
                    var q = _context.Request.RawUrl.Split('?')[1];
                    QueryString = ParseUrlPairs(q);
                }

                switch (_context.Request.ContentType) {
                    case ContentTypes.Application_Form_UrlEncoded:
                        Form = ParseUrlPairs(ReadInputStream());
                        break;
                    case ContentTypes.Application_Json:
                        Body = JsonSerializer.Deserialize<Hashtable>(ReadInputStream());
                        break;
                }
            }
        } protected HttpListenerContext _context;

        protected async void Send()
        {
            await Send(null);
        }

        private string ReadInputStream()
        {
            var len = (int)Context.Request.ContentLength64;
            var buffer = new byte[bufferSize];
            var result = string.Empty;

            int i = 0;
            while (i * bufferSize <= len) {
                int min = Min(bufferSize, (len - (i * bufferSize)));
                Context.Request.InputStream.Read(buffer, 0, min);
                result += new String(Encoding.UTF8.GetChars(buffer, 0, min));
                i++;
            }

            return result;
        }

        protected async Task Send(object output)
        {
            if (Context.Response.ContentType == ContentTypes.Application_Json) {
                var json = JsonSerializer.Serialize(output);
                await WriteOutputStream(Encoding.UTF8.GetBytes(json));
            } else {
                // default is to process output as a string
                await WriteOutputStream(Encoding.UTF8.GetBytes(output != null ? output.ToString() : string.Empty));
            }
        }

        private async Task WriteOutputStream(byte[] data)
        {
            Context.Response.ContentEncoding = Encoding.UTF8;
            Context.Response.ContentLength64 = data.LongLength;
            
            await Context.Response.OutputStream.WriteAsync(data, 0, data.Length);

            //int i = 0;
            //while (i * bufferSize <= data.Length) {
            //    int min = Min(bufferSize, data.Length - (i * bufferSize));
            //    Array.Copy(data, i * bufferSize, buffer, 0, min);
            //    Context.Response.OutputStream.Write(buffer, 0, min);
            //    i++;
            //}
            //Context.Response.OutputStream.Flush();

            // correct place?
            Context.Response.Close();
        }

        private Hashtable ParseUrlPairs(string s)
        {
            if (s == null || s.IndexOf('&') == -1)
                return null;

            var pairs = s.Split('&');
            var result = new Hashtable(pairs.Length);
            foreach (var pair in pairs) {
                var keyValue = pair.Split('=');
                result.Add(keyValue[0], keyValue[1]);
            }
            return result;
        }

        private int Min(int a, int b)
        {
            return ((a <= b) ? a : b);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
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

using System;
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

        private bool _disposedValue;
        private HttpListenerContext _context;

        protected StringDictionary QueryString { get; private set; }
        public virtual bool IsReusable { get; } = false;

        protected RequestHandlerBase()
        {
        }

        public HttpListenerContext Context
        {
            get => _context;
            set
            {
                _context = value;
                var i = _context.Request.RawUrl.IndexOf('?');
                if(i >= 0)
                {
                    var q = _context.Request.RawUrl.Substring(i + 1);
                    QueryString = ParseUrlPairs(q);
                }
            }
        }

        protected string Body 
        {
            get
            {
                switch (Context.Request.ContentType)
                {
                    case ContentTypes.Application_Text:
                    case ContentTypes.Application_Json:
                        return ReadInputStream();
                }

                return null;
            }
        }

        protected StringDictionary FormFields 
        {
            get
            {
                switch (Context.Request.ContentType)
                {
                    case ContentTypes.Application_Form_UrlEncoded:
                        return ParseUrlPairs(ReadInputStream());
                }

                return null;
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
                switch(keyValue.Length)
                {
                    case 1:
                        result.Add(HttpUtility.UrlDecode(keyValue[0]), null);
                        break;
                    case 2:
                        result.Add(HttpUtility.UrlDecode(keyValue[0]), HttpUtility.UrlDecode(keyValue[1]));
                        break;
                }
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
                // TODO: creating the strategy on every call seems like bad form
                var json = SimpleJson.SimpleJson.SerializeObject(output, new MapleSerializationStrategy());
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
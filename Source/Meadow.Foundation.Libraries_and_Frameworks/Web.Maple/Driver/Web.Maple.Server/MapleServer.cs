using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Foundation.Web.Maple.Server.Routing;

namespace Meadow.Foundation.Web.Maple.Server
{
    /// <summary>
    /// A lightweight web server.
    /// </summary>
    public partial class MapleServer
    {
        public const int MAPLE_SERVER_BROADCASTPORT = 17756;
        public const int DefaultPort = 5417;

        private bool _printDebugOutput = true;
        private RequestMethodCache _methodCache = new RequestMethodCache();

        private Dictionary<MethodInfo, IRequestHandler> _handlerCache = new Dictionary<MethodInfo, IRequestHandler>();
        private readonly HttpListener _httpListener = new HttpListener();
        private readonly IList<Type> _requestHandlers = new List<Type>();

        public IPAddress IPAddress { get; private set; }
        public int Port { get; private set; }

        /// <summary>
        /// Whether or not the server is listening for requests.
        /// </summary>
        public bool Running { get; protected set; } = false;

        /// <summary>
        /// Whether the server should operate on requests serially or in parallel.
        /// </summary>
        public RequestProcessMode ThreadingMode { get; protected set; }

        /// <summary>
        /// Whether or not the server should advertise it's name
        /// and IP via UDP for discovery.
        /// </summary>
        public bool Advertise { get; protected set; } = false;

        /// <summary>
        /// The interval, in milliseconds of how often to advertise.
        /// </summary>
        public int AdvertiseIntervalMs { get; set; } = 2000;

        /// <summary>
        /// The name of the device to advertise via UDP.
        /// </summary>
        public string DeviceName { get; set; } = "Meadow";

        public MapleServer(
            string ipAddress,
            int port = DefaultPort,
            bool advertise = false,
            RequestProcessMode processMode = RequestProcessMode.Serial)
        {
            Create(IPAddress.Parse(ipAddress), port, advertise, processMode);
        }

        /// <summary>
        /// Creates a new MapleServer that listens on the specified IP Address
        /// and Port.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port">Defaults to 5417.</param>
        /// <param name="advertise">Whether or not to advertise via UDP.</param>
        /// <param name="processMode">Whether or not the server should respond to
        /// requests in parallel or serial. For Meadow, only Serial works
        /// reliably today.</param>
        public MapleServer(
        IPAddress ipAddress,
        int port = DefaultPort,
        bool advertise = false,
        RequestProcessMode processMode = RequestProcessMode.Serial)
        {
            Create(ipAddress, port, advertise, processMode);
        }

        private void Create(IPAddress ipAddress,
        int port,
        bool advertise,
        RequestProcessMode processMode)
        {
            IPAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            Port = port;

            Advertise = advertise;
            ThreadingMode = processMode;

            if (IPAddress.Equals(IPAddress.Any))
            {
                // because .NET is apparently too stupid to understand "bind to all"
                foreach (var ni in NetworkInterface
                    .GetAllNetworkInterfaces()
                    .SelectMany(i => i.GetIPProperties().UnicastAddresses))
                {
                    if (ni.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        // for now, just use IPv4
                        if (_printDebugOutput)
                        {
                            Console.WriteLine($"Listening on http://{ni.Address}:{port}/");
                        }

                        _httpListener.Prefixes.Add($"http://{ni.Address}:{port}/");
                    }
                }
            }
            else
            {
                if (_printDebugOutput)
                {
                    Console.WriteLine($"Listening on http://{IPAddress}:{port}/");
                }
                _httpListener.Prefixes.Add($"http://{IPAddress}:{port}/");
            }

            LoadRequestHandlers();

            Initialize();
        }

        protected void Initialize()
        {
            
        }

        /// <summary>
        /// Starts listening to requests, and optionally advertises on UDP.
        /// </summary>
        public async void Start()
        {
            try
            {
                _httpListener.Start();
            }
            catch (HttpListenerException)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    throw new Exception(
                        $"The server application needs elevated privileges or you must open permission on the URL (e.g. `netsh http add urlacl url=http://{IPAddress}:{Port}/ user=DOMAIN\\user`)");
                }

                throw;
            }

            if (Advertise)
            {
                StartUdpAdvertisement();
            }
            await StartListeningToIncomingRequests();
            _httpListener.Close();
        }

        /// <summary>
        /// Stops listening to requests and advertising (if running).
        /// </summary>
        public void Stop()
        {
            Running = false;
        }

        //public void AddHandler(IRequestHandler handler)
        //{
        //    requestHandlers.Add(handler);
        //}

        //public void RemoveHandler(IRequestHandler handler)
        //{
        //    requestHandlers.Remove(handler);
        //}

        /// <summary>
        /// Begins advertising the server name and IP via UDP.
        /// </summary>
        protected void StartUdpAdvertisement()
        {
            Task.Run(() =>
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), MAPLE_SERVER_BROADCASTPORT);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

                    string broadcastData = $"{DeviceName}::{IPAddress}";

                    while (Running)
                    {
                        socket.SendTo(UTF8Encoding.UTF8.GetBytes(broadcastData), remoteEndPoint);
                        if (_printDebugOutput)
                        {
                            Console.WriteLine("UDP Broadcast: " + broadcastData + ", port: " + MAPLE_SERVER_BROADCASTPORT);
                        }
                        Thread.Sleep(AdvertiseIntervalMs);
                    }
                }
            });
        }

        /// <summary>
        /// Looks for IRequestHandlers and adds them to the `requestHandlers`
        /// collection for use later.
        /// </summary>
        protected void LoadRequestHandlers()
        {
            // look through all the assemblies in the app for IRequestHandlers
            // and add them to the `requestHandlers` collection
            if (_requestHandlers.Count == 0)
            {
                // Get classes that implement IRequestHandler
                var type = typeof(IRequestHandler);
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // loop through each assembly in the app and all the classes in it
                foreach (var assembly in assemblies)
                {
                    var types = assembly.GetTypes();
                    foreach (var t in types)
                    {
                        // if it inherits `IRequestHandler`, add it to the list
                        if (t.BaseType != null)
                        {
                            if (t.BaseType.GetInterfaces().Contains(typeof(IRequestHandler)))
                            {
                                _requestHandlers.Add(t);
                            }
                        }
                    }
                }

                if (_requestHandlers.Count == 0)
                {
                    Console.WriteLine("Warning: No Maple Server `IRequestHandler`s found. Server will not operate.");
                }
                else
                {
                    if (_printDebugOutput)
                    {
                        Console.WriteLine($"requestHandlers.Count: {_requestHandlers.Count}");
                    }
                }
            }
        }

        /// <summary>
        /// Starts a thread that listens to incoming Http requests and handles
        /// them. Note that the current implementation handles requests serially,
        /// rather than in parallel.
        /// </summary>
        /// <returns></returns>
        protected async Task StartListeningToIncomingRequests()
        {
            if (Running)
            {
                if (_printDebugOutput)
                {
                    Console.WriteLine("Already running.");
                }
                return;
            }

            Running = true;

            await Task.Run(async () =>
            {
                if (_printDebugOutput)
                {
                    Console.WriteLine("starting up listener.");
                }

                while (Running)
                {
                    try
                    {
                        // wait for a request to come in
                        HttpListenerContext context = await _httpListener.GetContextAsync();
                        if (_printDebugOutput)
                        {
                            Console.WriteLine("got one!");
                        }

                        // depending on our processing mode, process either
                        // synchronously, or spin off a thread and immediately
                        // process the next request (as it comes in)
                        switch (ThreadingMode)
                        {
                            case RequestProcessMode.Serial:
                                ProcessRequest(context).Wait();
                                break;
                            case RequestProcessMode.Parallel:
                                _ = ProcessRequest(context);
                                break;
                        }
                    }
                    catch (SocketException e)
                    {
                        if (_printDebugOutput)
                        {
                            Console.WriteLine("Socket Exception: " + e.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_printDebugOutput)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            });
        }

        protected Task ProcessRequest(HttpListenerContext context)
        {
            return Task.Run(async () =>
            {
                string[] urlQuery = context.Request.RawUrl.Substring(1).Split('?');
                string[] urlParams = urlQuery[0].Split('/');
                string requestedMethodName = urlParams[0].ToLower();

                if (_printDebugOutput)
                {
                    Console.WriteLine("Received " + context.Request.HttpMethod + " " + context.Request.RawUrl + " - Invoking " + requestedMethodName);
                }

                MethodInfo method = null;

                // has this method already been called and cached?
                if (_methodCache.Contains(context.Request.HttpMethod, requestedMethodName))
                {
                    method = _methodCache.GetMethod(context.Request.HttpMethod, requestedMethodName);
                }
                else
                {
                    // look in all the known request handlers
                    foreach (var handler in _requestHandlers)
                    {
                        // look in all the methods in the request handler for a matching name or *path*
                        var methods = handler.GetMethods();
                        foreach (var m in methods)
                        {
                            //first, let's see if the method has the correct http verb
                            Dictionary<string, string> supportedVerbs = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                            foreach (var attr in m.GetCustomAttributes())
                            {
                                switch (attr)
                                {
                                    case HttpGetAttribute a:
                                        supportedVerbs.Add("GET", a.Template ?? m.Name);
                                        break;
                                    case HttpPutAttribute a:
                                        supportedVerbs.Add("PUT", a.Template ?? m.Name);
                                        break;
                                    case HttpPatchAttribute a:
                                        supportedVerbs.Add("PATCH", a.Template ?? m.Name);
                                        break;
                                    case HttpPostAttribute a:
                                        supportedVerbs.Add("POST", a.Template ?? m.Name);
                                        break;
                                    case HttpDeleteAttribute a:
                                        supportedVerbs.Add("DELETE", a.Template ?? m.Name);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            // if the verb does't match the context method, then move to the next method to examine it
                            if (!supportedVerbs.ContainsKey(context.Request.HttpMethod))
                            {
                                continue;
                            }

                            // match the method or route template name:
                            if (supportedVerbs[context.Request.HttpMethod].Contains(requestedMethodName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                method = m;
                                _methodCache.Add(context.Request.HttpMethod, requestedMethodName, method);

                                break;
                            }
                        }
                    }
                }

                // if we couldn't find the method, return 404.
                if (method == null)
                {
                    // TODO: potentially load content from a file?
                    byte[] data = Encoding.UTF8.GetBytes("<head><body>404. Not found.</body><head>");
                    context.Response.ContentType = "text/html";
                    context.Response.ContentEncoding = Encoding.UTF8;
                    context.Response.ContentLength64 = data.LongLength;
                    context.Response.StatusCode = 404;
                    await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
                    context.Response.Close();
                    return;
                }

                IRequestHandler target;
                var shouldDispose = false;

                if (_handlerCache.ContainsKey(method))
                {
                    target = _handlerCache[method];
                }
                else
                {
                    // instantiate the handler, set the context (which contains all the request info)
                    target = Activator.CreateInstance(method.DeclaringType) as IRequestHandler;

                    if (target.IsReusable)
                    {
                        // cache for later use
                        _handlerCache.Add(method, target);
                    }
                    else
                    {
                        shouldDispose = true;
                    }
                }

                target.Context = context;
                try
                {
                    if(typeof(IActionResult).IsAssignableFrom(method.ReturnType))
                    {
                        var result = method.Invoke(target, null) as IActionResult;
                        await result.ExecuteResultAsync(context);
                    }
                    else
                    {
                        method.Invoke(target, null);
                    }
                }
                catch (Exception ex)
                {
                    if (_printDebugOutput) { Console.WriteLine(ex.Message); }
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }

                if (shouldDispose)
                {
                    target.Dispose();
                    target = null;
                }
                
            });
        }
    }
}
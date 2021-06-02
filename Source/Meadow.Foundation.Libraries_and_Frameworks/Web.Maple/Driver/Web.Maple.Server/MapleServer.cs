using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        // set to true for debug console writelines.
        private bool printDebugOutput = true;

        private const int MAPLE_SERVER_BROADCASTPORT = 17756;

        private readonly HttpListener httpListener;
        private readonly IList<Type> requestHandlers = new List<Type>();
        public IPAddress IPAddress { get; protected set; }

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
        // TODO: pull from Device.Name when the API is available.
        public string DeviceName { get; set; } = "Meadow";

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
            int port = 5417,
            bool advertise = false,
            RequestProcessMode processMode = RequestProcessMode.Serial)
        {
            IPAddress = ipAddress;
            Advertise = advertise;
            ThreadingMode = processMode;

            httpListener = new HttpListener();
            //httpListener.Prefixes.Add($"http://127.0.0.1:{port}/");
            //httpListener.Prefixes.Add($"http://localhost:{port}/");

            if (IPAddress != null) {
                httpListener.Prefixes.Add($"http://{IPAddress}:{port}/");
            }

            Init();

            if (printDebugOutput) { Console.WriteLine($"Will listen @ http://{IPAddress}:{port}/"); }
        }

        protected void Init()
        {
            LoadRequestHandlers();
        }

        /// <summary>
        /// Starts listening to requests, and optionally advertises on UDP.
        /// </summary>
        public async void Start()
        {
            httpListener.Start();
            if (Advertise) { StartUdpAdvertisement(); }
            await StartListeningToIncomingRequests();
            httpListener.Close();
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
                        if (printDebugOutput) { Console.WriteLine("UDP Broadcast: " + broadcastData + ", port: " + MAPLE_SERVER_BROADCASTPORT); }
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
            if (requestHandlers.Count == 0) {
                // Get classes that implement IRequestHandler
                var type = typeof(IRequestHandler);
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // loop through each assembly in the app and all the classes in it
                foreach (var assembly in assemblies) {
                    var types = assembly.GetTypes();
                    foreach (var t in types) {
                        // if it inherits `IRequestHandler`, add it to the list
                        if (t.BaseType != null) {
                            if (t.BaseType.GetInterfaces().Contains(typeof(IRequestHandler))) {
                                requestHandlers.Add(t);
                            }
                        }
                    }
                }
                if (requestHandlers.Count == 0) {
                    Console.WriteLine("Warning: No Maple Server `IRequestHandler`s found. Server will not operate.");
                } else {
                    if (printDebugOutput) { Console.WriteLine($"requestHandlers.Count: {requestHandlers.Count}"); }
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
            // if we're already running, bail out.
            if(Running) {
                if (printDebugOutput) { Console.WriteLine("Already running."); }
                return;
            }

            Running = true;

            await Task.Run(async () => {
            if (printDebugOutput) { Console.WriteLine("starting up listener."); }
                while (Running) {
                    try {
                        // wait for a request to come in
                        HttpListenerContext context = await httpListener.GetContextAsync();
                        if (printDebugOutput) { Console.WriteLine("got one!"); }

                        // depending on our processing mode, process either
                        // synchronously, or spin off a thread and immediately
                        // process the next request (as it comes in)
                        switch (ThreadingMode) {
                            case RequestProcessMode.Serial:
                                ProcessRequest(context).Wait();
                                break;
                            case RequestProcessMode.Parallel:
                                _ = ProcessRequest(context);
                                break;
                        }
                    } catch (SocketException e) {
                        if (printDebugOutput) { Console.WriteLine("Socket Exception: " + e.ToString()); }
                    } catch (Exception ex) {
                        if (printDebugOutput) { Console.WriteLine(ex.ToString()); }
                    }
                }
            });
        }

        protected Task ProcessRequest(HttpListenerContext context)
        {
            return Task.Run(async () => {

                string[] urlQuery = context.Request.RawUrl.Substring(1).Split('?');
                string[] urlParams = urlQuery[0].Split('/');
                string methodName = urlParams[0].ToLower();

                if (printDebugOutput) { Console.WriteLine("Received " + context.Request.HttpMethod + " " + context.Request.RawUrl + " - Invoking " + methodName); }

                // look in all the known request handlers
                bool wasMethodFound = false;
                foreach (var handler in requestHandlers) {

                    // look in all the methods in the request handler for a match
                    var methods = handler.GetMethods();
                    foreach (var method in methods) {

                        //first, let's see if the method has the correct http verb
                        List<string> supportedVerbs = new List<string>();
                        foreach (var attr in method.GetCustomAttributes()) {
                            switch (attr) {
                                case HttpGetAttribute a:
                                    supportedVerbs.Add("GET");
                                    break;
                                case HttpPutAttribute a:
                                    supportedVerbs.Add("PUT");
                                    break;
                                case HttpPatchAttribute a:
                                    supportedVerbs.Add("PATCH");
                                    break;
                                case HttpPostAttribute a:
                                    supportedVerbs.Add("POST");
                                    break;
                                case HttpDeleteAttribute a:
                                    supportedVerbs.Add("DELETE");
                                    break;
                                default:
                                    break;
                            }
                        }

                        // if the verb does't match the context method, then move to the next method to examine it
                        if (!supportedVerbs.Contains(context.Request.HttpMethod)) {
                            continue;
                        }

                        // match the method name:
                        if (method.Name.ToLower() == methodName) {

                            // instantiate the handler, set the context (which contains all the request info)
                            using (IRequestHandler target = Activator.CreateInstance(handler) as IRequestHandler) {
                                target.Context = context;
                            try {
                                method.Invoke(target, null);
                            } catch (Exception ex) {
                                if (printDebugOutput) { Console.WriteLine(ex.Message); }
                                context.Response.StatusCode = 500;
                                context.Response.Close();
                            }
                                // Cleanup
                                //target.Dispose();
                                //target = null;
                            }

                            wasMethodFound = true;
                            break;
                        }
                    }
                    if (wasMethodFound) break;
                }

                // if we couldn't find the method, return 404.
                if (!wasMethodFound) {
                    byte[] data = Encoding.UTF8.GetBytes("<head><body>404. can not find.</body><head>");
                    context.Response.ContentType = "text/html";
                    context.Response.ContentEncoding = Encoding.UTF8;
                    context.Response.ContentLength64 = data.LongLength;
                    context.Response.StatusCode = 404;
                    await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
                    context.Response.Close();
                }
            });
        }
    }
}
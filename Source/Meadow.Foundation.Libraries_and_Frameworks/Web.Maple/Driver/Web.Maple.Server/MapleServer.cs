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
    public partial class MapleServer
    {
        private bool DebugView = false;

        private const int MAPLE_SERVER_BROADCASTPORT = 17756;

        private HttpListener httpListener;
        private IList<Type> requestHandlers = new List<Type>();
        private IPAddress ipAddress;

        public bool Running { get; set; } = false;

        public bool Advertise { get; set; } = false;
        public int AdvertiseIntervalMs { get; set; } = 2000;


        // TODO: pull from Device.Name when the API is available.
        public string DeviceName { get; set; } = "Meadow";

        public MapleServer(IPAddress ipAddress, int port = 5417)
        {
            this.ipAddress = ipAddress;

            httpListener = new HttpListener();
            //httpListener.Prefixes.Add($"http://127.0.0.1:{port}/");
            //httpListener.Prefixes.Add($"http://localhost:{port}/");

            if (this.ipAddress != null) {
                httpListener.Prefixes.Add($"http://{this.ipAddress}:{port}/");
            }

            this.Init();

            if (DebugView) { Console.WriteLine($"Will listen @ http://{this.ipAddress}:{port}/"); }
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
            await HandleIncomingRequests();
            httpListener.Close();
        }

        public void Stop()
        {
            Running = false;
        }

        //public void AddHandler(IRequestHandler handler)
        //{
        //    handlers.Add(handler);
        //}

        //public void RemoveHandler(IRequestHandler handler)
        //{
        //    handlers.Remove(handler);
        //}


        /// <param name="data">string to be broadcast over UDP</param>
        /// <param name="interval">millis</param>
        protected void StartUdpAdvertisement()
        {
            Task.Run(async () => {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), MAPLE_SERVER_BROADCASTPORT);
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

                    string broadcastData = $"{DeviceName}::{ipAddress}";
                    while (Running) {
                        socket.SendTo(UTF8Encoding.UTF8.GetBytes(broadcastData), remoteEndPoint);
                        if (DebugView) { Console.WriteLine("UDP Broadcast: " + broadcastData + ", port: " + MAPLE_SERVER_BROADCASTPORT); }
                        Thread.Sleep(AdvertiseIntervalMs);
                    }
                }
            });
        }

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
                    throw new Exception("No Maple Server `IRequestHandler`s found. Server can not operate.");
                } else {
                    if (DebugView) { Console.WriteLine($"requestHandlers.Count: {requestHandlers.Count}"); }
                }
            }
        }

        protected async Task HandleIncomingRequests()
        {
            // if we're already running, bail out.
            if(Running) {
                if (DebugView) { Console.WriteLine("Already running."); }
                return;
            }

            this.Running = true;

            await Task.Run(async () => {
            if (DebugView) { Console.WriteLine("starting up listener."); }
                while (Running) {
                    try {

                        // Wait for a request to come in
                        // TODO: today, the logic here is blocking; the request
                        // is received, and then we proceed to find the appropriate
                        // handler, instantiate it, execute it, and then continue
                        // the loop.
                        //
                        // this means each request has to wait for handling completion
                        // before the next one (if there is one), will be dealt with.
                        //
                        // to change this, we should probably add a property called `ThreadingMode`
                        // that is of `enum ThreadModeType { single, multi }`
                        //
                        // and, when a new context comes in,
                        // we should dispatch to a non-blocking/async method that handles.
                        // something like the following:
                        // protected void ProcessRequest(HttpListenerContext context)
                        // {
                        //     Task.Run(() => {
                        //         //do all the work here
                        //     });
                        // }
                        HttpListenerContext context = await httpListener.GetContextAsync();
                        if (DebugView) { Console.WriteLine("got one!"); }

                        string[] urlQuery = context.Request.RawUrl.Substring(1).Split('?');
                        string[] urlParams = urlQuery[0].Split('/');
                        string methodName = urlParams[0].ToLower();

                        if (DebugView) { Console.WriteLine("Received " + context.Request.HttpMethod + " " + context.Request.RawUrl + " - Invoking " + methodName); }

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
                                    IRequestHandler target = Activator.CreateInstance(handler) as IRequestHandler;
                                    target.Context = context;
                                    try {
                                        method.Invoke(target, null);
                                    } catch (Exception ex) {
                                        if (DebugView) { Console.WriteLine(ex.Message); }
                                        context.Response.StatusCode = 500;
                                        context.Response.Close();
                                    }
                                    // Cleanup
                                    target.Dispose();
                                    target = null;

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
                    } catch (SocketException e) {
                        if (DebugView) { Console.WriteLine("Socket Exception: " + e.ToString()); }
                    } catch (Exception ex) {
                        if (DebugView) { Console.WriteLine(ex.ToString()); }
                    }
                }
            });
        }
    }
}

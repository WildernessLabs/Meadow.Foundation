using System;
using System.Threading;
using Meadow;

namespace Hello_RGB
{
    class Program
    {
        static IApp _app;

        static void Main(string[] args)
        {
            // instantiate and run new meadow app
            _app = new RGBApp();
            _app.Run();

            // run forever
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

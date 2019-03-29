using Meadow;
using System;
using System.Threading;

namespace ParallaxPir_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new ParallaxPirApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
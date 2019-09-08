using System;
using System.Threading;
using Meadow;

namespace TftSpiDisplay_Sample
{
    class MainClass
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new TftSpiDisplayApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

using System;
using System.Threading;
using Meadow;

namespace PushButton_Sample
{
    class MainClass
    {
        static IApp app;

        public static void Main(string[] args)
        {
            app = new PushButtonApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

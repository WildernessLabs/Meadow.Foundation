using Meadow;
using System;
using System.Threading;

namespace Lcd2004_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello LCD");

            app = new Lcd2004App();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

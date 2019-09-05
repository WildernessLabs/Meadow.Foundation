using Meadow;
using System.Threading;

namespace Capacitive_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new CapacitiveApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
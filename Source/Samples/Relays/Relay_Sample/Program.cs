using Meadow;
using System.Threading;

namespace Relay_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new RelayApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

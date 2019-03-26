using Meadow;
using System.Threading;

namespace LedBarGraph_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new LedBarGraphApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

using Meadow;
using System.Threading;

namespace Led_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new LedApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
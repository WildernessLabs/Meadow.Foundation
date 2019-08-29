using Meadow;
using System.Threading;

namespace GY521_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new GY521App();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

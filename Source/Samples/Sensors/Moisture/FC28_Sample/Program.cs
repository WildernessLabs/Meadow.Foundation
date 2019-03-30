using Meadow;
using System.Threading;

namespace FC28_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new FC28App();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

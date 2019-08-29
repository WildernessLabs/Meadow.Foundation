using Meadow;
using System.Threading;

namespace GY63_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new GY63App();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

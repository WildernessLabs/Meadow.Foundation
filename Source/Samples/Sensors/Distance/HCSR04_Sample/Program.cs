using Meadow;
using System.Threading;

namespace HCSR04_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new HCSR04App();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

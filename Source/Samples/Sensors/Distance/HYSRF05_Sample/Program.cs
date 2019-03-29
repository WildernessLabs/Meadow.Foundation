using Meadow;
using System.Threading;

namespace HYSRF05_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new HYSRF05App();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

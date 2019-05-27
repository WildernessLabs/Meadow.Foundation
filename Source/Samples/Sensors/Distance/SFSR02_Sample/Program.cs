using Meadow;
using System.Threading;

namespace SFSR02_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new SFSR02App();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

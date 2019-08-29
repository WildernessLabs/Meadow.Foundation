using Meadow;
using System.Threading;

namespace DS1307_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new DS1307App();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

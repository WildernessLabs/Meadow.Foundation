using Meadow;
using System.Threading;

namespace Ms5611_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new MeadowApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

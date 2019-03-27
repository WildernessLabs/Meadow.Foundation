using Meadow;
using System.Threading;

namespace DipSwitch_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new DipSwitchApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
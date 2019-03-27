using Meadow;
using System.Threading;

namespace SpstSwitch_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new SpstSwitchApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
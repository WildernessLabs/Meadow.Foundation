using Meadow;
using System.Threading;

namespace SpdtSwitch_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new SpdtSwitchApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

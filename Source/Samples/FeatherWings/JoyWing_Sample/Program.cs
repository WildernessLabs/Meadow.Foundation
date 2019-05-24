using Meadow;
using System.Threading;

namespace JoyWing_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new JoyWingApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
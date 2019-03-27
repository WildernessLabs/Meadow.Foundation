using Meadow;
using System.Threading;

namespace RotaryEncoder_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new RotaryEncoderApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

using Meadow;
using System.Threading;

namespace RotaryEncoderWithButton_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new RotaryEncoderWithButtonApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

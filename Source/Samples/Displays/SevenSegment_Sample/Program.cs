using Meadow;
using System.Threading;

namespace SevenSegment_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new SevenSegmentApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

using Meadow;
using System.Threading;

namespace PiezoSpeaker_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new PiezoSpeakerApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

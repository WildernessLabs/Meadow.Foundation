using System.Threading;
using Meadow;

namespace ICs.IOExpanders.HT16K33_Sample
{
    class Program
    {
        static IApp app;

        public static void Main(string[] args)
        {
            app = new MeadowApp();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
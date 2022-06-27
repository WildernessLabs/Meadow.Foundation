using Meadow;
using System.Threading;

namespace ICs.IOExpanders.Sw18AB_Samples
{
    class Program
    {
        static IApp app;

        public static void Main(string[] args)
        {
            MeadowOS.Main(args);
            //            app = new MeadowApp();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
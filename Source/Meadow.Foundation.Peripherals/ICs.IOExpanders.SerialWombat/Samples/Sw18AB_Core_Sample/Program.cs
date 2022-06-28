using Meadow;
using System.Threading;

namespace ICs.IOExpanders.Sw18AB_Samples
{
    class Program
    {
        public static void Main(string[] args)
        {
            MeadowOS.Main(args);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
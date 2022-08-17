using Meadow;
using System.Threading;

namespace ICs.IOExpanders.Mcp23x08_Input_Sample
{
    class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            // instantiate and run new meadow app
            app = new SuperMeadowApp();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
using System.Threading;

namespace Meadow.Foundation.Sensors.Radio.Rfid.IDxxLA_Sample
{
    internal class Program
    {
        private static IApp app;

        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--exitOnDebug")
            {
                return;
            }

            // instantiate and run new meadow app
            app = new MeadowApp();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}

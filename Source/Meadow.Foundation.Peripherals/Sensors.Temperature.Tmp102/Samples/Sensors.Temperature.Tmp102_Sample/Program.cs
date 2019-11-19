using System.Threading;
using Meadow;

namespace Sensors.Temperature.TMP102_Sample
{
    class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            // instantiate and run new meadow app
            app = new App();

            Thread.Sleep(-1);
        }
    }
}

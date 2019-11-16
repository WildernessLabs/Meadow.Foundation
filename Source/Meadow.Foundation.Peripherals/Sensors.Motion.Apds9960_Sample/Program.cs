using Meadow;
using System.Threading;

namespace BasicSensors.Motion.Apds9960_Sample
{
    class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            // instantiate and run new meadow app
            app = new App();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
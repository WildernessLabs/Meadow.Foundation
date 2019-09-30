using Meadow;
using System.Threading;

namespace AnalogTemperature_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new AnalogTemperatureApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

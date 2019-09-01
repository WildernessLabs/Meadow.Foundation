using Meadow;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BME280_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new BME280App();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

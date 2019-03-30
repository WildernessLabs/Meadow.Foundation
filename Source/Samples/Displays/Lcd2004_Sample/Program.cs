using Meadow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lcd2004_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new Lcd2004App();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

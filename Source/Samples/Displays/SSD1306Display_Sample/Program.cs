using System;
using System.Threading;
using Meadow;

namespace SSD1306Display_Sample
{
    class MainClass
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new SSD1306DisplayApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

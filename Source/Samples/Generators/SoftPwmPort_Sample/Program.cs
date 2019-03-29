using System;
using Meadow;

namespace SoftPwmPort_Sample
{
    class MainClass
    {
        static IApp app;

        public static void Main(string[] args)
        {
            app = new SoftPwmPortApp();
        }
    }
}

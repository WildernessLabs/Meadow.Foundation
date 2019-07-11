using System.Threading;
using Meadow;

namespace Servo_Sample
{
    class MainClass
    {
        static IApp app;

        public static void Main(string[] args)
        {
            app = new ServoApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
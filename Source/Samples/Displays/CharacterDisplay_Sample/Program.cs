using Meadow;
using System;
using System.Threading;

namespace CharacterDisplay_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello LCD");

            app = new CharacterDisplayApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

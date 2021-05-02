using System;
using System.Diagnostics;

namespace Meadow.Foundation.Sensors.LoadCell
{
    internal static class Output
    {
        [Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        [Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, string message)
        {
            if (condition) Console.WriteLine(message);
        }
    }
}
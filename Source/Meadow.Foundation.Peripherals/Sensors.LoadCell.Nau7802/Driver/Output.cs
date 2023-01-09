using System;
using System.Diagnostics;

namespace Meadow.Foundation.Sensors.LoadCell
{
    internal static class Output
    {
        [Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
            Resolver.Log.Info(message);
        }

        [Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, string message)
        {
            if (condition) Resolver.Log.Info(message);
        }
    }
}
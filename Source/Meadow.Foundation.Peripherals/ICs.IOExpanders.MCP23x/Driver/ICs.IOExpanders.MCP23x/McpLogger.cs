using System;
using System.IO;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public static class McpLogger
    {
        public static TextWriter DebugOut { internal get; set; } = TextWriter.Null;
        public static TextWriter ErrorOut { internal get; set; } = TextWriter.Null;

        public static void DisableLogging()
        {
            DebugOut.WriteLine("Disabling logging for Mcp23x devices");
            DebugOut = TextWriter.Null;
            ErrorOut = TextWriter.Null;
        }

        public static void LogToConsole()
        {
            DebugOut = Console.Out;
            ErrorOut = Console.Error;
            DebugOut.WriteLine("Enabled logging for Mcp23x devices");
        }
    }
}

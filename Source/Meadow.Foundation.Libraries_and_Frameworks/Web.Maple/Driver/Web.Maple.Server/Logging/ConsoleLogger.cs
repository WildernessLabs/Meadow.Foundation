using System;

namespace Meadow.Foundation.Web.Maple.Server
{
    public class ConsoleLogger : ILogger
    {
        public Loglevel Loglevel { get; set; } = Loglevel.Error;

        public void Debug(string message)
        {
            Log(Loglevel.Debug, message);
        }

        public void DebugIf(bool condition, string message)
        {
            if (condition) Log(Loglevel.Debug, message);
        }

        public void Info(string message)
        {
            Log(Loglevel.Info, message);
        }

        public void InfoIf(bool condition, string message)
        {
            if (condition) Log(Loglevel.Info, message);
        }

        public void Warn(string message)
        {
            Log(Loglevel.Warning, message);
        }

        public void WarnIf(bool condition, string message)
        {
            if (condition) Log(Loglevel.Warning, message);
        }

        public void Error(string message)
        {
            Log(Loglevel.Error, message);
        }

        public void ErrorIf(bool condition, string message)
        {
            if (condition) Log(Loglevel.Error, message);
        }

        public void Log(Loglevel level, string message)
        {
            if (Loglevel < level) return;
            Console.WriteLine($"{level.ToString().ToUpper()}: {message}");
        }
    }
}
using System;
using System.IO;
using System.Linq;

namespace Meadow.Logging
{
    public class FileLogger : ILogProvider
    {
        private string LogFilePath { get; }

        public FileLogger()
        {
            LogFilePath = Path.Combine(Resolver.Device.PlatformOS.FileSystem.DocumentsDirectory, "meadow.log");
            if (!File.Exists(LogFilePath))
            {
                File.Create(LogFilePath).Close();
            }
        }

        public void Log(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Warning:
                case LogLevel.Error:
                    LogToFile(message);
                    break;
            }
        }

        private void LogToFile(string message)
        {
            if (message.EndsWith(Environment.NewLine))
            {
                File.AppendAllText(LogFilePath, message);
            }
            else
            {
                File.AppendAllText(LogFilePath, message + Environment.NewLine);
            }
        }

        public string[] GetLogContents()
        {
            if (!File.Exists(LogFilePath))
            {
                return new string[0];
            }
            return File.ReadLines(LogFilePath).ToArray();
        }

        public void TruncateLog()
        {
            File.Create(LogFilePath).Close();
        }
    }
}
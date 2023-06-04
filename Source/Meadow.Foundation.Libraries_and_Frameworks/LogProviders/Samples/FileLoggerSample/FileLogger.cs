using Meadow;
using Meadow.Logging;
using System;
using System.IO;
using System.Linq;

namespace Logging
{
    public class FileLogger : ILogProvider
    {
        private string LogFilePath { get; }
        public LogLevel MinimumLevelToLog { get; set; }

        public FileLogger(LogLevel minimumLevelToLog = LogLevel.Warning)
        {
            LogFilePath = Path.Combine(MeadowOS.FileSystem.DocumentsDirectory, "meadow.log");
            if (!File.Exists(LogFilePath))
            {
                File.Create(LogFilePath).Close();
            }
        }

        public void Log(LogLevel level, string message)
        {
            if (level >= MinimumLevelToLog && level != LogLevel.None)
            {
                LogToFile(message);
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
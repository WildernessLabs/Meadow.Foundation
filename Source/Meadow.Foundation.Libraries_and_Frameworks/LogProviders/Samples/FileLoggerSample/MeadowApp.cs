using Meadow;
using Meadow.Devices;
using Meadow.Logging;
using System;
using System.Threading.Tasks;

namespace Logging
{
    public class MeadowApp : App<F7FeatherV2>
    {
        public override Task Initialize()
        {
            Resolver.Log.Info($"Initializing...");

            // an our own logger to the system logger
            AddFileLogger();

            return base.Initialize();
        }

        public override async Task Run()
        {
            Resolver.Log.Info($"This will not be in the log file (it's just info)");

            Resolver.Log.Warn($"This will be in the log file (it's a warning)");
        }

        private void AddFileLogger()
        {
            // only log warnings and errors to a file
            var fileLogger = new FileLogger(LogLevel.Warning);

            // output the log contents just for display.  Do it before adding the logger so we don't recurse
            var lineNumber = 1;
            var contents = fileLogger.GetLogContents();
            if (contents.Length > 0)
            {
                Resolver.Log.Info($"Log contents{Environment.NewLine}------------");

                foreach (var line in contents)
                {
                    Resolver.Log.Info($"{lineNumber++:000}> {line}");
                }
                Resolver.Log.Info($"------------");
            }
            else
            {
                Resolver.Log.Info($"Log is empty");
            }
            Resolver.Log.AddProvider(fileLogger);
        }
    }
}
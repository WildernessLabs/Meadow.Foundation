namespace Meadow.Foundation.Web.Maple.Server
{
    public interface ILogger
    {
        Loglevel Loglevel { get; set; }
        void Log(Loglevel level, string message);
        void Debug(string message);
        void DebugIf(bool condition, string message);
        void Info(string message);
        void InfoIf(bool condition, string message);
        void Warn(string message);
        void WarnIf(bool condition, string message);
        void Error(string message);
        void ErrorIf(bool condition, string message);
    }
}
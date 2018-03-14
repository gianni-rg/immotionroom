#if !EXCLUDE_LOG4NET

namespace ImmotionAR.ImmotionRoom.Logger.Log4Net
{
    public class LoggerConfiguration : ILoggerConfiguration
    {
        public ILogWatcher LogWatcher { get; set; }

        public bool RollingFileEnabled { get; set; }
        public string RollingStyle { get; set; }
        public string RollingFileLogFormat { get; set; }
        public string LogLevel { get; set; }
        public bool AppendToFile { get; set; }
        public string LogFile { get; set; }
        public int MaxSizeRollBackups { get; set; }
        public string MaximumFileSize { get; set; }
        public string DatePattern { get; set; }
        public bool StaticLogFileName { get; set; }

        public bool MemoryLoggerEnabled { get; set; }
        public string MemoryLogFormat { get; set; }

        public bool TraceLoggerEnabled { get; set; }
        public string TraceLogFormat { get; set; }

        public bool ConsoleLoggerEnabled { get; set; }
        public string ConsoleLogFormat { get; set; }
    }
}

#endif
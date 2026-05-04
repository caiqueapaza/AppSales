namespace APISales.Log
{
    public class CustomerLogger : ILogger
    {
        private static readonly object LogFileLock = new();
        private readonly string loggerName;
        private readonly CustomLoggerProviderConfiguration loggerConfig;

        public CustomerLogger(string name, CustomLoggerProviderConfiguration config)
        {
            loggerName = name;
            loggerConfig = config;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel == loggerConfig.LogLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                                Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {logLevel}: {eventId} - {formatter(state, exception)}";
            WriteTextFile(message);
        }

        private void WriteTextFile(string message)
        {
            const string fileLog = @"c:\Temp\log\Sales_log.txt";

            lock (LogFileLock)
            {
                using (var fileStream = new FileStream(fileLog, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.WriteLine(message);
                }
            }
        }
    }
}

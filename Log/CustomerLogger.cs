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
            var baseDir = Environment.GetEnvironmentVariable("APP_LOG_DIR");
            if (string.IsNullOrWhiteSpace(baseDir))
            {
                baseDir = Path.Combine(AppContext.BaseDirectory, "logs");
            }
            Directory.CreateDirectory(baseDir);
            var fileLog = Path.Combine(baseDir, "Sales_log.txt");

            lock (LogFileLock)
            {
                try
                {
                    using (var fileStream = new FileStream(fileLog, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine(message);
                    }
                }
                catch
                {
                    // Nunca bloquear a aplicação por falha de log em arquivo.
                }
            }
        }
    }
}

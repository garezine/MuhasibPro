using Microsoft.Extensions.Logging;

namespace Muhasib.Business.Infrastructure.Provider
{
    public class SimpleFileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;

        public SimpleFileLoggerProvider(string filePath)
        {
            _filePath = filePath;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new SimpleFileLogger(_filePath, categoryName);
        }

        public void Dispose() { }
    }

    public class SimpleFileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly string _categoryName;

        public SimpleFileLogger(string filePath, string categoryName)
        {
            _filePath = filePath;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {message}";

            File.AppendAllText(_filePath, logMessage + Environment.NewLine);
        }
    }
}

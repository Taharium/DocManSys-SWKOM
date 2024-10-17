using System.Collections.Concurrent;

namespace PaperlessRest.Log4Net;

public class Log4NetProvider : ILoggerProvider
{
    private readonly string _log4NetConfigFile;
    private readonly ConcurrentDictionary<string, ILogger> _loggers = new();

    public Log4NetProvider(string log4NetConfigFile = "log4net.config")
    {
        _log4NetConfigFile = log4NetConfigFile;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
    }

    public void Dispose()
    {
        _loggers.Clear();
    }

    private ILogger CreateLoggerImplementation(string name)
    {
        return new Log4NetLogger(name, new FileInfo(_log4NetConfigFile));
    }
}
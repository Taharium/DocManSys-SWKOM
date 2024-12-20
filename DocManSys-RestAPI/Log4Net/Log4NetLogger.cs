﻿using System.Reflection;
using log4net;
using log4net.Config;

namespace DocManSys_RestAPI.Log4Net;

public class Log4NetLogger : ILogger {
    private readonly ILog _log;

    public Log4NetLogger(string name, FileInfo fileInfo) {
        var loggerRepository = LogManager.GetRepository(Assembly.GetEntryAssembly()!);
        _log = LogManager.GetLogger(loggerRepository.Name, name);

        XmlConfigurator.Configure(loggerRepository, fileInfo);
        if (!loggerRepository.Configured)
            throw new InvalidOperationException($"LoggerRepository could not be configured!");
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel) {
        switch (logLevel) {
            case LogLevel.Critical:
                return _log.IsFatalEnabled;
            case LogLevel.Debug:
            case LogLevel.Trace:
                return _log.IsDebugEnabled;
            case LogLevel.Error:
                return _log.IsErrorEnabled;
            case LogLevel.Information:
                return _log.IsInfoEnabled;
            case LogLevel.Warning:
                return _log.IsWarnEnabled;
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel));
        }
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) {
        if (!IsEnabled(logLevel)) {
            return;
        }

        if (formatter == null) {
            throw new ArgumentNullException(nameof(formatter));
        }

        string message = $"{formatter(state, exception)} {exception}";

        if (!string.IsNullOrEmpty(message) || exception != null) {
            switch (logLevel) {
                case LogLevel.Critical:
                    _log.Fatal(message);
                    break;
                case LogLevel.Debug:
                case LogLevel.Trace:
                    _log.Debug(message);
                    break;
                case LogLevel.Error:
                    _log.Error(message);
                    break;
                case LogLevel.Information:
                    _log.Info(message);
                    break;
                case LogLevel.Warning:
                    _log.Warn(message);
                    break;
                default:
                    _log.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                    _log.Info(message, exception);
                    break;
            }
        }
    }
}
using System.Text;

namespace ChronoTrace.SourceGenerators;

internal sealed class Logger
{
    private enum Level
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
    }

    private const string ScratchPadFile = "scratchpad.log";
    private readonly string _logPath;
    private readonly StringBuilder _messageBuilder;
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly Level _level;
    private bool _flushed;

    internal Logger(string path, string level)
    {
        _level = Enum.TryParse<Level>(level, ignoreCase: true, out var logLevel) ? logLevel : Level.Info;
        _logPath = Path.Combine(path, ScratchPadFile);
        _messageBuilder = new StringBuilder();
        _semaphoreSlim = new SemaphoreSlim(1, 1);
        _flushed = false;
    }

    internal void Trace(string message) => Log(Level.Trace, message);
    internal void Debug(string message) => Log(Level.Debug, message);
    internal void Info(string message) => Log(Level.Info, message);
    internal void Warning(string message) => Log(Level.Warning, message);
    internal void Error(string message) => Log(Level.Error, message);

    internal void Flush()
    {
#if DEBUG
        if (_logPath == ScratchPadFile)
        {
            return;            
        }

        _semaphoreSlim.Wait();
        if (!_flushed)
        {
            File.WriteAllText(_logPath, string.Empty);
            _flushed = true;
        }
        File.AppendAllText(_logPath, _messageBuilder.ToString());
        _semaphoreSlim.Release();
#endif
    }

    private void Log(Level level, string message)
    {
#if DEBUG
        if (level >= _level)
        {
            _semaphoreSlim.Wait();
            _messageBuilder.AppendLine($"[{DateTime.Now:yyyy/MM/dd hh:mm:ss.fff}] [{level}]: {message}");
            _semaphoreSlim.Release();
        }
#endif
    }
}
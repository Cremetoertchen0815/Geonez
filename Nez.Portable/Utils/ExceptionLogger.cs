using System;
using System.IO;

namespace Nez;

public class ExceptionLogger
{
    //Singleton
    private readonly string _logPath;

    private ExceptionLogger()
    {
        Directory.CreateDirectory("logs");
        _logPath = Path.Combine("logs", $"error-log-{DateTime.Now:yyyy-MM-dd_hh-mm-ss-tt}.log");
    }

    public static ExceptionLogger Instance { get; } = new();

    public event Action<Exception> ExceptionThrown;

    public void ReportMessage(string message)
    {
        File.AppendAllText(_logPath, $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.FFF tt} - LOG: {message}\n");
    }

    public void ReportHandledExcpetion(Exception exception)
    {
        LogException(exception, "ERROR(");
    }

    internal void ReportCrash(CrashPoint point, Exception exception)
    {
        LogException(exception, $"CRASH({point}");
    }

    private void LogException(Exception exception, string type)
    {
        if (ExceptionThrown is not null)
            try
            {
                ExceptionThrown(exception);
            }
            catch (Exception)
            {
                ExceptionThrown = null;
                LogException(exception, type);
            }

        var txt =
            $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.FFF tt} - {type}; CurrentScene: {Core.Scene?.GetType().FullName})\n";
        var ex = exception;
        var cnt = 0;
        while (ex != null)
        {
            txt += (cnt < 1 ? "base" : $"inner #{cnt}") + $": {ex.Message} \n{ex.StackTrace}\n";
            ex = ex.InnerException;
            cnt++;
        }

        txt += "----------------\n";
        File.AppendAllText(_logPath, txt);
    }

    internal enum CrashPoint
    {
        UPDATE,
        DRAW,
        INIT,
        UNSPECIFIED
    }
}
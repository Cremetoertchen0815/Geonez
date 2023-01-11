using System;
using System.IO;

namespace Nez;
public class CrashHandler
{
	//Singleton
	private static CrashHandler _instance = new CrashHandler();
	public static CrashHandler Instance => _instance;
	private CrashHandler()
	{
		Directory.CreateDirectory("logs");
		_logPath = Path.Combine("logs", $"error-log-{DateTime.Now:yyyy-MM-dd_hh-mm-ss-tt}.log");
	}
	private string _logPath;

	public void ReportMessage(string message) => File.AppendAllText(_logPath, $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.FFF tt} - LOG: {message}\n");
	public void ReportHandledExcpetion(Exception exception) => LogException(exception, $"ERROR(");
	internal void ReportCrash(CrashPoint point, Exception exception) => LogException(exception, $"CRASH({point}");

	private void LogException(Exception exception, string title)
	{
		var txt = $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.FFF tt} - {title}, base): {exception.Message} - {exception.StackTrace}\n";
		var ex = exception.InnerException;
		int cnt = 1;
		while (ex != null)
		{
			txt += $"{DateTime.Now:yyyy-MM-dd hh:mm:ss.FFF tt} - {title}, inner #{cnt}): {ex.Message} - {ex.StackTrace}\n";
			ex = ex.InnerException;
			cnt++;
		}
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

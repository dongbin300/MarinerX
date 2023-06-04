using System;
using System.IO;

namespace MarinerX.Bot
{
    public class Logger
    {
        public static void Log(string className, string? methodName, Exception exception)
        {
            Log(className, methodName, exception.ToString());
        }

        public static void Log(string className, string? methodName, string message)
        {
            File.AppendAllText($"Logs/{DateTime.Today:yyyyMMdd}.log", $"{DateTime.Now:HH:mm:ss.fff} [{className}.{methodName}] {message}" + Environment.NewLine);
        }
    }
}

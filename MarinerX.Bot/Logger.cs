using MarinerX.Bot.Models;

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

        public static void LogHistory(BotHistory botHistory)
        {
            File.AppendAllText($"Logs/{DateTime.Today:yyyyMMdd}_history.log", $"[{botHistory.DateTime:HH:mm:ss.fff}] {botHistory.Text}" + Environment.NewLine);
        }
    }
}

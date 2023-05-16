using System;
using System.IO;

namespace Albedo.Utils
{
    public class Logger
    {
        public static void Log(string className, string? methodName, string message)
        {
            File.AppendAllText($"Logs/{DateTime.Today:yyyyMMdd}.log", $"[{className}.{methodName}] {message}" + Environment.NewLine);
        }
    }
}

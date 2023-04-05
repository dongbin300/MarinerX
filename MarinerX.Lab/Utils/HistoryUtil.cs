using System;
using System.IO;

namespace MarinerX.Lab.Utils
{
    internal class HistoryUtil
    {
        readonly static string Path1m = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "BinanceFuturesData", "1m");
        readonly static string Path1d = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gaten", "BinanceFuturesData", "1D");
    }
}

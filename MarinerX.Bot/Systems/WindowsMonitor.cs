﻿using System;
using System.Diagnostics;
using System.Management;
using System.Reflection;
using System.Threading.Tasks;

namespace MarinerX.Bot.Systems
{
    public class WindowsMonitor
    {
        public static async Task<double> MemoryUsage()
        {
            try
            {
                var processName = Process.GetCurrentProcess().ProcessName;
                var searcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT * FROM Win32_PerfFormattedData_PerfProc_Process WHERE Name = \'{processName}\'");
                foreach (var obj in searcher.Get())
                {
                    var memory = long.Parse(obj["WorkingSetPrivate"].ToString());
                    return Math.Round((double)memory / 1000000, 1);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(WindowsMonitor), MethodBase.GetCurrentMethod()?.Name, ex);
            }

            return Common.NullDoubleValue;
        }
    }
}
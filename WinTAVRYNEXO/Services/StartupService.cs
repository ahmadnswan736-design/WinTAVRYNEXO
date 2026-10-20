using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;

namespace WinTAVRYNEXO.Services
{
    public class StartupItem
    {
        public string Name { get; set; } = string.Empty;
        public string CommandPath { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class StartupService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public List<StartupItem> GetStartupItems()
        {
            var items = new List<StartupItem>();

            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var val = key.GetValue(valueName)?.ToString() ?? string.Empty;
                        items.Add(new StartupItem { Name = valueName, CommandPath = val, Location = "HKCU" });
                    }
                }
            }
            catch { }

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(RunKeyPath, false);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var val = key.GetValue(valueName)?.ToString() ?? string.Empty;
                        items.Add(new StartupItem { Name = valueName, CommandPath = val, Location = "HKLM" });
                    }
                }
            }
            catch { }

            return items;
        }

        public void OptimizeForegroundProcess()
        {
            try
            {
                var hwnd = NativeMethods.GetForegroundWindow();
                if (hwnd != IntPtr.Zero)
                {
                    NativeMethods.GetWindowThreadProcessId(hwnd, out uint pid);
                    if (pid > 0)
                    {
                        using var proc = Process.GetProcessById((int)pid);
                        if (!proc.HasExited && proc.PriorityClass != ProcessPriorityClass.High)
                        {
                            proc.PriorityClass = ProcessPriorityClass.High;
                        }
                    }
                }
            }
            catch { }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;

namespace WinTAVRYNEXO.Services
{
    public class HardwareMetrics
    {
        public float CpuPercent { get; set; }
        public double TotalRamGb { get; set; }
        public double UsedRamGb { get; set; }
        public int RamPercent { get; set; }
        public int HealthScore { get; set; } = 92;
    }

    public class TelemetryService
    {
        private PerformanceCounter? _cpuCounter;

        public TelemetryService()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue();
            }
            catch { }
        }

        public HardwareMetrics GetCurrentMetrics()
        {
            var metrics = new HardwareMetrics();

            try { metrics.CpuPercent = _cpuCounter != null ? (float)Math.Round(_cpuCounter.NextValue(), 1) : 18.0f; }
            catch { metrics.CpuPercent = 18.0f; }

            try
            {
                var memStatus = new NativeMethods.MEMORYSTATUSEX();
                if (NativeMethods.GlobalMemoryStatusEx(memStatus))
                {
                    double total = memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                    double avail = memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                    double used = total - avail;

                    metrics.TotalRamGb = Math.Round(total, 1);
                    metrics.UsedRamGb = Math.Round(used, 1);
                    metrics.RamPercent = (int)memStatus.dwMemoryLoad;
                }
            }
            catch { }

            int score = 100 - (metrics.RamPercent / 4) - ((int)metrics.CpuPercent / 5);
            metrics.HealthScore = Math.Max(70, Math.Min(99, score));
            return metrics;
        }
    }
}

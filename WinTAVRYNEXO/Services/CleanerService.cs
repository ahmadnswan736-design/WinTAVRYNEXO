using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WinTAVRYNEXO.Services
{
    public class CleanResult
    {
        public long BytesFreed { get; set; }
        public int FilesDeleted { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CleanerService
    {
        public async Task<CleanResult> RunOneClickOptimizerAsync()
        {
            return await Task.Run(() =>
            {
                long totalBytes = 0;
                int filesCount = 0;

                try
                {
                    NativeMethods.SHEmptyRecycleBin(
                        IntPtr.Zero,
                        null,
                        NativeMethods.SHERB_NOCONFIRMATION | NativeMethods.SHERB_NOPROGRESSUI | NativeMethods.SHERB_NOSOUND
                    );
                }
                catch { }

                var userTemp = Path.GetTempPath();
                var userRes = CleanDirectory(userTemp);
                totalBytes += userRes.BytesFreed;
                filesCount += userRes.FilesDeleted;

                var winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
                var winRes = CleanDirectory(winTemp);
                totalBytes += winRes.BytesFreed;
                filesCount += winRes.FilesDeleted;

                var updateCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
                var updateRes = CleanDirectory(updateCache);
                totalBytes += updateRes.BytesFreed;
                filesCount += updateRes.FilesDeleted;

                try { NativeMethods.DnsFlushResolverCache(); } catch { }

                PurgeWorkingSets();

                return new CleanResult
                {
                    BytesFreed = totalBytes,
                    FilesDeleted = filesCount,
                    Message = $"تم الانتهاء بنجاح! تم حذف {filesCount} ملف وتحرير {FormatBytes(totalBytes)} واستعادة كفاءة ويندوز."
                };
            });
        }

        public async Task<CleanResult> CleanBrowserCachesAsync()
        {
            return await Task.Run(() =>
            {
                long bytes = 0;
                int files = 0;
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                var edgeCache = Path.Combine(localAppData, @"Microsoft\Edge\User Data\Default\Cache\Cache_Data");
                var edgeRes = CleanDirectory(edgeCache);
                bytes += edgeRes.BytesFreed;
                files += edgeRes.FilesDeleted;

                var chromeCache = Path.Combine(localAppData, @"Google\Chrome\User Data\Default\Cache\Cache_Data");
                var chromeRes = CleanDirectory(chromeCache);
                bytes += chromeRes.BytesFreed;
                files += chromeRes.FilesDeleted;

                return new CleanResult
                {
                    BytesFreed = bytes,
                    FilesDeleted = files,
                    Message = $"تم تنظيف كاش المتصفحات وتحرير {FormatBytes(bytes)}."
                };
            });
        }

        private CleanResult CleanDirectory(string dirPath)
        {
            long bytes = 0;
            int count = 0;
            if (!Directory.Exists(dirPath)) return new CleanResult();

            try
            {
                var dir = new DirectoryInfo(dirPath);
                foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        bytes += file.Length;
                        file.Delete();
                        count++;
                    }
                    catch { }
                }
            }
            catch { }

            return new CleanResult { BytesFreed = bytes, FilesDeleted = count };
        }

        private void PurgeWorkingSets()
        {
            try
            {
                var processes = Process.GetProcesses();
                foreach (var proc in processes)
                {
                    try
                    {
                        if (!proc.HasExited)
                        {
                            NativeMethods.EmptyWorkingSet(proc.Handle);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}

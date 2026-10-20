using System;
using Microsoft.Win32;

namespace WinTAVRYNEXO.Services
{
    public class PrivacyService
    {
        public bool DisableTelemetry()
        {
            try
            {
                using var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection");
                key?.SetValue("AllowTelemetry", 0, RegistryValueKind.DWord);

                using var adKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo");
                adKey?.SetValue("Enabled", 0, RegistryValueKind.DWord);

                using var actKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\System");
                actKey?.SetValue("PublishUserActivities", 0, RegistryValueKind.DWord);

                using var searchKey = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Windows\Explorer");
                searchKey?.SetValue("DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord);

                return true;
            }
            catch { return false; }
        }

        public bool ClearActivityHistory()
        {
            try
            {
                var recentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                if (System.IO.Directory.Exists(recentFolder))
                {
                    foreach (var file in System.IO.Directory.GetFiles(recentFolder))
                    {
                        try { System.IO.File.Delete(file); } catch { }
                    }
                }
                return true;
            }
            catch { return false; }
        }
    }
}

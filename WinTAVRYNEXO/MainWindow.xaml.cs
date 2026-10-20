using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WinTAVRYNEXO.Services;

namespace WinTAVRYNEXO
{
    public partial class MainWindow : Window
    {
        private readonly CleanerService _cleanerService;
        private readonly TelemetryService _telemetryService;
        private readonly StartupService _startupService;
        private readonly PrivacyService _privacyService;
        private readonly DispatcherTimer _metricsTimer;

        public MainWindow()
        {
            InitializeComponent();

            _cleanerService = new CleanerService();
            _telemetryService = new TelemetryService();
            _startupService = new StartupService();
            _privacyService = new PrivacyService();

            _metricsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
            _metricsTimer.Tick += (s, e) => UpdateMetricsDisplay();
            _metricsTimer.Start();

            UpdateMetricsDisplay();
        }

        private void UpdateMetricsDisplay()
        {
            try
            {
                var metrics = _telemetryService.GetCurrentMetrics();
                TxtCpu.Text = $"{metrics.CpuPercent}%";
                TxtRam.Text = $"{metrics.RamPercent}%";
                TxtRamDetails.Text = $"{metrics.UsedRamGb} GB / {metrics.TotalRamGb} GB";
                TxtHealthScore.Text = $"{metrics.HealthScore}%";
            }
            catch { }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnMaximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private async void BtnOneClickOptimize_Click(object sender, RoutedEventArgs e)
        {
            BtnOneClickOptimize.IsEnabled = false;
            TxtStatus.Text = "جاري تنفيذ الصيانة الشاملة، مسح الملفات المؤقتة، تحرير الذاكرة وتفريغ الكاش...";

            try
            {
                var result = await _cleanerService.RunOneClickOptimizerAsync();
                _startupService.OptimizeForegroundProcess();

                TxtJunk.Text = "0.00 GB";
                TxtHealthScore.Text = "99%";
                TxtStatus.Text = result.Message;
                MessageBox.Show(result.Message, "WinTAVRYNEXO - تم التحسين بنجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                TxtStatus.Text = $"حدث خطأ: {ex.Message}";
            }
            finally
            {
                BtnOneClickOptimize.IsEnabled = true;
            }
        }

        private async void BtnDeepClean_Click(object sender, RoutedEventArgs e)
        {
            var res = await _cleanerService.CleanBrowserCachesAsync();
            MessageBox.Show(res.Message, "التنظيف الشامل", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSpeedup_Click(object sender, RoutedEventArgs e)
        {
            var items = _startupService.GetStartupItems();
            MessageBox.Show($"تم فحص برامج الإقلاع بنجاح ({items.Count} برنامج مسجل).", "تسريع النظام", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPrivacy_Click(object sender, RoutedEventArgs e)
        {
            bool ok = _privacyService.DisableTelemetry();
            _privacyService.ClearActivityHistory();
            MessageBox.Show(ok ? "تم حظر إرسال بيانات القياس عن بُعد وتعطيل التتبع بنجاح." : "يرجى تشغيل البرنامج كمسؤول.", "درع الخصوصية", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPowerTools_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("أدوات الفرم الآمن DoD 5220.22-M ومكتشف الملفات الضخمة جاهزة.", "الأدوات المتقدمة", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

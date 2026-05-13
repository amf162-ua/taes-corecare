using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CoreCare.Models;
using CoreCare.Services;
using CoreCare.Data;
using CoreCare.Orchestrators;

namespace CoreCare.Views.Modals
{
    public partial class BenchmarkProgressWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly ScanOptions _options;
        private readonly int _durationSeconds;
        private readonly BenchmarkOrchestrator _orchestrator;
        private readonly HardwareMonitorService _monitor;
        private DateTime _startTime;
        private bool _isCompleted;

        public RegistroBenchmark FinalResult { get; private set; }

        public BenchmarkProgressWindow(string benchmarkName, ScanOptions options, int durationSeconds)
        {
            InitializeComponent();
            TxtBenchmarkName.Text = benchmarkName.ToUpper();

            _options = options;
            _durationSeconds = Math.Max(1, durationSeconds);
            _monitor = App.Monitor;
            _orchestrator = new BenchmarkOrchestrator(_monitor, new Models.StressWorker());

            // Configurar el temporizador para la actualización visual
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _timer.Tick += Timer_Tick;

            Loaded += BenchmarkProgressWindow_Loaded;
        }

        private void BenchmarkProgressWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _startTime = DateTime.UtcNow;
            _timer.Start();
            _ = RunBenchmarkAsync();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var elapsed = DateTime.UtcNow - _startTime;
            var total = TimeSpan.FromSeconds(_durationSeconds);
            var progress = Math.Min(1.0, elapsed.TotalMilliseconds / total.TotalMilliseconds);

            ProgressBar.Value = progress * 100.0;
            TxtPercent.Text = $"{(int)(progress * 100)}%";

            UpdateTelemetry();

            if (_isCompleted)
            {
                _timer.Stop();
            }
        }

        private async Task RunBenchmarkAsync()
        {
            try
            {
                TxtStatus.Text = "EJECUTANDO BENCHMARK...";
                var registro = await _orchestrator.RunBenchmarkAsync(_options, _durationSeconds);

                using var db = new CoreCareDbContext();
                var historyService = new BenchmarkHistoryService(db);

                var currentUser = SessionService.CurrentUser;
                if (currentUser == null)
                {
                    throw new InvalidOperationException("No hay sesión activa para guardar el benchmark.");
                }

                registro.UserId = currentUser.Id;

                historyService.SaveBenchmark(registro);

                FinalResult = registro;
                _isCompleted = true;

                Dispatcher.Invoke(() =>
                {
                    TxtStatus.Text = "FINALIZADO";
                    DialogResult = true;
                    Close();
                });
            }
            catch (Exception ex)
            {
                _isCompleted = true;
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Error al ejecutar el benchmark: " + ex.GetBaseException().Message);
                    DialogResult = false;
                });
            }
        }

        private void UpdateTelemetry()
        {
            try
            {
                _monitor.UpdateHardware();

                if (_options.ScanCPU && _monitor.TryGetCpuLoad(out var cpuLoad, out _))
                {
                    TxtCpu.Text = $"{cpuLoad:F0}%";
                }
                else
                {
                    TxtCpu.Text = "N/A";
                }

                if (_options.ScanRAM && _monitor.TryGetRamLoad(out var ramLoad, out _))
                {
                    TxtRam.Text = $"{ramLoad:F0}%";
                }
                else
                {
                    TxtRam.Text = "N/A";
                }

                var temps = new[]
                {
                    _options.ScanCPU && _monitor.TryGetCpuTemperature(out var cpuTemp, out _) ? cpuTemp.Value : (float?)null,
                    _options.ScanGPU && _monitor.TryGetGpuTemperature(out var gpuTemp, out _) ? gpuTemp : (float?)null
                }.Where(t => t.HasValue).Select(t => t!.Value).ToList();

                if (temps.Any())
                {
                    TxtTemp.Text = $"{temps.Average():F0}°C";
                }
                else
                {
                    TxtTemp.Text = "N/A";
                }
            }
            catch
            {
                TxtCpu.Text = "N/A";
                TxtRam.Text = "N/A";
                TxtTemp.Text = "N/A";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            this.DialogResult = false;
            this.Close();
        }
    }
}
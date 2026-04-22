using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreCare.Models;
using CoreCare.Services;
using CoreCare.Data;
using CoreCare.Orchestrators;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Linq;
using System.Windows.Media;
using System.Text;

namespace CoreCare.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly HardwareMonitorService _hardwareService;
        private readonly ProcessService _processService;
        private readonly DispatcherTimer _refreshTimer;

        [ObservableProperty]
        private string _cpuDisplay;

        [ObservableProperty]
        private DateTime _historyFrom = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        private DateTime _historyTo = DateTime.Today;

        [ObservableProperty]
        private string _historyStatus = "Sin datos cargados.";

        [ObservableProperty]
        private string _historyUserLabel = "Usuario: -";

        [ObservableProperty]
        private string _degradationStatus = "Analisis de degradacion pendiente.";

        [ObservableProperty]
        private string _trendStatus = "Tendencia pendiente.";

        [ObservableProperty]
        private Brush _degradationBrush = Brushes.DimGray;

        [ObservableProperty]
        private BenchmarkOptionItem? _selectedBenchmarkOption;

        [ObservableProperty]
        private bool _isBenchmarkRunning;

        [ObservableProperty]
        private string _benchmarkStatus = "Listo para ejecutar benchmark.";

        [ObservableProperty]
        private string _benchmarkSummary = "Sin ejecuciones todavia.";

        public int BenchmarkDurationSeconds { get; } = 8;

        public ObservableCollection<ProcessItem> Processes { get; set; } = new();
        public ObservableCollection<RegistroBenchmark> HistoryItems { get; } = new();
        public ObservableCollection<BenchmarkOptionItem> BenchmarkOptions { get; } = new()
        {
            new BenchmarkOptionItem { Code = "1", Label = "Monitor CPU" },
            new BenchmarkOptionItem { Code = "2", Label = "Monitor GPU" },
            new BenchmarkOptionItem { Code = "3", Label = "Monitor RAM" },
            new BenchmarkOptionItem { Code = "4", Label = "Monitor Disco" },
            new BenchmarkOptionItem { Code = "5", Label = "Monitor Todo" }
        };

        public MainViewModel()
        {
            _hardwareService = new HardwareMonitorService();
            _processService = new ProcessService();

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _refreshTimer.Tick += (s, e) => UpdateAllData();
            _refreshTimer.Start();

            SelectedBenchmarkOption = BenchmarkOptions.Last();

            UpdateAllData();
            LoadHistory();
        }

        private void UpdateAllData()
        {
            try
            {
                _hardwareService.UpdateHardware();

                float load = _hardwareService.GetCpuLoad();
                CpuDisplay = $"{Math.Round(load, 1)} %";

                var list = _processService.GetActiveProcesses();

                Processes.Clear();
                foreach (var item in list)
                {
                    Processes.Add(item);
                }
            }
            catch
            {
                CpuDisplay = "N/A";
            }
        }

        [RelayCommand]
        private async Task RunBenchmarkAsync()
        {
            if (IsBenchmarkRunning)
            {
                return;
            }

            if (SelectedBenchmarkOption == null)
            {
                BenchmarkStatus = "Selecciona un modo de benchmark antes de ejecutar.";
                return;
            }

            IsBenchmarkRunning = true;
            BenchmarkStatus = "Iniciando benchmark...";

            try
            {
                _refreshTimer.Stop();

                var stressWorker = new StressWorker();
                var orchestrator = new BenchmarkOrchestrator(_hardwareService, stressWorker);
                var options = MapBenchmarkOption(SelectedBenchmarkOption.Code);

                var benchmarkTask = orchestrator.RunBenchmarkAsync(options, BenchmarkDurationSeconds);
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(BenchmarkDurationSeconds + 20));

                var completedTask = await Task.WhenAny(benchmarkTask, timeoutTask);
                if (completedTask != benchmarkTask)
                {
                    stressWorker.Stop();
                    BenchmarkStatus = "El benchmark no finalizo a tiempo. Revisa sensores GPU/CPU y vuelve a intentar.";
                    return;
                }

                var registro = await benchmarkTask;

                using var db = new CoreCareDbContext();
                registro.UserId = GetOrCreateSystemUserId(db);
                db.RegistrosBenchmark.Add(registro);
                db.SaveChanges();

                BenchmarkStatus = $"Resultado guardado en la base de datos con Id {registro.Id}.";
                BenchmarkSummary = BuildBenchmarkSummary(registro, options);

                LoadHistory();
            }
            catch (Exception ex)
            {
                BenchmarkStatus = $"Error durante benchmark: {ex.GetBaseException().Message}";
                BenchmarkSummary = "No se pudo generar resumen por un error en la ejecucion.";
            }
            finally
            {
                _refreshTimer.Start();
                UpdateAllData();
                IsBenchmarkRunning = false;
            }
        }

        [RelayCommand]
        public void TerminateProcess(ProcessItem process)
        {
            if (process == null) return;

            var result = MessageBox.Show(
                $"¿Seguro que quieres cerrar {process.Name}?\nSe perderán los datos no guardados.",
                "Confirmar acción",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // SOLUCIÓN 3: El método en ProcessService se llama KillProcess, no TerminateProcess
                bool ok = _processService.KillProcess(process.Id);

                if (ok)
                {
                    UpdateAllData();
                }
                else
                {
                    MessageBox.Show("No se pudo cerrar. Puede que no tengas permisos o el proceso ya haya terminado.");
                }
            }
        }

        [RelayCommand]
        private void LoadHistory()
        {
            try
            {
                using var db = new CoreCareDbContext();
                var currentUser = GetOrCreateSystemUser(db);
                int userId = currentUser.Id;

                var historyService = new BenchmarkHistoryService(db);

                DateTime from = HistoryFrom.Date;
                DateTime to = HistoryTo.Date.AddDays(1).AddTicks(-1);

                if (from > to)
                {
                    HistoryStatus = "Rango invalido: 'Desde' no puede ser mayor que 'Hasta'.";
                    return;
                }

                var history = historyService.GetHistory(userId, from, to, 200);

                HistoryItems.Clear();
                foreach (var item in history)
                {
                    HistoryItems.Add(item);
                }

                string username = string.IsNullOrWhiteSpace(currentUser.username)
                    ? currentUser.name
                    : currentUser.username;
                HistoryUserLabel = $"Usuario: {username}";
                HistoryStatus = $"{history.Count} registros en el rango {from:yyyy-MM-dd} a {to:yyyy-MM-dd}.";

                var trend = historyService.BuildTrend(history);
                TrendStatus = BuildTrendStatus(trend);

                var degradation = historyService.AnalyzeDegradation(history);
                DegradationStatus = degradation.Message;
                DegradationBrush = !degradation.HasEnoughData
                    ? Brushes.DarkGoldenrod
                    : degradation.IsDegraded ? Brushes.Firebrick : Brushes.SeaGreen;
            }
            catch (Exception ex)
            {
                HistoryStatus = $"Error cargando historico: {ex.GetBaseException().Message}";
                HistoryUserLabel = "Usuario: -";
                DegradationStatus = "No se pudo calcular degradacion.";
                TrendStatus = "No se pudo calcular tendencia.";
                DegradationBrush = Brushes.Firebrick;
            }
        }

        [RelayCommand]
        private void ShowHistoryDetails(RegistroBenchmark? registro)
        {
            if (registro == null)
            {
                return;
            }

            var detailsWindow = new BenchmarkDetailsWindow(registro)
            {
                Owner = Application.Current?.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            detailsWindow.ShowDialog();
        }

        private static string BuildTrendStatus(IReadOnlyList<BenchmarkTrendPoint> trend)
        {
            if (trend.Count < 2)
            {
                return "Tendencia: datos insuficientes para comparar evolucion.";
            }

            var first = trend.First();
            var last = trend.Last();
            float scoreDelta = last.Score - first.Score;
            string direction = scoreDelta > 0f ? "mejora" : scoreDelta < 0f ? "empeora" : "estable";

            return $"Tendencia extremo-a-extremo: inicio {first.Score:F1}, fin {last.Score:F1} ({Math.Abs(scoreDelta):F1}, {direction}).";
        }

        private static ScanOptions MapBenchmarkOption(string code)
        {
            return code switch
            {
                "1" => ScanOptions.ScanCPUOnly(),
                "2" => ScanOptions.ScanGPUOnly(),
                "3" => ScanOptions.ScanRAMOnly(),
                "4" => ScanOptions.ScanDiskOnly(),
                _ => ScanOptions.FullScan()
            };
        }

        private static string BuildBenchmarkSummary(RegistroBenchmark registro, ScanOptions options)
        {
            var builder = new StringBuilder();

            builder.AppendLine("---------------- RESULTADO ----------------");
            builder.AppendLine($"Timestamp: {registro.Timestamp:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Score:     {registro.Score:F1}/10");

            var unavailableReadings = registro.SensorReadings
                .Where(reading => reading.Name.Contains("NO DISPONIBLE", StringComparison.OrdinalIgnoreCase))
                .Select(reading => reading.Name)
                .Distinct()
                .ToList();

            if (options.ScanCPU)
            {
                builder.AppendLine();
                builder.AppendLine("[CPU]");
                builder.AppendLine($"Carga media:      {registro.CpuLoad:F1} %");
                builder.AppendLine($"Temperatura media:{registro.CpuTemp:F1} C");
                builder.AppendLine($"Frecuencia media: {registro.CpuClock:F2} GHz");
            }

            if (options.ScanGPU)
            {
                builder.AppendLine();
                builder.AppendLine("[GPU]");
                builder.AppendLine($"Carga media:      {registro.GpuLoad:F1} %");
                builder.AppendLine($"Temperatura media:{registro.GpuTemp:F1} C");
            }

            if (options.ScanRAM)
            {
                builder.AppendLine();
                builder.AppendLine("[RAM]");
                builder.AppendLine($"Uso medio:        {registro.RamUsed:F2} GB");
                builder.AppendLine($"Carga media:      {registro.RamLoad:F1} %");
            }

            if (options.ScanDisk)
            {
                builder.AppendLine();
                builder.AppendLine("[DISCO]");
                builder.AppendLine($"Carga media:      {registro.DiskLoad:F1} %");
                builder.AppendLine($"Lectura media:    {registro.DiskReadRate:F2} Mb/s");
                builder.AppendLine($"Escritura media:  {registro.DiskWriteRate:F2} Mb/s");
            }

            if (unavailableReadings.Any())
            {
                builder.AppendLine();
                builder.AppendLine("[SENSORES NO DISPONIBLES]");
                foreach (var unavailable in unavailableReadings)
                {
                    builder.AppendLine($"- {unavailable}");
                }
            }

            builder.AppendLine("-------------------------------------------");

            return builder.ToString();
        }

        private static int GetOrCreateSystemUserId(CoreCareDbContext db)
            => GetOrCreateSystemUser(db).Id;

        private static User GetOrCreateSystemUser(CoreCareDbContext db)
        {
            var existingUser = db.Users.FirstOrDefault(user => user.IsActive);

            if (existingUser != null)
            {
                return existingUser;
            }

            var systemUser = new User
            {
                name = "prueba",
                username = "prueba",
                email = "prueba@corecare.local",
                password = string.Empty,
                createdAt = DateTime.UtcNow,
                IsActive = true,
                Plan = TipoPlan.Basico
            };

            db.Users.Add(systemUser);
            db.SaveChanges();

            return systemUser;
        }

        public sealed class BenchmarkOptionItem
        {
            public required string Code { get; set; }
            public required string Label { get; set; }
        }
    }
}
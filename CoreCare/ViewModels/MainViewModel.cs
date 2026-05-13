using System;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreCare.Models;
using CoreCare.Services;
using CoreCare.Views;
using CoreCare.Data;
using CoreCare.Orchestrators;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using QuestPDF.Fluent;
using System.Linq;
using System.Windows.Media;
using System.Text;

namespace CoreCare.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly HardwareMonitorService _hardwareService;
        private readonly ProcessService _processService;
        private readonly GeminiAIService _geminiService;
        private readonly SystemSpecsService _systemSpecsService;
        private readonly HardwareUpgradeAdvisorService _upgradeAdvisorService;
        private readonly DispatcherTimer _refreshTimer;

        [ObservableProperty]
        private string _cpuDisplay = "N/A";

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private DateTime _historyFrom = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        private DateTime _historyTo = DateTime.Today;

        [ObservableProperty]
        private string _historyStatus = "Sin datos cargados.";

        [ObservableProperty]
        private string _historyUserLabel = "Usuario: -";

        public bool IsAuthenticated => SessionService.CurrentUser != null;

        public bool IsAdministrador => SessionService.CurrentUser?.Role == UserRole.Administrador;

        public bool IsCliente => SessionService.CurrentUser?.Role == UserRole.Cliente;

        public string CurrentUserDisplayName => SessionService.CurrentUser == null
            ? "Invitado"
            : (string.IsNullOrWhiteSpace(SessionService.CurrentUser.username) ? SessionService.CurrentUser.name : SessionService.CurrentUser.username);

        public string CurrentRoleLabel => SessionService.CurrentUser?.Role switch
        {
            UserRole.Administrador => "Administrador",
            UserRole.Cliente => "Cliente",
            _ => "Sin rol"
        };

        [ObservableProperty]
        private string _degradationStatus = "Analisis de degradacion pendiente.";

        [ObservableProperty]
        private string _trendStatus = "Tendencia pendiente.";

        [ObservableProperty]
        private PlotModel? _trendPlotModel;

        [ObservableProperty]
        private PlotModel? _scoreBarPlotModel;

        // last subset used for plotting (most recent up to 10 runs)
        public List<RegistroBenchmark> LastHistorySubset { get; private set; } = new();

        [ObservableProperty]
        private string _historyHoverInfo = string.Empty;

        [ObservableProperty]
        private string _kpiCurrentScore = "—";

        [ObservableProperty]
        private string _kpiDegradation = "—";

        [ObservableProperty]
        private string _kpiWorstComponent = "—";

        [ObservableProperty]
        private string _kpiConsistency = "—";

        // heatmap: list of (MetricName, List<values>)
        public List<(string Name, List<double> Values)> HeatmapData { get; private set; } = new();

        // small multiples: store mini chart models
        [ObservableProperty]
        private PlotModel? _miniCpuPlot;

        [ObservableProperty]
        private PlotModel? _miniGpuPlot;

        [ObservableProperty]
        private PlotModel? _miniRamPlot;

        [ObservableProperty]
        private PlotModel? _miniDiskPlot;

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
        public static IReadOnlyList<BenchmarkOptionItem> DefaultBenchmarkOptions { get; } =
            new List<BenchmarkOptionItem>
            {
                new BenchmarkOptionItem { Code = "1", Label = "Monitor CPU" },
                new BenchmarkOptionItem { Code = "2", Label = "Monitor GPU" },
                new BenchmarkOptionItem { Code = "3", Label = "Monitor RAM" },
                new BenchmarkOptionItem { Code = "4", Label = "Monitor Disco" },
                new BenchmarkOptionItem { Code = "5", Label = "Monitor Todo" }
            };

        public ObservableCollection<BenchmarkOptionItem> BenchmarkOptions { get; } =
            new(DefaultBenchmarkOptions);

        public MainViewModel()
        {
            _hardwareService = App.Monitor;
            _processService = App.Processes;
            _geminiService = new GeminiAIService();
            _systemSpecsService = new SystemSpecsService();
            _upgradeAdvisorService = new HardwareUpgradeAdvisorService();

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _refreshTimer.Tick += (s, e) => UpdateAllData();
            _refreshTimer.Start();

            SelectedBenchmarkOption = BenchmarkOptions.Last();
            HistoryUserLabel = $"Usuario: {CurrentUserDisplayName} ({CurrentRoleLabel})";

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

            if (!IsAuthenticated)
            {
                BenchmarkStatus = "Debes iniciar sesión para ejecutar benchmarks.";
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
                registro.UserId = SessionService.CurrentUser!.Id;
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

            if (!IsAdministrador)
            {
                MessageBox.Show("Esta acción solo está disponible para administradores.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

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

        private static System.Collections.Generic.List<Sponsor> GetFixedSponsors()
        {
            return new System.Collections.Generic.List<Sponsor>
            {
                new Sponsor { Name = "PC Componentes", Message = "¡Encuentra los mejores componentes al mejor precio!", Website = "https://www.pccomponentes.com" },
                new Sponsor { Name = "Amazon", Message = "Envío rápido en miles de productos de hardware.", Website = "https://www.amazon.es" },
                new Sponsor { Name = "Coolmod", Message = "Especialistas en refrigeración y modding.", Website = "https://www.coolmod.com" }
            };
        }

        [RelayCommand]
        public async Task GenerateReportAsync()
        {
            if (IsBusy)
            {
                return;
            }

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"Informe_CoreCare_{DateTime.Now:yyyyMMdd_HHmmss}",
                DefaultExt = ".pdf",
                Filter = "PDF files (*.pdf)|*.pdf"
            };

            if (saveDialog.ShowDialog() != true)
            {
                return;
            }

            LoadingWindow? loadingWindow = null;
            IsBusy = true;

            try
            {
                loadingWindow = new LoadingWindow
                {
                    Owner = Application.Current?.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                loadingWindow.Show();

                loadingWindow.UpdateProgress(0, "Preparando informe...");

                var hardwareTask = Task.Run(() =>
                {
                    _hardwareService.UpdateHardware();
                    _hardwareService.UpdateHardware();
                });
                var specsTask = Task.Run(() => _systemSpecsService.GetSystemSpecs());

                await Task.WhenAll(hardwareTask, specsTask);

                loadingWindow.UpdateProgress(25, "Analizando telemetría...");

                var systemSpecs = specsTask.Result;
                var telemetryWarnings = new System.Collections.Generic.List<string>();
                var telemetryData = BuildReportTelemetry(systemSpecs, telemetryWarnings);
                var (upgradeScores, upgradeRecommendations) = _upgradeAdvisorService.Analyze(systemSpecs, telemetryData);

                loadingWindow.UpdateProgress(45, "Preparando recomendaciones...");

                var (recommendations, generatedLocally) = await GetFastRecommendationsAsync(telemetryData, systemSpecs);

                loadingWindow.UpdateProgress(80, "Generando PDF...");

                var data = new ReportData
                {
                    CompanyName = "TechRepairs S.L.",
                    ClientName = "Jesús Pérez",
                    ReportDate = DateTime.Now,
                    SystemSpecs = systemSpecs,
                    Recommendations = recommendations,
                    TelemetryWarnings = telemetryWarnings,
                    RecommendationsGeneratedLocally = generatedLocally,
                    UpgradeScores = upgradeScores,
                    UpgradeRecommendations = upgradeRecommendations,
                    TelemetryData = telemetryData,
                    Sponsors = GetFixedSponsors()
                };

                var document = new ReportDocument(data);
                document.GeneratePdf(saveDialog.FileName);

                loadingWindow.UpdateProgress(100, "Completado");
                loadingWindow.Close();

                MessageBox.Show($"Informe generado con éxito.\nGuardado en: {saveDialog.FileName}", "PDF generado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                loadingWindow?.Close();
                IsBusy = false;
            }
        }

        private SystemTelemetryMock BuildReportTelemetry(SystemSpecs systemSpecs, System.Collections.Generic.List<string> telemetryWarnings)
        {
                float ramUsed = (float)Math.Round(_hardwareService.GetRamUsageGb(), 1);
                float ramAvailable = (float)Math.Round(_hardwareService.GetRamAvailableGb(), 1);
                float ramTotal = (float)Math.Round(ramUsed + ramAvailable, 1);

                var cpuTemperature = -1f;
                if (_hardwareService.TryGetCpuTemperature(out var cpuTemperatureData, out var cpuTemperatureReason))
                {
                    cpuTemperature = (float)Math.Round(cpuTemperatureData.Value, 1, MidpointRounding.ToEven);
                }
                else
                {
                    telemetryWarnings.Add($"Temperatura CPU no disponible: {cpuTemperatureReason}");
                }

                var diskUsagePercent = -1f;
                if (_hardwareService.TryGetDiskLoad(out var diskLoad, out var diskLoadReason))
                {
                    diskUsagePercent = (float)Math.Round(diskLoad, 1, MidpointRounding.ToEven);
                }
                else
                {
                    telemetryWarnings.Add($"Uso de disco no disponible: {diskLoadReason}");
                }

                return new SystemTelemetryMock
                {
                    CpuUsagePercent = (float)Math.Round(_hardwareService.GetCpuLoad(), 1, MidpointRounding.ToEven),
                    CpuTemperatureC = cpuTemperature,
                    GpuUsagePercent = (float)Math.Round(_hardwareService.GetGpuLoad(), 1, MidpointRounding.ToEven),
                    GpuTemperatureC = (float)Math.Round(_hardwareService.GetGpuTemperature(), 1, MidpointRounding.ToEven),
                    RamTotalGb = ramTotal,
                    RamUsedGb = ramUsed,
                    DiskType = systemSpecs.DiskModel,
                    DiskUsagePercent = diskUsagePercent,
                };
        }

        private async Task<(System.Collections.Generic.List<string> Recommendations, bool GeneratedLocally)> GetFastRecommendationsAsync(SystemTelemetryMock telemetryData, SystemSpecs systemSpecs)
        {
            try
            {
                var aiResponse = await _geminiService.GetRecommendationsAsync(telemetryData, systemSpecs);
                if (!aiResponse.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
                {
                    return (new System.Collections.Generic.List<string> { aiResponse }, false);
                }
            }
            catch
            {
            }

            return (BuildLocalRecommendations(telemetryData, systemSpecs), true);
        }

        private static System.Collections.Generic.List<string> BuildLocalRecommendations(SystemTelemetryMock telemetryData, SystemSpecs systemSpecs)
        {
            var recommendations = new System.Collections.Generic.List<string>();

            if (telemetryData.CpuUsagePercent > 80 || telemetryData.CpuTemperatureC > 85)
            {
                recommendations.Add($@"=== PRIORIDAD ALTA ===
[CPU]
- Problema: {(telemetryData.CpuTemperatureC > 85 ? $"La temperatura de CPU alcanza {telemetryData.CpuTemperatureC:F1} C, por encima del rango recomendado para uso sostenido." : $"El uso de CPU está en {telemetryData.CpuUsagePercent:F1}%, por encima del rango cómodo para uso sostenido.")}
- Solución: Ejecutar CoreCare como administrador, comprobar refrigeración y cerrar procesos intensivos antes de tareas críticas.
- Coste: Bajo / Medio
- Impacto: Alto");
            }

            if (telemetryData.RamTotalGb > 0 && telemetryData.RamUsedGb / telemetryData.RamTotalGb > 0.8)
            {
                recommendations.Add($@"=== PRIORIDAD MEDIA ===
[Memoria RAM]
- Problema: La memoria utilizada está cerca del límite disponible ({telemetryData.RamUsedGb:F1} GB de {telemetryData.RamTotalGb:F1} GB).
- Solución: Cerrar aplicaciones en segundo plano o ampliar RAM si esta situación se repite.
- Coste: Medio
- Impacto: Medio / Alto");
            }

            if (telemetryData.DiskUsagePercent > 80)
            {
                recommendations.Add($@"=== PRIORIDAD MEDIA ===
[Almacenamiento]
- Problema: El disco presenta una carga elevada ({telemetryData.DiskUsagePercent:F1}%).
- Solución: Revisar procesos de escritura, estado SMART y espacio disponible en {systemSpecs.DiskModel}.
- Coste: Bajo
- Impacto: Medio");
            }

            if (telemetryData.GpuUsagePercent > 80 || telemetryData.GpuTemperatureC > 85)
            {
                recommendations.Add($@"=== PRIORIDAD MEDIA ===
[GPU]
- Problema: La GPU muestra carga o temperatura elevada.
- Solución: Revisar drivers, ventilación y aplicaciones con aceleración gráfica activa.
- Coste: Bajo
- Impacto: Medio");
            }

            return recommendations;
        }

        [RelayCommand]
        private void LoadHistory()
        {
            try
            {
                using var db = new CoreCareDbContext();
                var currentUser = SessionService.CurrentUser;

                if (currentUser == null)
                {
                    HistoryStatus = "Debes iniciar sesión para consultar el historial.";
                    HistoryUserLabel = "Usuario: -";
                    return;
                }

                int userId = currentUser.Id;

                var historyService = new BenchmarkHistoryService(db);

                DateTime from = HistoryFrom.Date;
                DateTime to = HistoryTo.Date.AddDays(1).AddTicks(-1);

                if (from > to)
                {
                    HistoryStatus = "Rango invalido: 'Desde' no puede ser mayor que 'Hasta'.";
                    return;
                }

                var history = historyService.GetHistory(userId, from, to, 200)
                    .OrderByDescending(h => h.Timestamp)
                    .ToList();

                var orderedForLabels = history
                    .OrderBy(h => h.Timestamp)
                    .ToList();

                for (int i = 0; i < orderedForLabels.Count; i++)
                {
                    orderedForLabels[i].RunLabel = $"R{i + 1}";
                }

                var runLabelById = orderedForLabels.ToDictionary(item => item.Id, item => item.RunLabel);

                for (int i = 0; i < history.Count; i++)
                {
                    if (runLabelById.TryGetValue(history[i].Id, out var runLabel))
                    {
                        history[i].RunLabel = runLabel;
                    }
                }

                HistoryItems.Clear();
                foreach (var item in history)
                {
                    HistoryItems.Add(item);
                }

                string username = string.IsNullOrWhiteSpace(currentUser.username)
                    ? currentUser.name
                    : currentUser.username;
                HistoryUserLabel = $"Usuario: {username} ({currentUser.Role})";
                HistoryStatus = $"{history.Count} registros en el rango {from:yyyy-MM-dd} a {to:yyyy-MM-dd}.";

                var trend = historyService.BuildTrend(history);
                TrendStatus = BuildTrendStatus(trend);

                // limit to last 10 runs for charts, then restore chronological order
                var subset = history
                    .Take(10)
                    .OrderBy(h => h.Timestamp)
                    .ToList();
                LastHistorySubset = subset;

                // build chart models from subset
                TrendPlotModel = BuildTrendPlotModel(subset);
                ScoreBarPlotModel = BuildScoreBarPlotModel(subset);

                // build new layout: KPIs, mini charts, heatmap
                // Build heatmap FIRST so data is ready when MiniCpuPlot PropertyChanged triggers DrawHeatmap
                BuildHeatmap(subset);
                BuildKPIs(subset);
                BuildMiniCharts(subset);

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

        public sealed class BenchmarkOptionItem
        {
            public required string Code { get; set; }
            public required string Label { get; set; }
        }

        private PlotModel BuildTrendPlotModel(IReadOnlyCollection<RegistroBenchmark> history)
        {
            var model = new PlotModel { Title = "Tendencias por métrica" };

            var dateAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "yyyy-MM-dd",
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                Angle = 45
            };

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = 100,
                Title = "% / magnitud",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            };

            model.Axes.Add(dateAxis);
            model.Axes.Add(valueAxis);

            var cpuSeries = new LineSeries { Title = "CPU %", Color = OxyColors.SkyBlue, StrokeThickness = 2 };
            var gpuSeries = new LineSeries { Title = "GPU %", Color = OxyColors.Orange, StrokeThickness = 2 };
            var ramSeries = new LineSeries { Title = "RAM %", Color = OxyColors.MediumSeaGreen, StrokeThickness = 2 };
            var diskSeries = new LineSeries { Title = "Disco %", Color = OxyColors.PaleVioletRed, StrokeThickness = 2 };

            // assume 'history' is already ordered and limited
            foreach (var r in history)
            {
                double x = DateTimeAxis.ToDouble(r.Timestamp);
                cpuSeries.Points.Add(new DataPoint(x, r.CpuLoad));
                gpuSeries.Points.Add(new DataPoint(x, r.GpuLoad));
                ramSeries.Points.Add(new DataPoint(x, r.RamLoad));
                diskSeries.Points.Add(new DataPoint(x, r.DiskLoad));
            }

            model.Series.Add(cpuSeries);
            model.Series.Add(gpuSeries);
            model.Series.Add(ramSeries);
            model.Series.Add(diskSeries);

            // show trackers on hover with a concise format
            cpuSeries.TrackerFormatString = "{0}\n{1:yyyy-MM-dd}: CPU={2:0.0}%";
            gpuSeries.TrackerFormatString = "{0}\n{1:yyyy-MM-dd}: GPU={2:0.0}%";
            ramSeries.TrackerFormatString = "{0}\n{1:yyyy-MM-dd}: RAM={2:0.0}%";
            diskSeries.TrackerFormatString = "{0}\n{1:yyyy-MM-dd}: Disco={2:0.0}%";

            return model;
        }

        private PlotModel BuildScoreBarPlotModel(IReadOnlyCollection<RegistroBenchmark> history)
        {
            var model = new PlotModel { Title = "Score por ejecución" };

            var categoryAxis = new CategoryAxis { Position = AxisPosition.Bottom, Angle = 45 };
            var valueAxis = new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = 10, Title = "Score (0-10)" };

            var columnSeries = new BarSeries { StrokeColor = OxyColors.Black, StrokeThickness = 1, FillColor = OxyColors.SteelBlue };

            var ordered = history.ToList();
            foreach (var r in ordered)
            {
                categoryAxis.Labels.Add(r.Timestamp.ToString("yyyy-MM-dd"));

                // color by performance tier
                OxyColor color;
                if (r.Score >= 7.5f) color = OxyColor.FromRgb(34, 197, 94); // green
                else if (r.Score >= 5f) color = OxyColor.FromRgb(234, 179, 8); // yellow
                else color = OxyColor.FromRgb(220, 38, 38); // red

                var item = new BarItem(r.Score) { Color = color };
                columnSeries.Items.Add(item);
            }

            // For horizontal bars, swap axes: category on left, value at bottom
            var categoryAxisLeft = new CategoryAxis { Position = AxisPosition.Left };
            foreach (var lbl in categoryAxis.Labels) categoryAxisLeft.Labels.Add(lbl);
            var valueAxisBottom = new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = 10, Title = "Score (0-10)" };

            model.Axes.Add(categoryAxisLeft);
            model.Axes.Add(valueAxisBottom);
            model.Series.Add(columnSeries);

            return model;
        }

        private void BuildKPIs(IReadOnlyCollection<RegistroBenchmark> history)
        {
            if (history.Count == 0)
            {
                KpiCurrentScore = "—";
                KpiDegradation = "—";
                KpiWorstComponent = "—";
                KpiConsistency = "—";
                return;
            }

            var ordered = history.OrderBy(h => h.Timestamp).ToList();

            // Current score: last run
            var lastRun = ordered.Last();
            KpiCurrentScore = $"{lastRun.Score:F1} / 10";

            // Degradation: first vs last
            var firstRun = ordered.First();
            float degradationPercent = firstRun.Score > 0 ? ((lastRun.Score - firstRun.Score) / firstRun.Score) * 100 : 0;
            KpiDegradation = $"{degradationPercent:+0.0;-0.0}%";

            // Worst component: which metric has largest variance or drop
            var cpuVar = ordered.Select(h => h.CpuLoad).DefaultIfEmpty(0).ToList();
            var gpuVar = ordered.Select(h => h.GpuLoad).DefaultIfEmpty(0).ToList();
            var ramVar = ordered.Select(h => h.RamLoad).DefaultIfEmpty(0).ToList();
            var diskVar = ordered.Select(h => h.DiskLoad).DefaultIfEmpty(0).ToList();

            var cpuSD = VarianceDouble(cpuVar);
            var gpuSD = VarianceDouble(gpuVar);
            var ramSD = VarianceDouble(ramVar);
            var diskSD = VarianceDouble(diskVar);

            var components = new[] { ("CPU", cpuSD), ("GPU", gpuSD), ("RAM", ramSD), ("Disk", diskSD) };
            KpiWorstComponent = components.OrderByDescending(x => x.Item2).First().Item1;

            // Consistency: inverse of coefficient of variation
            var scores = ordered.Select(h => (double)h.Score).ToList();
            var mean = scores.Average();
            var stdev = StandardDeviation(scores);
            double consistency = mean > 0 && stdev > 0 ? Math.Max(0, 100 - (stdev / mean) * 100) : 100;
            KpiConsistency = $"{consistency:F0}%";
        }

        private void BuildMiniCharts(IReadOnlyCollection<RegistroBenchmark> history)
        {
            if (history.Count == 0)
            {
                MiniCpuPlot = null;
                MiniGpuPlot = null;
                MiniRamPlot = null;
                MiniDiskPlot = null;
                return;
            }

            var ordered = history.OrderBy(h => h.Timestamp).ToList();

            var runLabels = ordered.Select(h => h.RunLabel ?? string.Empty).ToList();

            MiniCpuPlot = BuildMiniPlot("CPU %", ordered.Select(h => (double)h.CpuLoad).ToList(), runLabels, OxyColors.DarkCyan, OxyColor.FromArgb(30, 0, 105, 111));
            MiniGpuPlot = BuildMiniPlot("GPU %", ordered.Select(h => (double)h.GpuLoad).ToList(), runLabels, OxyColors.Orange, OxyColor.FromArgb(40, 218, 113, 1));
            MiniRamPlot = BuildMiniPlot("RAM %", ordered.Select(h => (double)h.RamLoad).ToList(), runLabels, OxyColors.Red, OxyColor.FromArgb(30, 161, 53, 68));
            MiniDiskPlot = BuildMiniPlot("Disk %", ordered.Select(h => (double)h.DiskLoad).ToList(), runLabels, OxyColors.Gold, OxyColor.FromArgb(40, 218, 113, 1));
        }

        private PlotModel BuildMiniPlot(string title, List<double> values, List<string> runLabels, OxyColor lineColor, OxyColor fillColor)
        {
            var model = new PlotModel { Title = title, TitleFontSize = 12 };
            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = true,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                GapWidth = 0.2,
                IsTickCentered = true,
                TextColor = OxyColors.Gray,
                TickStyle = TickStyle.None,
            };
            categoryAxis.Labels.AddRange(runLabels);
            model.Axes.Add(categoryAxis);
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, IsAxisVisible = false, IsPanEnabled = false, IsZoomEnabled = false });

            var series = new LineSeries
            {
                Color = lineColor,
                StrokeThickness = 2,
                DataFieldX = null,
                DataFieldY = null,
                TrackerFormatString = "{0}\nValor: {4:F1}%",
                CanTrackerInterpolatePoints = true
            };

            for (int i = 0; i < values.Count; i++)
            {
                series.Points.Add(new DataPoint(i, values[i]));
            }

            model.Series.Add(series);
            return model;
        }

        private void BuildHeatmap(IReadOnlyCollection<RegistroBenchmark> history)
        {
            HeatmapData.Clear();

            if (history.Count == 0) return;

            var ordered = history
                .OrderByDescending(h => h.Timestamp)
                .Take(10)
                .OrderBy(h => h.Timestamp)
                .ToList();

            var cpuValues = ordered.Select(h => (double)h.CpuLoad).ToList();
            var gpuValues = ordered.Select(h => (double)h.GpuLoad).ToList();
            var ramValues = ordered.Select(h => (double)h.RamLoad).ToList();
            var diskValues = ordered.Select(h => (double)h.DiskLoad).ToList();

            HeatmapData.Add(("CPU", cpuValues));
            HeatmapData.Add(("GPU", gpuValues));
            HeatmapData.Add(("RAM", ramValues));
            HeatmapData.Add(("Disk", diskValues));
        }

        private static double StandardDeviation(IReadOnlyList<double> values)
        {
            if (values.Count < 2) return 0;
            double mean = values.Average();
            double sumSquaredDiff = values.Sum(x => Math.Pow(x - mean, 2));
            return Math.Sqrt(sumSquaredDiff / values.Count);
        }

        private static double VarianceDouble(IReadOnlyList<float> values)
        {
            if (values.Count < 2) return 0;
            var doubleValues = values.Select(v => (double)v).ToList();
            return StandardDeviation(doubleValues);
        }
    }
}

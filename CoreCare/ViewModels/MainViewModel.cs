using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreCare.Models;
using CoreCare.Services;
using CoreCare.Data;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Linq;
using System.Windows.Media;

namespace CoreCare.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly HardwareMonitorService _hardwareService;
        private readonly ProcessService _processService;

        [ObservableProperty]
        private string _cpuDisplay;

        [ObservableProperty]
        private DateTime _historyFrom = DateTime.Today.AddDays(-30);

        [ObservableProperty]
        private DateTime _historyTo = DateTime.Today;

        [ObservableProperty]
        private string _historyStatus = "Sin datos cargados.";

        [ObservableProperty]
        private string _degradationStatus = "Analisis de degradacion pendiente.";

        [ObservableProperty]
        private string _trendStatus = "Tendencia pendiente.";

        [ObservableProperty]
        private Brush _degradationBrush = Brushes.DimGray;

        public ObservableCollection<ProcessItem> Processes { get; set; } = new();
        public ObservableCollection<RegistroBenchmark> HistoryItems { get; } = new();

        public MainViewModel()
        {
            _hardwareService = new HardwareMonitorService();
            _processService = new ProcessService();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, e) => UpdateAllData();
            timer.Start();

            UpdateAllData();
            LoadHistory();
        }

        private void UpdateAllData()
        {
            // SOLUCIÓN 2: Obligatorio pedirle a la placa base que lea los sensores en este milisegundo
            _hardwareService.UpdateHardware();

            // SOLUCIÓN 1: Obtenemos el float, lo redondeamos y lo convertimos a string con el símbolo "%"
            float load = _hardwareService.GetCpuLoad();
            CpuDisplay = $"{Math.Round(load, 1)} %";

            var list = _processService.GetActiveProcesses();

            Processes.Clear();
            foreach (var item in list)
            {
                Processes.Add(item);
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
                int userId = GetOrCreateSystemUserId(db);

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

                HistoryStatus = $"Usuario {userId}: {history.Count} registros en el rango {from:yyyy-MM-dd} a {to:yyyy-MM-dd}.";

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
                DegradationStatus = "No se pudo calcular degradacion.";
                TrendStatus = "No se pudo calcular tendencia.";
                DegradationBrush = Brushes.Firebrick;
            }
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

            return $"Tendencia score: {first.Score:F1} -> {last.Score:F1} ({Math.Abs(scoreDelta):F1}, {direction}).";
        }

        private static int GetOrCreateSystemUserId(CoreCareDbContext db)
        {
            var existingUser = db.Users.FirstOrDefault(user => user.IsActive);

            if (existingUser != null)
            {
                return existingUser.Id;
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

            return systemUser.Id;
        }
    }
}
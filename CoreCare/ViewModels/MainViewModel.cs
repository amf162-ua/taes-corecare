using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreCare.Models;
using CoreCare.Services;
using CoreCare.Views;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using QuestPDF.Fluent;

namespace CoreCare.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly HardwareMonitorService _hardwareService;
        private readonly ProcessService _processService;
        private readonly GeminiAIService _geminiService;
        private readonly SystemSpecsService _systemSpecsService;

        [ObservableProperty]
        private string _cpuDisplay;

        [ObservableProperty]
        private bool _isBusy;

        private System.Windows.Threading.Dispatcher _dispatcher;

        public ObservableCollection<ProcessItem> Processes { get; set; } = new();

        public MainViewModel()
        {
            _dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            _hardwareService = new HardwareMonitorService();
            _processService = new ProcessService();
            _geminiService = new GeminiAIService();
            _systemSpecsService = new SystemSpecsService();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, e) => UpdateAllData();
            timer.Start();

            UpdateAllData();
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
        public async Task GenerateReportAsync()
        {
            LoadingWindow? loadingWindow = null;
            try
            {
                loadingWindow = new LoadingWindow();
                loadingWindow.Show();

                loadingWindow.UpdateProgress(0, "Recopilando datos del sistema...");

                var hardwareTask = Task.Run(() =>
                {
                    _hardwareService.UpdateHardware();
                    _hardwareService.UpdateHardware();
                });
                var specsTask = Task.Run(() => _systemSpecsService.GetSystemSpecs());

                await Task.WhenAll(hardwareTask, specsTask);

                loadingWindow.UpdateProgress(20, "Analizando hardware...");

                var systemSpecs = specsTask.Result;
                float ramUsed = (float)Math.Round(_hardwareService.GetRamUsageGb(), 1);
                float ramAvailable = (float)Math.Round(_hardwareService.GetRamAvailableGb(), 1);
                float ramTotal = (float)Math.Round(ramUsed + ramAvailable, 1);

                var telemetryData = new SystemTelemetryMock
                {
                    CpuUsagePercent = (float)Math.Round(_hardwareService.GetCpuLoad(), 1, MidpointRounding.ToEven),
                    CpuTemperatureC = (float)Math.Round(_hardwareService.GetGpuTemperature(), 1, MidpointRounding.ToEven),
                    RamTotalGb = ramTotal,
                    RamUsedGb = ramUsed,
                    DiskType = systemSpecs.DiskModel,
                    DiskUsagePercent = 0
                };

                loadingWindow.UpdateProgress(40, "Generando recomendaciones con IA...");

                var aiResponse = await _geminiService.GetRecommendationsAsync(telemetryData, systemSpecs);

                var recommendations = new System.Collections.Generic.List<string>();

                if (aiResponse.StartsWith("Error:"))
                {
                    recommendations.Add("Error al conectar con IA. Intenta más tarde.");
                    MessageBox.Show("No se pudieron obtener recomendaciones de IA.\nEl servicio está temporalmente no disponible.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    recommendations.Add(aiResponse);
                }

                loadingWindow.UpdateProgress(80, "Generando informe PDF...");

                var data = new ReportData
                {
                    CompanyName = "TechRepairs S.L.",
                    ClientName = "Jesús Pérez",
                    ReportDate = DateTime.Now,
                    SystemSpecs = systemSpecs,
                    Recommendations = recommendations,
                    TelemetryData = telemetryData
                };

                var document = new ReportDocument(data);
                loadingWindow.UpdateProgress(100, "¡Completado!");
                loadingWindow.Close();

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Informe_CoreCare_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".pdf",
                    Filter = "PDF files (*.pdf)|*.pdf"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    document.GeneratePdf(saveDialog.FileName);
                    MessageBox.Show($"¡Informe generado con éxito!\nGuardado en: {saveDialog.FileName}", "PDF Generado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                loadingWindow?.Close();
            }
        }
    }
}

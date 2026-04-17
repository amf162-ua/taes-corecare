using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CoreCare.Models;
using CoreCare.Services;
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

        public ObservableCollection<ProcessItem> Processes { get; set; } = new();

        public MainViewModel()
        {
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
            try
            {
                _hardwareService.UpdateHardware();
                _hardwareService.UpdateHardware();

                float cpuLoad = (float)Math.Round(_hardwareService.GetCpuLoad(), 1, MidpointRounding.ToEven);
                float gpuLoad = (float)Math.Round(_hardwareService.GetGpuLoad(), 1, MidpointRounding.ToEven);
                float gpuTemp = (float)Math.Round(_hardwareService.GetGpuTemperature(), 1, MidpointRounding.ToEven);
                float ramUsed = (float)Math.Round(_hardwareService.GetRamUsageGb(), 1);
                float ramAvailable = (float)Math.Round(_hardwareService.GetRamAvailableGb(), 1);
                float ramTotal = (float)Math.Round(ramUsed + ramAvailable, 1);

                var telemetryData = new SystemTelemetryMock
                {
                    CpuUsagePercent = cpuLoad,
                    CpuTemperatureC = gpuTemp,
                    RamTotalGb = ramTotal,
                    RamUsedGb = ramUsed,
                    DiskType = "[PLACEHOLDER]",
                    DiskUsagePercent = gpuLoad
                };

                var aiResponse = await _geminiService.GetRecommendationsAsync(telemetryData);

                var recommendations = new System.Collections.Generic.List<string> { aiResponse };

                var systemSpecs = _systemSpecsService.GetSystemSpecs();
                systemSpecs.RamTotalGb = ramTotal;

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

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = System.IO.Path.Combine(desktopPath, $"Informe_CoreCare_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                document.GeneratePdf(filePath);

                MessageBox.Show($"¡Informe generado con éxito!\nGuardado en: {filePath}", "PDF Generado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

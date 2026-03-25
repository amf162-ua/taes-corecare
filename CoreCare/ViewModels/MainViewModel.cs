using System; // Añadido para poder usar TimeSpan
using CommunityToolkit.Mvvm.ComponentModel;
using CoreCare.Models;
using CoreCare.Services;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace CoreCare.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly HardwareMonitorService _hardwareService;
        private readonly ProcessService _processService;

        [ObservableProperty]
        private string _cpuDisplay;

        // Esta lista es la que "leerá" el DataGrid en el XAML
        public ObservableCollection<ProcessItem> Processes { get; set; } = new();

        public MainViewModel()
        {
            _hardwareService = new HardwareMonitorService();
            _processService = new ProcessService();

            // Actualizamos cada 2 segundos para no estresar el PC
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (s, e) => UpdateAllData();
            timer.Start();

            UpdateAllData(); // Primera carga al abrir
        }

        private void UpdateAllData()
        {
            // 1. Actualizamos el texto de la CPU
            CpuDisplay = _hardwareService.GetCpuLoad();

            // 2. Obtenemos los procesos del servicio
            var list = _processService.GetActiveProcesses();

            // 3. Refrescamos la colección de la pantalla
            Processes.Clear();
            foreach (var item in list)
            {
                Processes.Add(item);
            }
        }
    }
}


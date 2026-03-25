using System; // Añadido para poder usar TimeSpan
using CommunityToolkit.Mvvm.ComponentModel;
using CoreCare.Models;
using CoreCare.Services;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using System.Windows; // Para el MessageBox

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

        [RelayCommand]
        public void TerminateProcess(ProcessItem process)
        {
            if (process == null) return;

            // Confirmación de seguridad
            var result = MessageBox.Show(
                $"¿Seguro que quieres cerrar {process.Name}?\nSe perderán los datos no guardados.",
                "Confirmar acción",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                bool ok = _processService.TerminateProcess(process.Id);
                if (ok)
                {
                    UpdateAllData(); // Refrescamos la lista inmediatamente
                }
                else
                {
                    MessageBox.Show("No se pudo cerrar. Puede que no tengas permisos o el proceso ya haya terminado.");
                }
            }
        }
    }
}


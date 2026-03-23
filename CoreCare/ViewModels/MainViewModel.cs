using System; // Añadido para poder usar TimeSpan
using CommunityToolkit.Mvvm.ComponentModel;
using CoreCare.Services;
using System.Windows.Threading;

namespace CoreCare.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly HardwareMonitorService _hardwareService;

        [ObservableProperty]
        private string _cpuDisplay;

        public MainViewModel()
        {
            _hardwareService = new HardwareMonitorService();

            // Un temporizador para actualizar la CPU cada segundo
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => {
                // 1. Refrescamos los sensores físicos primero
                _hardwareService.UpdateHardware();

                // 2. Extraemos el float y lo convertimos a string con 1 decimal (F1) y el símbolo %
                CpuDisplay = $"{_hardwareService.GetCpuLoad():F1} %";
            };
            timer.Start();
        }
    }
}
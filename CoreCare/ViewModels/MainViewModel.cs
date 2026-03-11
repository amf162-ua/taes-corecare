using CommunityToolkit.Mvvm.ComponentModel;
using CoreCare.Services;
using System.Windows.Threading;

namespace CoreCare.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly HardwareService _hardwareService;

        [ObservableProperty]
        private string _cpuDisplay;

        public MainViewModel()
        {
            _hardwareService = new HardwareService();

            // Un temporizador para actualizar la CPU cada segundo
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => {
                CpuDisplay = _hardwareService.GetCpuLoad();
            };
            timer.Start();
        }
    }
}

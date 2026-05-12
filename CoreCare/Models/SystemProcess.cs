using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace CoreCare.Models
{
    public class SystemProcess : INotifyPropertyChanged
    {
        private string _status;
        private double _cpuUsage;
        private double _ramUsage;

        public string Id { get; set; }
        public string Name { get; set; }
        public int Pid { get; set; }
        public bool IsProtected { get; set; }

        public double CpuUsage
        {
            get => _cpuUsage;
            set { _cpuUsage = value; OnPropertyChanged(nameof(CpuUsage)); OnPropertyChanged(nameof(CpuColorBrush)); }
        }

        public double RamUsage
        {
            get => _ramUsage;
            set { _ramUsage = value; OnPropertyChanged(nameof(RamUsage)); OnPropertyChanged(nameof(RamProgress)); }
        }

        // Progreso de RAM basado en un máximo de 8GB
        public double RamProgress => (RamUsage / 8.0) * 100;

        // "running", "critical", "stopping", "stopped", "failed"
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(BorderColor));
                OnPropertyChanged(nameof(ButtonText));
                OnPropertyChanged(nameof(IsButtonEnabled));
                OnPropertyChanged(nameof(CriticalWarningVis));
                OnPropertyChanged(nameof(FailedWarningVis));
                OnPropertyChanged(nameof(ProtectedVis));
            }
        }

        // --- LÓGICA VISUAL DIRECTAMENTE EN EL MODELO PARA EVITAR ERRORES XAML ---

        public Visibility ProtectedVis => IsProtected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CriticalWarningVis => Status == "critical" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility FailedWarningVis => Status == "failed" ? Visibility.Visible : Visibility.Collapsed;

        public bool IsButtonEnabled => Status != "stopping" && Status != "stopped";

        public string ButtonText => Status == "stopping" ? "DETENIENDO..." :
                                    Status == "stopped" ? "DETENIDO" : "FRENAR";

        public SolidColorBrush BorderColor
        {
            get
            {
                if (Status == "stopping") return new SolidColorBrush(Colors.Gray);
                if (Status == "failed") return new SolidColorBrush(Colors.Yellow);
                if (Status == "critical") return new SolidColorBrush(Colors.Red);
                if (CpuUsage > 50) return new SolidColorBrush(Colors.Orange);
                return new SolidColorBrush(Color.FromRgb(0, 255, 255)); // Cyan
            }
        }

        public SolidColorBrush CpuColorBrush => CpuUsage > 70 ? new SolidColorBrush(Colors.Red) :
                                                CpuUsage > 50 ? new SolidColorBrush(Colors.Orange) :
                                                new SolidColorBrush(Colors.LightGreen);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
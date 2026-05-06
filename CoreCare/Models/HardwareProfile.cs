using System.ComponentModel;

namespace CoreCare.Models
{
    public class HardwareProfile : INotifyPropertyChanged
    {
        private string _cpu = "No especificado";
        private string _gpu = "No especificado";
        private string _ram = "No especificado";
        private string _storage = "No especificado";
        private string _motherboard = "No especificado";

        public string Cpu { get => _cpu; set { _cpu = value; OnPropertyChanged(nameof(Cpu)); } }
        public string Gpu { get => _gpu; set { _gpu = value; OnPropertyChanged(nameof(Gpu)); } }
        public string Ram { get => _ram; set { _ram = value; OnPropertyChanged(nameof(Ram)); } }
        public string Storage { get => _storage; set { _storage = value; OnPropertyChanged(nameof(Storage)); } }
        public string Motherboard { get => _motherboard; set { _motherboard = value; OnPropertyChanged(nameof(Motherboard)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
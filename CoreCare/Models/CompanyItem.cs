using System.ComponentModel;
using System.Windows.Media;

namespace CoreCare.Models
{
    public class CompanyItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(BorderColor));
                OnPropertyChanged(nameof(BgColor));
                OnPropertyChanged(nameof(TextColor));
            }
        }

        // Lógica visual para cuando está seleccionado
        public SolidColorBrush BorderColor => IsSelected ? new SolidColorBrush(Color.FromRgb(0, 255, 255)) : new SolidColorBrush(Color.FromArgb(50, 0, 255, 255));
        public SolidColorBrush BgColor => IsSelected ? new SolidColorBrush(Color.FromArgb(30, 0, 255, 255)) : new SolidColorBrush(Color.FromRgb(21, 26, 46));
        public SolidColorBrush TextColor => IsSelected ? new SolidColorBrush(Color.FromRgb(0, 255, 255)) : new SolidColorBrush(Colors.White);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
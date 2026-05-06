using System.Windows;
using CoreCare.Models;

namespace CoreCare
{
    public partial class BenchmarkDetailsWindow : Window
    {
        public BenchmarkDetailsWindow(RegistroBenchmark registro)
        {
            InitializeComponent();
            DataContext = registro;
        }
    }
}

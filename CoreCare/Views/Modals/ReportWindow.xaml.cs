using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CoreCare.Models;

namespace CoreCare.Views.Modals
{
    public partial class ReportWindow : Window
    {
        private ObservableCollection<CompanyItem> _companies;
        private string _selectedCompanyId = null;

        public ReportWindow(string benchmarkName)
        {
            InitializeComponent();
            TxtBenchmarkName.Text = $"Benchmark: {benchmarkName}";
            LoadCompanies();
        }

        private void LoadCompanies()
        {
            _companies = new ObservableCollection<CompanyItem>
            {
                new CompanyItem { Id = "1", Name = "AMD Support", Icon = "🔴", IsSelected = false },
                new CompanyItem { Id = "2", Name = "NVIDIA Support", Icon = "🟢", IsSelected = false },
                new CompanyItem { Id = "3", Name = "Intel Technical Team", Icon = "🔵", IsSelected = false },
                new CompanyItem { Id = "4", Name = "Microsoft Support", Icon = "🟦", IsSelected = false },
                new CompanyItem { Id = "5", Name = "CoreCare Premium", Icon = "⭐", IsSelected = false },
                new CompanyItem { Id = "6", Name = "Hardware Diagnostics", Icon = "🔧", IsSelected = false }
            };
            CompaniesList.ItemsSource = _companies;
        }

        private void Company_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var clickedCompany = btn?.DataContext as CompanyItem;

            if (clickedCompany != null)
            {
                // Desmarcar todas y marcar la seleccionada
                foreach (var company in _companies)
                {
                    company.IsSelected = (company.Id == clickedCompany.Id);
                }
                _selectedCompanyId = clickedCompany.Id;
                ValidateForm();
            }
        }

        private void Form_Changed(object sender, TextChangedEventArgs e)
        {
            ValidateForm();
        }

        private void ValidateForm()
        {
            // Se requiere que haya una empresa seleccionada y al menos 10 caracteres
            if (BtnSubmit != null)
            {
                BtnSubmit.IsEnabled = !string.IsNullOrWhiteSpace(TxtDescription.Text)
                                      && TxtDescription.Text.Trim().Length >= 10
                                      && _selectedCompanyId != null;
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Cambiar estado del botón
            BtnSubmit.IsEnabled = false;
            BtnSubmit.Content = "Enviando...";
            TxtDescription.IsEnabled = false;

            // Simular tiempo de subida a la red (1.5 segundos)
            await Task.Delay(1500);

            // Cambiar a la pantalla de éxito
            FormGrid.Visibility = Visibility.Collapsed;
            SuccessGrid.Visibility = Visibility.Visible;

            // Esperar 2 segundos para que el usuario lea el mensaje
            await Task.Delay(2000);

            // Cerrar el modal con éxito
            this.DialogResult = true;
            this.Close();
        }
    }
}
using System.Collections.Generic;
using System.Windows.Controls;
using CoreCare.Models;

namespace CoreCare.Views.Pages
{
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
            LoadReports();
        }

        private void LoadReports()
        {
            var mockReports = new List<Report>
            {
                new Report
                {
                    Id = "1",
                    BenchmarkName = "CPU Stress Test",
                    Description = "El sistema se sobrecalienta rápidamente al ejecutar este benchmark. La temperatura alcanza los 95°C en menos de 10 segundos.",
                    Company = "Intel Technical Team",
                    CompanyIcon = "🔵",
                    Status = "in-review",
                    Date = "18/03/2026 14:30",
                    TicketId = "INT-2026-4521"
                },
                new Report
                {
                    Id = "2",
                    BenchmarkName = "GPU Ray Tracing",
                    Description = "Artefactos visuales detectados durante la prueba de iluminación global. Posible fallo en los RT Cores.",
                    Company = "NVIDIA Support",
                    CompanyIcon = "🟢",
                    Status = "pending",
                    Date = "17/03/2026 10:15",
                    TicketId = "NV-9928-X"
                }
            };

            ReportsList.ItemsSource = mockReports;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
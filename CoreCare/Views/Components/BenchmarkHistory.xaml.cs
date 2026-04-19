using System;
using System.Collections.Generic;
using System.Windows.Controls;
using CoreCare.Models; // Tu modelo original con float y TimeSpan

namespace CoreCare.Views.Components
{
    public partial class BenchmarkHistory : UserControl
    {
        public BenchmarkHistory()
        {
            InitializeComponent();

            // Cargamos datos de prueba usando tu modelo BenchmarkResult tal cual
            ResultsList.ItemsSource = new List<BenchmarkResult>
            {
                new BenchmarkResult
                {
                    Score = 15400f,
                    PeakCpuTemp = 65f,
                    AvgCpuLoad = 40f,
                    Duration = TimeSpan.FromSeconds(30)
                },
                new BenchmarkResult
                {
                    Score = 7200f,
                    PeakCpuTemp = 85f,
                    AvgCpuLoad = 95f,
                    Duration = TimeSpan.FromSeconds(45)
                }
            };
        }
    }
}
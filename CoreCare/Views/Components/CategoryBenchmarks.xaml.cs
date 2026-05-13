using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CoreCare.Models;
using CoreCare.ViewModels;
using CoreCare.Services;

namespace CoreCare.Views.Components
{
    public partial class CategoryBenchmarks : UserControl
    {
        public CategoryBenchmarks()
        {
            InitializeComponent();
            LoadStaticData();
        }

        private const int DefaultDurationSeconds = 10;
        private const int FullDurationSeconds = 15;

        private void LoadStaticData()
        {
            var categories = new List<CategoryDefinition>
            {
                new CategoryDefinition
                {
                    Name = "CPU TEST",
                    Icon = "💻",
                    CategoryColor = "Orange",
                    Description = "Pruebas de rendimiento del procesador",
                    Tests = new List<TestDefinition>
                    {
                        CreateTest("1", "Carga y temperatura del procesador", DefaultDurationSeconds)
                    }
                },
                new CategoryDefinition
                {
                    Name = "GPU TEST",
                    Icon = "🎮",
                    CategoryColor = "Cyan",
                    Description = "Pruebas de rendimiento de tarjeta gráfica",
                    Tests = new List<TestDefinition>
                    {
                        CreateTest("2", "Carga y temperatura de la GPU", DefaultDurationSeconds)
                    }
                },
                new CategoryDefinition
                {
                    Name = "RAM TEST",
                    Icon = "🧠",
                    CategoryColor = "MediumSeaGreen",
                    Description = "Pruebas de uso y carga de memoria",
                    Tests = new List<TestDefinition>
                    {
                        CreateTest("3", "Carga de memoria RAM", DefaultDurationSeconds)
                    }
                },
                new CategoryDefinition
                {
                    Name = "DISCO TEST",
                    Icon = "💾",
                    CategoryColor = "PaleVioletRed",
                    Description = "Pruebas de rendimiento de disco",
                    Tests = new List<TestDefinition>
                    {
                        CreateTest("4", "Lectura, escritura y carga de disco", DefaultDurationSeconds)
                    }
                },
                new CategoryDefinition
                {
                    Name = "BENCHMARK COMPLETO",
                    Icon = "🧪",
                    CategoryColor = "Gold",
                    Description = "Benchmark completo de todos los componentes",
                    Tests = new List<TestDefinition>
                    {
                        CreateTest("5", "CPU, GPU, RAM y disco", FullDurationSeconds)
                    }
                }
            };
            CategoriesControl.ItemsSource = categories;
        }

        private static TestDefinition CreateTest(string code, string description, int durationSeconds)
        {
            var option = MainViewModel.DefaultBenchmarkOptions.First(o => o.Code == code);
            return new TestDefinition
            {
                Code = option.Code,
                Name = option.Label,
                Description = description,
                Duration = durationSeconds,
                Options = MapBenchmarkOption(option.Code)
            };
        }

        private static ScanOptions MapBenchmarkOption(string code)
        {
            return code switch
            {
                "1" => ScanOptions.ScanCPUOnly(),
                "2" => ScanOptions.ScanGPUOnly(),
                "3" => ScanOptions.ScanRAMOnly(),
                "4" => ScanOptions.ScanDiskOnly(),
                _ => ScanOptions.FullScan()
            };
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var stackPanel = btn?.Parent as StackPanel; // Según el XAML, el padre es el StackPanel[cite: 11]
            if (stackPanel != null && stackPanel.Children.Count > 1)
            {
                var testsList = stackPanel.Children[1] as ItemsControl;
                if (testsList != null)
                {
                    testsList.Visibility = testsList.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }

        private void RunTest_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var testInfo = btn?.DataContext as TestDefinition;
            if (testInfo == null) return;

            if (!SessionService.IsAuthenticated)
            {
                MessageBox.Show("Debes iniciar sesión para ejecutar benchmarks.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 1. Instanciar la ventana de progreso
            // Le pasamos si el usuario es premium (App.IsPremium)[cite: 6, 9]
            var progressWin = new Modals.BenchmarkProgressWindow(testInfo.Name, testInfo.Options, testInfo.Duration);
            progressWin.Owner = Window.GetWindow(this);

            // 2. ESTA LÍNEA HACE QUE LA VENTANA SE VEA EN PANTALLA[cite: 10, 12]
            // ShowDialog detiene el código aquí hasta que la ventana se cierre.
            if (progressWin.ShowDialog() == true)
            {
                // Al terminar, mostramos el resultado final generado[cite: 12]
                var resultado = progressWin.FinalResult;
                MessageBox.Show($"¡{testInfo.Name} completado!\nScore Final: {resultado.Score:F2}", "Éxito");
            }
        }
    }

    // Clases auxiliares dentro del namespace para evitar errores de compilación
    public class CategoryDefinition
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public string CategoryColor { get; set; }
        public List<TestDefinition> Tests { get; set; }
    }

    public class TestDefinition
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public ScanOptions Options { get; set; } = ScanOptions.FullScan();
        public string EstimatedTime => $"{Duration}s";
    }
}
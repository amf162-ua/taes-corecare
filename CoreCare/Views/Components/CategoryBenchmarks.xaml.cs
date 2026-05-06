using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CoreCare.Models;

namespace CoreCare.Views.Components
{
    public partial class CategoryBenchmarks : UserControl
    {
        public CategoryBenchmarks()
        {
            InitializeComponent();
            LoadStaticData();
        }

        private void LoadStaticData()
        {
            var categories = new List<CategoryDefinition>
            {
                new CategoryDefinition {
                    Name = "CPU TEST", Icon = "💻", CategoryColor = "Orange",
                    Description = "Pruebas de rendimiento del procesador",
                    Tests = new List<TestDefinition> {
                        new TestDefinition { Name = "CPU Básico", Description = "Test rápido de 10 segundos", Duration = 10 },
                        new TestDefinition { Name = "CPU Avanzado", Description = "Test profundo de 30 segundos", Duration = 30 }
                    }
                },
                new CategoryDefinition {
                    Name = "GPU TEST", Icon = "🎮", CategoryColor = "Cyan",
                    Description = "Pruebas de tarjeta gráfica",
                    Tests = new List<TestDefinition> {
                        new TestDefinition { Name = "GPU Básico", Description = "Rendimiento estándar", Duration = 10 }
                    }
                }
            };
            CategoriesControl.ItemsSource = categories;
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

            // 1. Instanciar la ventana de progreso
            // Le pasamos si el usuario es premium (App.IsPremium)[cite: 6, 9]
            var progressWin = new Modals.BenchmarkProgressWindow(testInfo.Name, App.IsPremium);
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
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
    }
}
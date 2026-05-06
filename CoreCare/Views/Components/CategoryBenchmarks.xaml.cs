using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CoreCare.Models; // Aquí está tu modelo BenchmarkResult

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
                        new TestDefinition { Name = "CPU Básico", Description = "Evaluación rápida", EstimatedTime = "~2 min" },
                        new TestDefinition { Name = "CPU Avanzado", Description = "Análisis multihilo", EstimatedTime = "~10 min" }
                    }
                },
                new CategoryDefinition {
                    Name = "GPU TEST", Icon = "🎮", CategoryColor = "Cyan",
                    Description = "Pruebas de tarjeta gráfica",
                    Tests = new List<TestDefinition> {
                        new TestDefinition { Name = "GPU Básico", Description = "Rendimiento 1080p", EstimatedTime = "~3 min" },
                        new TestDefinition { Name = "GPU Avanzado", Description = "Ray Tracing Test", EstimatedTime = "~15 min" }
                    }
                }
            };
            CategoriesControl.ItemsSource = categories;
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var parent = btn.Parent as StackPanel;
            var testsList = parent.Children[1] as ItemsControl;
            testsList.Visibility = testsList.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void RunTest_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var testInfo = btn.DataContext as TestDefinition;

            // Al ejecutar, creamos el objeto siguiendo TU MODELO
            var progressWin = new Modals.BenchmarkProgressWindow(testInfo.Name, true);

            if (progressWin.ShowDialog() == true)
            {
                // El resultado final que devuelve la ventana ya usa tus floats y TimeSpan
                BenchmarkResult resultado = progressWin.FinalResult;

                MessageBox.Show($"Test {testInfo.Name} completado.\nScore: {resultado.Score}", "Éxito");
            }
        }
    }

    // Clases internas para organizar la vista (no persisten, solo para el UI)
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
        public string EstimatedTime { get; set; }
    }
}
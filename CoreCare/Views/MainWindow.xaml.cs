using CoreCare.ViewModels;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using System.Linq;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoreCare
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            this.DataContext = vm;

            Loaded += (_, _) => Dispatcher.BeginInvoke(new Action(DrawHeatmap), System.Windows.Threading.DispatcherPriority.Loaded);

            // Subscribe to VM property changes to redraw heatmap
            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.MiniCpuPlot))
                {
                    Dispatcher.BeginInvoke(new Action(DrawHeatmap), System.Windows.Threading.DispatcherPriority.Background);
                }
            };
        }

        private void TrendPlot_MouseLeave(object sender, MouseEventArgs e)
        {
            HoverPopup.IsOpen = false;
        }

        private void ScorePlot_MouseLeave(object sender, MouseEventArgs e)
        {
            HoverPopup.IsOpen = false;
        }

        public void DrawHeatmap()
        {
            if (this.DataContext is not MainViewModel vm) return;
            if (vm.HeatmapData == null || vm.HeatmapData.Count == 0) return;

            HeatmapCanvas.Children.Clear();

            const double cellWidth = 40;
            const double cellHeight = 36;
            const double labelWidth = 60;
            const double rowLabelHeight = 22;

            var metrics = vm.HeatmapData;
            int numRuns = metrics.FirstOrDefault().Values.Count;

            // Draw column headers (run labels)
            for (int col = 0; col < numRuns; col++)
            {
                var text = new TextBlock { Text = $"R{col + 1}", FontSize = 9, Foreground = Brushes.Gray, TextAlignment = TextAlignment.Center, Width = cellWidth - 2 };
                Canvas.SetLeft(text, labelWidth + col * cellWidth + 2);
                Canvas.SetTop(text, 2);
                HeatmapCanvas.Children.Add(text);
            }

            // Draw heatmap cells
            for (int row = 0; row < metrics.Count; row++)
            {
                var metricRow = metrics[row];
                var values = metricRow.Values;
                double maxValue = values.Count > 0 ? values.Max() : 1;

                // Row label
                var label = new TextBlock { Text = metricRow.Name, FontSize = 11, Foreground = Brushes.Gray, TextAlignment = TextAlignment.Right };
                Canvas.SetLeft(label, 4);
                Canvas.SetTop(label, rowLabelHeight + row * cellHeight + 8);
                HeatmapCanvas.Children.Add(label);

                // Cells for each run
                for (int col = 0; col < values.Count; col++)
                {
                    double value = values[col];
                    double normalized = maxValue > 0 ? value / maxValue : 0;

                    // Red color gradient: light red -> medium red -> dark red
                    byte r, g, b;
                    if (normalized < 0.33)
                    {
                        // Light red (#FFE8E8)
                        r = (byte)(255 - normalized * 50);
                        g = (byte)(232 - normalized * 100);
                        b = (byte)(232 - normalized * 100);
                    }
                    else if (normalized < 0.66)
                    {
                        // Medium red (#FFCCCC)
                        r = (byte)(255 - (normalized - 0.33) * 30);
                        g = (byte)(204 - (normalized - 0.33) * 100);
                        b = (byte)(204 - (normalized - 0.33) * 100);
                    }
                    else
                    {
                        // Dark red (#CC0000)
                        r = (byte)(204 + (normalized - 0.66) * 50);
                        g = (byte)(0);
                        b = (byte)(0);
                    }

                    var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
                    var cell = new Rectangle
                    {
                        Width = cellWidth - 2,
                        Height = cellHeight - 2,
                        Fill = brush,
                        Stroke = Brushes.White,
                        StrokeThickness = 1,
                        RadiusX = 5,
                        RadiusY = 5
                    };
                    Canvas.SetLeft(cell, labelWidth + col * cellWidth);
                    Canvas.SetTop(cell, rowLabelHeight + row * cellHeight);
                    HeatmapCanvas.Children.Add(cell);

                    // Value text
                    var valueText = new TextBlock
                    {
                        Text = value.ToString("F1"),
                        FontSize = 8,
                        Width = cellWidth - 2,
                        Height = cellHeight - 2,
                        Foreground = normalized > 0.6 ? Brushes.White : Brushes.Black,
                        TextAlignment = TextAlignment.Center
                    };
                    Canvas.SetLeft(valueText, labelWidth + col * cellWidth);
                    Canvas.SetTop(valueText, rowLabelHeight + row * cellHeight + 9);
                    HeatmapCanvas.Children.Add(valueText);
                }
            }
        }
    }
}
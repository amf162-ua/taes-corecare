using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CoreCare.ViewModels;

namespace CoreCare.Views.Modals
{
    public partial class ReportBenchmarkSelectionWindow : Window
    {
        public ObservableCollection<ReportBenchmarkSelectionItem> BenchmarkItems { get; }

        public IReadOnlyList<ReportBenchmarkSelectionItem> SelectedBenchmarks { get; private set; } =
            Array.Empty<ReportBenchmarkSelectionItem>();

        public bool GenerateWithoutBenchmarks { get; private set; }

        public ReportBenchmarkSelectionWindow()
        {
            InitializeComponent();

            BenchmarkItems = new ObservableCollection<ReportBenchmarkSelectionItem>(
                MainViewModel.DefaultBenchmarkOptions.Select(option => new ReportBenchmarkSelectionItem
                {
                    Code = option.Code,
                    Label = option.Label,
                    DurationSeconds = option.Code == "5" ? 15 : 10
                }));

            DataContext = this;
            UpdateRunButtonState();
        }

        private void BenchmarkSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox { DataContext: ReportBenchmarkSelectionItem item } checkBox)
            {
                item.IsSelected = checkBox.IsChecked == true;
            }

            UpdateRunButtonState();
        }

        private void UpdateRunButtonState()
        {
            BtnRunSelected.IsEnabled = BenchmarkItems.Any(item => item.IsSelected);
        }

        private void RunSelected_Click(object sender, RoutedEventArgs e)
        {
            SelectedBenchmarks = BenchmarkItems
                .Where(item => item.IsSelected)
                .Select(item => item.Clone())
                .ToList();
            GenerateWithoutBenchmarks = false;
            DialogResult = true;
            Close();
        }

        private void GenerateWithoutBenchmarks_Click(object sender, RoutedEventArgs e)
        {
            SelectedBenchmarks = Array.Empty<ReportBenchmarkSelectionItem>();
            GenerateWithoutBenchmarks = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public sealed class ReportBenchmarkSelectionItem
    {
        public required string Code { get; set; }
        public required string Label { get; set; }
        public int DurationSeconds { get; set; }
        public bool IsSelected { get; set; }
        public string EstimatedTime => $"Duracion estimada: {DurationSeconds}s";

        public ReportBenchmarkSelectionItem Clone()
        {
            return new ReportBenchmarkSelectionItem
            {
                Code = Code,
                Label = Label,
                DurationSeconds = DurationSeconds,
                IsSelected = IsSelected
            };
        }
    }
}

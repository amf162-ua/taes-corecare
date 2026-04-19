using System;
using System.Windows;
using System.Windows.Threading;
using CoreCare.Models;

namespace CoreCare.Views.Modals
{
    public partial class BenchmarkProgressWindow : Window
    {
        private DispatcherTimer _timer;
        private int _currentStep = 0;
        private const int TotalSteps = 100; // 10 segundos (100ms * 100)
        private Random _rnd = new Random();

        public BenchmarkResult FinalResult { get; private set; }
        public bool IsPremium { get; set; }

        public BenchmarkProgressWindow(string benchmarkName, bool premium)
        {
            InitializeComponent();
            TxtBenchmarkName.Text = benchmarkName.ToUpper();
            IsPremium = premium;

            // Configurar vista según plan
            if (IsPremium)
            {
                BasicView.Visibility = Visibility.Collapsed;
                PremiumView.Visibility = Visibility.Visible;
            }

            // Configurar el Timer (Equivalente al setInterval de React)
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _currentStep++;
            double progress = (_currentStep / (double)TotalSteps) * 100;

            // Actualizar UI Básica
            ProgressBar.Value = progress;
            TxtPercent.Text = $"{(int)progress}%";

            // Simular métricas si es Premium
            if (IsPremium)
            {
                TxtCpu.Text = $"{_rnd.Next(20, 90)}%";
                TxtRam.Text = $"{_rnd.Next(30, 80)}%";
                TxtTemp.Text = $"{_rnd.Next(50, 85)}°C";
            }

            // Al finalizar (10 segundos)
            if (_currentStep >= TotalSteps)
            {
                _timer.Stop();
                GenerarResultado();
                this.DialogResult = true; // Cierra la ventana y avisa que terminó
            }
        }

        private void GenerarResultado()
        {
            // Creamos el resultado con los campos que pedía tu TSX
            FinalResult = new BenchmarkResult
            {
                Score = (float)(5000 + _rnd.NextDouble() * 5000),
                PeakCpuTemp = (float)(70 + _rnd.Next(0, 30)),
                PeakGpuTemp = (float)(80 + _rnd.Next(0, 20)),
                AvgCpuLoad = (float)(50 + _rnd.Next(0, 30)),
                AvgRamLoad = (float)(40 + _rnd.Next(0, 20)),
                Duration = TimeSpan.FromSeconds(10)
            };
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            this.DialogResult = false;
            this.Close();
        }
    }
}
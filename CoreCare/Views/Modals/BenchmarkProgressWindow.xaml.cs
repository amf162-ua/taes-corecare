using System;
using System.Windows;
using System.Windows.Threading;
using CoreCare.Models;
using CoreCare.Services;
using CoreCare.Data;

namespace CoreCare.Views.Modals
{
    public partial class BenchmarkProgressWindow : Window
    {
        private DispatcherTimer _timer;
        private int _currentStep = 0;
        private const int TotalSteps = 100; // Simula 10 segundos
        private Random _rnd = new Random();

        public RegistroBenchmark FinalResult { get; private set; }

        public BenchmarkProgressWindow(string benchmarkName, bool isPremium)
        {
            InitializeComponent();
            TxtBenchmarkName.Text = benchmarkName.ToUpper();

            // Configurar el temporizador para la actualización visual
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _currentStep++;
            double progress = (_currentStep / (double)TotalSteps) * 100;

            ProgressBar.Value = progress;
            TxtPercent.Text = $"{(int)progress}%";

            // Simulación de métricas en tiempo real
            TxtCpu.Text = $"{_rnd.Next(20, 95)}%";
            TxtRam.Text = $"{_rnd.Next(30, 85)}%";
            TxtTemp.Text = $"{_rnd.Next(50, 80)}°C";

            if (_currentStep >= TotalSteps)
            {
                _timer.Stop();
                GuardarYFinalizar();
            }
        }

        private void GuardarYFinalizar()
        {
            try
            {
                // Conexión y servicio de historial
                var db = new CoreCareDbContext();
                var historyService = new BenchmarkHistoryService(db);

                // Crear el objeto de registro para la base de datos
                FinalResult = new RegistroBenchmark
                {
                    UserId = 1, // ID de usuario por defecto
                    Score = (float)(5.0 + _rnd.NextDouble() * 5.0),
                    CpuTemp = (float)_rnd.Next(65, 88),
                    GpuTemp = (float)_rnd.Next(60, 80),
                    RamLoad = (float)_rnd.Next(40, 90),
                    Timestamp = DateTime.Now
                };

                // Guardar en la base de datos
                historyService.SaveBenchmark(FinalResult);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar en la base de datos: " + ex.Message);
                this.DialogResult = false;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            this.DialogResult = false;
            this.Close();
        }
    }
}
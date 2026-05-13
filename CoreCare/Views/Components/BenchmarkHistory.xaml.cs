using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CoreCare.Models;
using CoreCare.Services;
using CoreCare.Data;

namespace CoreCare.Views.Components
{
    public partial class BenchmarkHistory : UserControl
    {
        public BenchmarkHistory()
        {
            InitializeComponent();
        }

        // Se ejecuta cada vez que la pestaña se carga en pantalla
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatosDesdeBD();
        }

        private void CargarDatosDesdeBD()
        {
            try
            {
                var currentUser = SessionService.CurrentUser;
                if (currentUser == null)
                {
                    ResultsList.ItemsSource = Array.Empty<RegistroBenchmark>();
                    return;
                }

                var db = new CoreCareDbContext();
                var service = new BenchmarkHistoryService(db);

                // Traer los últimos 50 registros del usuario actual
                var resultados = service.GetHistory(currentUser.Id, null, null, 50);
                foreach (var item in resultados)
                {
                    item.PerformancePlot = BuildPerformancePlot(item);
                }

                // Vincular la lista a la interfaz
                ResultsList.ItemsSource = resultados;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error cargando historial: " + ex.Message);
            }
        }

        private static OxyPlot.PlotModel BuildPerformancePlot(RegistroBenchmark item)
        {
            var model = new OxyPlot.PlotModel
            {
                Title = "Rendimiento",
                TitleFontSize = 11,
                Background = OxyPlot.OxyColors.Transparent,
                PlotAreaBackground = OxyPlot.OxyColors.Transparent,
                PlotAreaBorderColor = OxyPlot.OxyColors.Transparent,
                TextColor = OxyPlot.OxyColors.White
            };

            var timeAxis = new OxyPlot.Axes.DateTimeAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                StringFormat = "HH:mm:ss",
                IntervalType = OxyPlot.Axes.DateTimeIntervalType.Seconds,
                MajorGridlineStyle = OxyPlot.LineStyle.Solid,
                MinorGridlineStyle = OxyPlot.LineStyle.Dot,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                TextColor = OxyPlot.OxyColors.Gray
            };

            var valueAxis = new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                Minimum = 0,
                Maximum = 100,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MajorGridlineStyle = OxyPlot.LineStyle.Solid,
                MinorGridlineStyle = OxyPlot.LineStyle.Dot,
                TextColor = OxyPlot.OxyColors.Gray
            };

            model.Axes.Add(timeAxis);
            model.Axes.Add(valueAxis);

            void AddSeries(ComponentType component, string title, OxyPlot.OxyColor color)
            {
                var readings = item.SensorReadings
                    .Where(r => r.Component == component && r.Type == SensorType.Load)
                    .OrderBy(r => r.TimeStamp)
                    .ToList();

                if (readings.Count == 0)
                {
                    return;
                }

                var series = new OxyPlot.Series.LineSeries
                {
                    Title = title,
                    Color = color,
                    StrokeThickness = 2,
                    TrackerFormatString = "{0}\n{1:HH:mm:ss}: {2:0.0}%",
                    CanTrackerInterpolatePoints = true
                };

                foreach (var reading in readings)
                {
                    series.Points.Add(new OxyPlot.DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(reading.TimeStamp), reading.Value));
                }

                model.Series.Add(series);
            }

            AddSeries(ComponentType.Cpu, "CPU", OxyPlot.OxyColors.SkyBlue);
            AddSeries(ComponentType.Gpu, "GPU", OxyPlot.OxyColors.Orange);
            AddSeries(ComponentType.Ram, "RAM", OxyPlot.OxyColors.MediumSeaGreen);
            AddSeries(ComponentType.Disk, "Disco", OxyPlot.OxyColors.PaleVioletRed);

            return model;
        }
    }
}
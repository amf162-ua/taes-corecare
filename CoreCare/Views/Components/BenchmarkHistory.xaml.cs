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
                var db = new CoreCareDbContext();
                var service = new BenchmarkHistoryService(db);

                // Traer los últimos 50 registros del usuario con ID 1
                var resultados = service.GetHistory(1, null, null, 50);

                // Vincular la lista a la interfaz
                ResultsList.ItemsSource = resultados;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error cargando historial: " + ex.Message);
            }
        }
    }
}
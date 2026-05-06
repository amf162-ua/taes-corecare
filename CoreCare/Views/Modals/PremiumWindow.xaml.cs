using System.Collections.Generic;
using System.Windows;

namespace CoreCare.Views.Modals
{
    public partial class PremiumWindow : Window
    {
        public PremiumWindow()
        {
            InitializeComponent();
            LoadFeatures();
        }

        private void LoadFeatures()
        {
            // La misma lista de beneficios de tu archivo TSX
            var features = new List<string>
            {
                "Acceso a todos los benchmarks avanzados",
                "Suite completa de pruebas automatizadas",
                "Análisis térmico detallado",
                "Pruebas de estrés prolongadas",
                "Exportación de resultados en PDF",
                "Comparación con base de datos global",
                "Soporte técnico prioritario",
                "Actualizaciones anticipadas"
            };

            // Pasamos la lista al XAML
            FeaturesList.ItemsSource = features;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // Cierra la ventana sin realizar ninguna acción
            this.DialogResult = false;
            this.Close();
        }

        private void BtnUpgrade_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Esto es lo que activa el siguiente paso
            this.Close();
        }
    }
}
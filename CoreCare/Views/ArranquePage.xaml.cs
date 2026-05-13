using System.Windows.Controls;
using CoreCare.ViewModels; // IMPORTANTE AÑADIR ESTO

namespace CoreCare.Views
{
    public partial class ArranquePage : Page
    {
        public ArranquePage()
        {
            InitializeComponent();

            // Conectamos la vista con el ViewModel unificado
            this.DataContext = new MainViewModel();
        }
    }
}
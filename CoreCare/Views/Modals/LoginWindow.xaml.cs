using System.Windows;

namespace CoreCare.Views.Modals
{
    public partial class LoginWindow : Window
    {
        // Esta propiedad la mantenemos por si la necesitas, 
        // pero ahora usamos App.CurrentUsername para todo el programa
        public string UserName { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // Si cerramos sin loguear, el DialogResult es falso por defecto
            this.DialogResult = false;
            this.Close();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validamos que el campo no esté vacío
            if (!string.IsNullOrEmpty(TxtLoginEmail.Text))
            {
                // 2. Extraemos el nombre (lo que hay antes del @)
                UserName = TxtLoginEmail.Text.Split('@')[0];

                // 3. ACTUALIZAMOS EL ESTADO GLOBAL (Muy importante)
                // Esto es lo que permite que el Header cambie a modo "Perfil"
                App.IsUserLoggedIn = true;
                App.CurrentUsername = UserName;

                // 4. Cerramos con éxito
                // Establecer DialogResult en 'true' hace que el ShowDialog() 
                // del Header devuelva verdadero y ejecute 'ActualizarInterfaz()'
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Por favor, introduce un email válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
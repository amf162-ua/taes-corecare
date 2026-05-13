using System.Linq;
using System.Windows;
using CoreCare.Data;
using CoreCare.Models;

namespace CoreCare.Views.Modals
{
    public partial class LoginWindow : Window
    {
        // Controlamos que no se lancen múltiples procesos a la vez
        private bool _isNavigating = false;

        public LoginWindow()
        {
            InitializeComponent();
        }

        // Botón INICIAR SESIÓN[cite: 9, 10]
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorText.Text = "Introduce usuario y contraseña.";
                ErrorText.Visibility = Visibility.Visible;
                _isNavigating = false;
                return;
            }

            using var db = new CoreCareDbContext();
            var user = db.Users.FirstOrDefault(u =>
                u.username == username && u.password == password && u.IsActive);

            if (user == null)
            {
                ErrorText.Text = "Usuario o contraseña incorrectos.";
                ErrorText.Visibility = Visibility.Visible;
                _isNavigating = false;
                return;
            }

            // Guardamos el estado en la clase global App
            App.IsUserLoggedIn = true;
            App.CurrentUsername = user.username;
            App.IsPremium = user.Plan == TipoPlan.Premium;

            // CORRECCIÓN: Cerramos la ventana indicando éxito. 
            // Esto evita crear un "new MainWindow" duplicado[cite: 7, 9].
            this.DialogResult = true;
            this.Close();
        }

        // Botón CONTINUAR COMO INVITADO[cite: 9, 10]
        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            App.IsUserLoggedIn = false;
            App.CurrentUsername = "Invitado";
            App.IsPremium = false;

            // CORRECCIÓN: Cerramos devolviendo el control a la ventana principal[cite: 9].
            this.DialogResult = true;
            this.Close();
        }

        // Botón CREAR CUENTA (REGISTRO)[cite: 9, 10]
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            string nombre = RegNombreBox.Text.Trim();
            string email = RegEmailBox.Text.Trim();
            string password = RegPasswordBox.Password;
            string confirm = RegConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                RegErrorText.Text = "Todos los campos son obligatorios.";
                RegErrorText.Visibility = Visibility.Visible;
                _isNavigating = false;
                return;
            }

            if (password != confirm)
            {
                RegErrorText.Text = "Las contraseñas no coinciden.";
                RegErrorText.Visibility = Visibility.Visible;
                _isNavigating = false;
                return;
            }

            using var db = new CoreCareDbContext();

            if (db.Users.Any(u => u.username == email))
            {
                RegErrorText.Text = "El email ya está registrado.";
                RegErrorText.Visibility = Visibility.Visible;
                _isNavigating = false;
                return;
            }

            var newUser = new User
            {
                username = email,
                password = password,
                IsActive = true,
                Plan = TipoPlan.Basico
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            App.IsUserLoggedIn = true;
            App.CurrentUsername = newUser.username;
            App.IsPremium = false;

            this.DialogResult = true;
            this.Close();
        }

        // Si tienes un botón de cerrar (X) o Cancelar, asegúrate de tener este método:
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
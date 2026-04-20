using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace CoreCare.Views.Modals
{
    public partial class LoginWindow : Window
    {
        public string UserName { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Aquí simulas el handleLogin de tu archivo original
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtLoginEmail.Text))
            {
                UserName = TxtLoginEmail.Text.Split('@')[0];
                this.DialogResult = true; // Indica que el login fue exitoso
                this.Close();
            }
        }
    }
}
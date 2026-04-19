using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CoreCare.Views.Modals
{
    public partial class PaymentWindow : Window
    {
        private bool _isProcessing = false;

        public PaymentWindow()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (!_isProcessing)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        // Formatea el número de tarjeta (añade espacios cada 4 números)
        private void TxtCardNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            string unformatted = new string(TxtCardNumber.Text.Where(char.IsDigit).ToArray());
            if (unformatted.Length > 16) unformatted = unformatted.Substring(0, 16);

            string formatted = string.Join(" ", Enumerable.Range(0, unformatted.Length / 4 + (unformatted.Length % 4 == 0 ? 0 : 1))
                                                          .Select(i => unformatted.Substring(i * 4, Math.Min(4, unformatted.Length - i * 4))));

            if (TxtCardNumber.Text != formatted)
            {
                TxtCardNumber.Text = formatted;
                TxtCardNumber.CaretIndex = formatted.Length; // Mantener el cursor al final
            }
            ValidateForm(null, null);
        }

        // Formatea la fecha de expiración (Añade la barra MM/AA)
        private void TxtExpiry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string unformatted = new string(TxtExpiry.Text.Where(char.IsDigit).ToArray());
            if (unformatted.Length > 4) unformatted = unformatted.Substring(0, 4);

            string formatted = unformatted;
            if (unformatted.Length >= 2)
            {
                formatted = unformatted.Insert(2, "/");
            }

            if (TxtExpiry.Text != formatted)
            {
                TxtExpiry.Text = formatted;
                TxtExpiry.CaretIndex = formatted.Length;
            }
            ValidateForm(null, null);
        }

        // Comprueba si todos los campos están llenos para activar el botón
        private void ValidateForm(object sender, TextChangedEventArgs e)
        {
            if (BtnPay == null) return;

            string cardNumber = new string(TxtCardNumber.Text.Where(char.IsDigit).ToArray());
            string expiry = new string(TxtExpiry.Text.Where(char.IsDigit).ToArray());
            string cvv = new string(TxtCvv.Text.Where(char.IsDigit).ToArray());

            bool isValid = cardNumber.Length == 16 &&
                           !string.IsNullOrWhiteSpace(TxtCardName.Text) &&
                           expiry.Length == 4 &&
                           cvv.Length == 3;

            BtnPay.IsEnabled = isValid && !_isProcessing;
        }

        // Simula el procesamiento del pago (Async)
        private async void BtnPay_Click(object sender, RoutedEventArgs e)
        {
            _isProcessing = true;
            BtnPay.IsEnabled = false;
            BtnBack.IsEnabled = false;

            BtnPay.Content = "PROCESANDO PAGO...";
            BtnPay.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100)); // Gris

            // Simula 2 segundos de espera (procesamiento bancario)
            await Task.Delay(2000);

            // Cierra el modal e indica éxito
            this.DialogResult = true;
            this.Close();
        }
    }
}
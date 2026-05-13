using System;
using System.Text.RegularExpressions;

namespace CoreCare.Services
{
    /// <summary>
    /// Servicio centralizado para validaciones de input
    /// Proporciona métodos de validación reutilizables y consistentes
    /// </summary>
    public static class ValidationService
    {
        /// <summary>
        /// Valida un email
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.Length <= 254;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida un nombre de usuario
        /// </summary>
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Username debe tener entre 3 y 20 caracteres, solo letras, números y guiones bajos
            return Regex.IsMatch(username, @"^[a-zA-Z0-9_]{3,20}$");
        }

        /// <summary>
        /// Valida una contraseña
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidatePassword(string password, int minLength = 8)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "La contraseña no puede estar vacía.");

            if (password.Length < minLength)
                return (false, $"La contraseña debe tener al menos {minLength} caracteres.");

            // Validar que contiene números, mayúsculas y minúsculas (recomendado pero no obligatorio)
            bool hasUpperCase = Regex.IsMatch(password, "[A-Z]");
            bool hasLowerCase = Regex.IsMatch(password, "[a-z]");
            bool hasNumbers = Regex.IsMatch(password, "[0-9]");

            // Para esta aplicación, solo requerimos longitud mínima
            // Descomenta las líneas siguientes si quieres validación más estricta:
            /*
            if (!hasUpperCase || !hasLowerCase || !hasNumbers)
            {
                return (false, "La contraseña debe contener mayúsculas, minúsculas y números.");
            }
            */

            return (true, string.Empty);
        }

        /// <summary>
        /// Valida un nombre
        /// </summary>
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Nombre entre 2 y 100 caracteres, solo letras, espacios y caracteres comunes
            return name.Length >= 2 && name.Length <= 100 && 
                   Regex.IsMatch(name, @"^[a-zA-Z0-9\s\-\.áéíóúàèìòùäëïöüñ]+$");
        }

        /// <summary>
        /// Sanitiza un string para prevenir SQL injection (capa adicional)
        /// </summary>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remover caracteres especiales peligrosos
            input = input.Trim();
            input = Regex.Replace(input, @"[';""–—–]", ""); // Remover quotes peligrosas
            return input;
        }

        /// <summary>
        /// Valida que un input no sea nulo o vacío
        /// </summary>
        public static bool IsNotNullOrEmpty(string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        /// <summary>
        /// Valida que un número esté dentro de un rango
        /// </summary>
        public static bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Valida que una URL sea válida
        /// </summary>
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            try
            {
                _ = new Uri(url);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida que una cadena sea un número entero
        /// </summary>
        public static bool IsValidInteger(string value)
        {
            return int.TryParse(value, out _);
        }

        /// <summary>
        /// Valida que una cadena sea un número decimal
        /// </summary>
        public static bool IsValidDecimal(string value)
        {
            return decimal.TryParse(value, out _);
        }
    }
}

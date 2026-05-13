using Xunit;
using CoreCare.Services;
using CoreCare.Models;
using BCrypt.Net;

namespace CoreCare.Tests
{
    /// <summary>
    /// Tests unitarios para el servicio de autenticación
    /// </summary>
    public class AuthServiceTests
    {
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _authService = new AuthService();
            LoggingService.Initialize();
        }

        #region Tests de Validación

        [Fact]
        public void ValidatePassword_WithValidPassword_ReturnsTrue()
        {
            // Arrange
            string validPassword = "MySecurePassword123";

            // Act
            var (isValid, errorMessage) = ValidationService.ValidatePassword(validPassword, 8);

            // Assert
            Assert.True(isValid);
            Assert.Empty(errorMessage);
        }

        [Fact]
        public void ValidatePassword_WithShortPassword_ReturnsFalse()
        {
            // Arrange
            string shortPassword = "short";

            // Act
            var (isValid, errorMessage) = ValidationService.ValidatePassword(shortPassword, 8);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(errorMessage);
        }

        [Fact]
        public void ValidatePassword_WithEmptyPassword_ReturnsFalse()
        {
            // Arrange
            string emptyPassword = "";

            // Act
            var (isValid, errorMessage) = ValidationService.ValidatePassword(emptyPassword, 8);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(errorMessage);
        }

        [Fact]
        public void IsValidEmail_WithValidEmail_ReturnsTrue()
        {
            // Arrange
            string validEmail = "test@example.com";

            // Act
            bool result = ValidationService.IsValidEmail(validEmail);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidEmail_WithInvalidEmail_ReturnsFalse()
        {
            // Arrange
            string invalidEmail = "notanemail";

            // Act
            bool result = ValidationService.IsValidEmail(invalidEmail);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidUsername_WithValidUsername_ReturnsTrue()
        {
            // Arrange
            string validUsername = "john_doe123";

            // Act
            bool result = ValidationService.IsValidUsername(validUsername);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidUsername_WithShortUsername_ReturnsFalse()
        {
            // Arrange
            string shortUsername = "ab";

            // Act
            bool result = ValidationService.IsValidUsername(shortUsername);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidUsername_WithSpecialCharacters_ReturnsFalse()
        {
            // Arrange
            string invalidUsername = "john@doe";

            // Act
            bool result = ValidationService.IsValidUsername(invalidUsername);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidName_WithValidName_ReturnsTrue()
        {
            // Arrange
            string validName = "John Doe";

            // Act
            bool result = ValidationService.IsValidName(validName);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidName_WithEmptyName_ReturnsFalse()
        {
            // Arrange
            string emptyName = "";

            // Act
            bool result = ValidationService.IsValidName(emptyName);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Tests de Hashing de Contraseñas

        [Fact]
        public void HashPassword_CreatesValidHash()
        {
            // Arrange
            string password = "MySecurePassword123";

            // Act
            string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            bool isVerified = BCrypt.Net.BCrypt.Verify(password, hash);

            // Assert
            Assert.NotEmpty(hash);
            Assert.NotEqual(password, hash);
            Assert.True(isVerified);
        }

        [Fact]
        public void HashPassword_VerifyWrongPassword_ReturnsFalse()
        {
            // Arrange
            string password = "MySecurePassword123";
            string wrongPassword = "WrongPassword";

            // Act
            string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            bool isVerified = BCrypt.Net.BCrypt.Verify(wrongPassword, hash);

            // Assert
            Assert.False(isVerified);
        }

        [Fact]
        public void HashPassword_ConsistentHashing_DifferentHashes()
        {
            // Arrange
            string password = "MySecurePassword123";

            // Act
            string hash1 = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            string hash2 = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

            // Assert
            Assert.NotEqual(hash1, hash2); // Los hashes deben ser diferentes
            Assert.True(BCrypt.Net.BCrypt.Verify(password, hash1));
            Assert.True(BCrypt.Net.BCrypt.Verify(password, hash2));
        }

        #endregion

        #region Tests de Sanitización

        [Fact]
        public void SanitizeInput_RemovesDangerousCharacters()
        {
            // Arrange
            string dangerousInput = "test'; DROP TABLE users;--";

            // Act
            string sanitized = ValidationService.SanitizeInput(dangerousInput);

            // Assert
            Assert.DoesNotContain("'", sanitized);
            Assert.DoesNotContain("\"", sanitized);
            Assert.Contains("test", sanitized);
        }

        #endregion

        #region Tests de Utilidades

        [Fact]
        public void IsValidUrl_WithValidUrl_ReturnsTrue()
        {
            // Arrange
            string validUrl = "https://www.example.com";

            // Act
            bool result = ValidationService.IsValidUrl(validUrl);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidUrl_WithInvalidUrl_ReturnsFalse()
        {
            // Arrange
            string invalidUrl = "not a url";

            // Act
            bool result = ValidationService.IsValidUrl(invalidUrl);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidInteger_WithValidInteger_ReturnsTrue()
        {
            // Arrange
            string validInteger = "12345";

            // Act
            bool result = ValidationService.IsValidInteger(validInteger);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidInteger_WithInvalidInteger_ReturnsFalse()
        {
            // Arrange
            string invalidInteger = "not a number";

            // Act
            bool result = ValidationService.IsValidInteger(invalidInteger);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidDecimal_WithValidDecimal_ReturnsTrue()
        {
            // Arrange
            string validDecimal = "123.45";

            // Act
            bool result = ValidationService.IsValidDecimal(validDecimal);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNotNullOrEmpty_WithEmptyString_ReturnsFalse()
        {
            // Arrange
            string emptyString = "";

            // Act
            bool result = ValidationService.IsNotNullOrEmpty(emptyString);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNotNullOrEmpty_WithValidString_ReturnsTrue()
        {
            // Arrange
            string validString = "test";

            // Act
            bool result = ValidationService.IsNotNullOrEmpty(validString);

            // Assert
            Assert.True(result);
        }

        #endregion
    }
}

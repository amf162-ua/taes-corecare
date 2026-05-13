using CoreCare.Data;
using CoreCare.Models;

namespace CoreCare.Services
{
    public class AuthService
    {
        public User? Login(string identifier, string password)
        {
            identifier = identifier.Trim();

            using var db = new CoreCareDbContext();

            // Buscar usuario por identificador (username o email) y comprobar contraseña
            var user = db.Users.FirstOrDefault(u => u.IsActive && (u.username == identifier || u.email == identifier));
            if (user == null) return null;

            // Verificar hash de contraseña (los usuarios seeders usan BCrypt)
            try
            {
                if (BCrypt.Net.BCrypt.Verify(password, user.password))
                {
                    return user;
                }
            }
            catch
            {
                // Si la verificación falla por formato (p.ej. contraseña en texto plano),
                // hacer una comparación segura como fallback.
                if (user.password == password) return user;
            }

            return null;
        }

        public User RegisterClient(string name, string username, string email, string password)
        {
            name = name.Trim();
            username = username.Trim();
            email = email.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("Todos los campos son obligatorios.");
            }

            using var db = new CoreCareDbContext();

            if (db.Users.Any(user => user.username == username))
            {
                throw new InvalidOperationException("Ya existe un usuario con ese nombre de usuario.");
            }

            if (db.Users.Any(user => user.email == email))
            {
                throw new InvalidOperationException("Ya existe un usuario con ese correo.");
            }

            // Hash de la contraseña antes de persistir
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

            var user = new User
            {
                name = name,
                username = username,
                email = email,
                password = passwordHash,
                Role = UserRole.Cliente,
                createdAt = DateTime.UtcNow,
                IsActive = true,
                Plan = TipoPlan.Basico
            };

            db.Users.Add(user);
            db.SaveChanges();

            return user;
        }
    }
}
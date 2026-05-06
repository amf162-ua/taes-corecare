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
            return db.Users.FirstOrDefault(user =>
                user.IsActive &&
                user.password == password &&
                (user.username == identifier || user.email == identifier));
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

            var user = new User
            {
                name = name,
                username = username,
                email = email,
                password = password,
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
namespace Evento.Ti.Domain.Entities
{
    public enum UserRole
    {
        Admin = 1,
        Staff = 2
    }

    public class User
    {
        // Chave primária
        public Guid Id { get; private set; }

        // Dados básicos
        public string Name { get; private set; } = null!;
        public string Email { get; private set; } = null!;

        // Nunca guardar senha em texto plano
        public string PasswordHash { get; private set; } = null!;

        // Permissão básica (por enquanto só Admin/Staff)
        public UserRole Role { get; private set; }

        public bool IsActive { get; private set; }

        // Auditoria
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Construtor para o EF
        private User() { }

        public User(string name, string email, string passwordHash, UserRole role)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

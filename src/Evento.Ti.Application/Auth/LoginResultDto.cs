namespace Evento.Ti.Application.Auth
{
    public class LoginResultDto
    {
        public bool Success { get; private set; }
        public string? Message { get; private set; }

        public Guid? UserId { get; private set; }
        public string? Name { get; private set; }
        public string? Email { get; private set; }
        public string? Role { get; private set; }

        private LoginResultDto() { }

        public static LoginResultDto Fail(string message)
            => new LoginResultDto { Success = false, Message = message };

        public static LoginResultDto Ok(Guid userId, string name, string email, string role)
            => new LoginResultDto
            {
                Success = true,
                UserId = userId,
                Name = name,
                Email = email,
                Role = role
            };
    }
}

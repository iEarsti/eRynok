using eRynok.Models;
namespace eRynok.DTOs.Authorization
{
    /// <summary>
    /// Результат аутентифікації користувача.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// JWT-токен користувача.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Дата та час завершення дії токена.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Ідентифікатор користувача.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ім'я користувача.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Електронна пошта користувача.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Роль користувача.
        /// </summary>
        public UserRole Role { get; set; }
    }
}
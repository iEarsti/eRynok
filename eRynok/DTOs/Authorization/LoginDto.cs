using System.ComponentModel.DataAnnotations;
namespace eRynok.DTOs.Authorization
{
    /// <summary>
    /// Дані для входу користувача в систему.
    /// </summary>
    public class LoginDto
    {
        /// <summary>
        /// Електронна пошта користувача.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Пароль користувача.
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
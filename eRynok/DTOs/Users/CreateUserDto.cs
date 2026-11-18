using eRynok.Models;
using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Users
{
    /// <summary>
    /// Дані для створення нового користувача.
    /// </summary>
    public class CreateUserDto
    {
        /// <summary>
        /// Ім'я користувача.
        /// </summary>
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

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
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Підтвердження пароля користувача.
        /// </summary>
        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Роль користувача в системі.
        /// 0 - Seller, 1 - Buyer, 2 - Admin.
        /// </summary>
        [Required]
        [EnumDataType(typeof(UserRole))]
        public UserRole Role { get; set; }

        /// <summary>
        /// Номер телефону користувача.
        /// </summary>
        [Phone]
        [StringLength(30, MinimumLength = 5)]
        public string? Phone { get; set; }

        /// <summary>
        /// Адреса користувача.
        /// </summary>
        [StringLength(200, MinimumLength = 3)]
        public string? Address { get; set; }
    }
}

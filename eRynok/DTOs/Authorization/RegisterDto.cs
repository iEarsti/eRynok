using eRynok.Models;
using System.ComponentModel.DataAnnotations;
namespace eRynok.DTOs.Authorization
{
    /// <summary>
    /// Дані для реєстрації нового користувача.
    /// </summary>
    public class RegisterDto
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
        /// Роль користувача.
        /// 0 - Seller, 1 - Buyer.
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
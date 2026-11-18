using eRynok.Models;
using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Users
{
    /// <summary>
    /// Дані для оновлення інформації про користувача.
    /// </summary>
    public class UpdateUserDto
    {
        /// <summary>
        /// Нове ім'я користувача.
        /// </summary>
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Нова електронна пошта користувача.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Нова роль користувача в системі.
        /// 0 - Seller, 1 - Buyer, 2 - Admin.
        /// </summary>
        [Required]
        [EnumDataType(typeof(UserRole))]
        public UserRole Role { get; set; }

        /// <summary>
        /// Новий номер телефону користувача.
        /// </summary>
        [Phone]
        [StringLength(30, MinimumLength = 5)]
        public string? Phone { get; set; }

        /// <summary>
        /// Нова адреса користувача.
        /// </summary>
        [StringLength(200, MinimumLength = 3)]
        public string? Address { get; set; }
    }
}

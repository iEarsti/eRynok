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
        /// Нове ім'я користувача. Від 2 до 150 символів.
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
        /// Нова роль користувача: Seller, Buyer або Admin.
        /// </summary>
        [Required]
        [EnumDataType(typeof(UserRole))]
        public UserRole Role { get; set; }

        /// <summary>
        /// Новий контактний номер телефону. Обов'язковий для продавців і покупців.
        /// </summary>
        [Phone]
        [StringLength(30, MinimumLength = 5)]
        public string? Phone { get; set; }

        /// <summary>
        /// Нова адреса користувача. Обов'язкова для продавців і покупців.
        /// </summary>
        [StringLength(200, MinimumLength = 3)]
        public string? Address { get; set; }
    }
}

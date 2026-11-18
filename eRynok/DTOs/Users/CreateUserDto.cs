using eRynok.Models;
using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Users
{
    /// <summary>
    /// Дані для реєстрації нового користувача.
    /// </summary>
    public class CreateUserDto
    {
        /// <summary>
        /// Ім'я користувача. Від 2 до 150 символів.
        /// </summary>
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Унікальна електронна пошта користувача.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Роль користувача: Seller, Buyer або Admin.
        /// </summary>
        [Required]
        [EnumDataType(typeof(UserRole))]
        public UserRole Role { get; set; }

        /// <summary>
        /// Контактний номер телефону. Обов'язковий для продавців і покупців.
        /// </summary>
        [Phone]
        [StringLength(30, MinimumLength = 5)]
        public string? Phone { get; set; }

        /// <summary>
        /// Адреса користувача. Обов'язкова для продавців і покупців.
        /// </summary>
        [StringLength(200, MinimumLength = 3)]
        public string? Address { get; set; }
    }
}

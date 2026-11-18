using eRynok.Models;

namespace eRynok.DTOs.Users
{
    /// <summary>
    /// Дані користувача, які повертаються клієнту.
    /// </summary>
    public class UserResponseDto
    {
        /// <summary>
        /// Унікальний ідентифікатор користувача.
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
        /// Роль користувача в системі.
        /// 0 - Seller, 1 - Buyer, 2 - Admin.
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Номер телефону користувача.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Адреса користувача.
        /// </summary>
        public string? Address { get; set; }
    }
}

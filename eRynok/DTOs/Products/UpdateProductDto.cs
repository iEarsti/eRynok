using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Products
{
    /// <summary>
    /// Дані для оновлення існуючого товару.
    /// </summary>
    public class UpdateProductDto
    {
        /// <summary>
        /// Нова назва товару. Від 2 до 100 символів.
        /// </summary>
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Новий опис товару. Від 5 до 1000 символів.
        /// </summary>
        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Нова ціна товару. Повинна бути більшою за 0.
        /// </summary>
        [Range(0.01, 100000000)]
        public decimal Price { get; set; }

        /// <summary>
        /// Нова URL-адреса зображення товару.
        /// </summary>
        [Url]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Новий ідентифікатор категорії товару.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        /// <summary>
        /// Новий ідентифікатор продавця.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
    }
}

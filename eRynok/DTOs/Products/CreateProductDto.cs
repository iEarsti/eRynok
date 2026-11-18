using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Products
{
    /// <summary>
    /// Дані для створення нового товару.
    /// </summary>
    public class CreateProductDto
    {
        /// <summary>
        /// Назва товару. Від 2 до 100 символів.
        /// </summary>
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Опис товару. Від 5 до 1000 символів.
        /// </summary>
        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Ціна товару. Повинна бути більшою за 0.
        /// </summary>
        [Range(0.01, 100000000)]
        public decimal Price { get; set; }

        /// <summary>
        /// URL-адреса зображення товару.
        /// </summary>
        [Url]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Ідентифікатор категорії товару.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        /// <summary>
        /// Ідентифікатор користувача з роллю продавця.
        /// </summary>
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
    }
}

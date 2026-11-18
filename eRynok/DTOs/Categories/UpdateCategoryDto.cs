using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Categories
{
    /// <summary>
    /// Дані для оновлення категорії.
    /// </summary>
    public class UpdateCategoryDto
    {
        /// <summary>
        /// Назва категорії. Від 2 до 100 символів.
        /// </summary>
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Опис категорії. Від 5 до 500 символів.
        /// </summary>
        [Required]
        [StringLength(500, MinimumLength = 5)]
        public string Description { get; set; } = string.Empty;
    }
}

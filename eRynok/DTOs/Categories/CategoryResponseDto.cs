namespace eRynok.DTOs.Categories
{
    /// <summary>
    /// Дані категорії, які повертаються клієнту.
    /// </summary>
    public class CategoryResponseDto
    {
        /// <summary>
        /// Унікальний ідентифікатор категорії.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Назва категорії.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Опис категорії.
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}

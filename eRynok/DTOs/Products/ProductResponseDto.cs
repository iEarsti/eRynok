namespace eRynok.DTOs.Products
{
    /// <summary>
    /// Дані товару, які повертаються клієнту.
    /// </summary>
    public class ProductResponseDto
    {
        /// <summary>
        /// Унікальний ідентифікатор товару.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Назва товару.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Опис товару.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Ціна товару.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// URL-адреса зображення товару.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Ідентифікатор категорії, до якої належить товар.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Ідентифікатор продавця, який створив товар.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Дата та час створення товару.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}

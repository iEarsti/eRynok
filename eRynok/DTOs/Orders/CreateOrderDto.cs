using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Orders
{
    /// <summary>
    /// Дані для створення нового замовлення.
    /// </summary>
    public class CreateOrderDto
    {
        /// <summary>
        /// Ідентифікатор покупця, який створює замовлення.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int BuyerId { get; set; }

        /// <summary>
        /// Ідентифікатор товару, який замовляється.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        /// <summary>
        /// Кількість одиниць товару. Від 1 до 1000.
        /// </summary>
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
    }
}

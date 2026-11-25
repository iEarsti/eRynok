using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Orders
{
    /// <summary>
    /// DTO для одного товару в замовленні.
    /// </summary>
    public class CreateOrderItemDto
    {
        /// <summary>
        /// Ідентифікатор товару.
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Кількість одиниць товару.
        /// </summary>
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
    }
}
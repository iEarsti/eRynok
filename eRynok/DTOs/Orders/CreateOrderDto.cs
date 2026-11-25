using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Orders
{
    /// <summary>
    /// DTO для створення групового замовлення.
    /// </summary>
    public class CreateOrderDto
    {
        /// <summary>
        /// Список товарів, які входять до замовлення.
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<CreateOrderItemDto> Items { get; set; }
            = new List<CreateOrderItemDto>();
    }
}
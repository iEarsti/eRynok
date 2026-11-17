using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Orders
{
    public class CreateOrderDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int BuyerId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
    }
}

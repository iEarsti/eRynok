namespace eRynok.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int? OrderGroupId { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ApplicationUser Buyer { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public OrderGroup? OrderGroup { get; set; }
    }
}

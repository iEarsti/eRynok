namespace eRynok.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Category Category { get; set; } = null!;
        public User Seller { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

using Microsoft.AspNetCore.Identity;

namespace eRynok.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty;

        public SellerProfile? SellerProfile { get; set; }

        public BuyerProfile? BuyerProfile { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<OrderGroup> OrderGroups { get; set; } = new List<OrderGroup>();
    }
}

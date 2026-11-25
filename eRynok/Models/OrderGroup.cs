using System.ComponentModel.DataAnnotations;

namespace eRynok.Models
{
    public class OrderGroup
    {
        public int Id { get; set; }

        [Required]
        public int BuyerId { get; set; }
        public ApplicationUser Buyer { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

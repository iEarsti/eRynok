using System.ComponentModel.DataAnnotations;

namespace eRynok.Models
{
    public class SellerProfile
    {
        [Key]
        public int UserId { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
    }
}

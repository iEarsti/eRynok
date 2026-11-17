using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Sellers
{
    public class UpdateSellerDto
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Individual|Company)$",
            ErrorMessage = "SellerType must be Individual or Company.")]
        public string SellerType { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(30, MinimumLength = 5)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Address { get; set; } = string.Empty;
    }
}

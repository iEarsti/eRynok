using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Products
{
    public class UpdateProductDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000000)]
        public decimal Price { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue)]
        public int UserId { get; set; }
    }
}

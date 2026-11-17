using eRynok.Models;
using System.ComponentModel.DataAnnotations;

namespace eRynok.DTOs.Users
{
    public class UpdateUserDto
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [EnumDataType(typeof(UserRole))]
        public UserRole Role { get; set; }

        [Phone]
        [StringLength(30, MinimumLength = 5)]
        public string? Phone { get; set; }

        [StringLength(200, MinimumLength = 3)]
        public string? Address { get; set; }
    }
}

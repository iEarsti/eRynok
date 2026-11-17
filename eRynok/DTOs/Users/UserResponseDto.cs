using eRynok.Models;

namespace eRynok.DTOs.Users
{
    public class UserResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }
    }
}

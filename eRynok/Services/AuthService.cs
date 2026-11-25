using eRynok.DTOs.Authorization;
using eRynok.Models;
using eRynok.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace eRynok.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<ServiceResult<AuthResponseDto>> Register(
            RegisterDto dto)
        {
            var email = dto.Email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return ServiceResult<AuthResponseDto>.Failure(
                    ServiceError.Conflict,
                    "A user with this email already exists.");
            }

            if (dto.Role != UserRole.Seller
                && dto.Role != UserRole.Buyer)
            {
                return ServiceResult<AuthResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "Only sellers and buyers can register.");
            }

            if (string.IsNullOrWhiteSpace(dto.Phone)
                || string.IsNullOrWhiteSpace(dto.Address))
            {
                return ServiceResult<AuthResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "Phone and address are required.");
            }

            var user = new ApplicationUser
            {
                Name = dto.Name.Trim(),
                UserName = email,
                Email = email
            };

            // Створення профілю відповідно до ролі
            if (dto.Role == UserRole.Seller)
            {
                user.SellerProfile = new SellerProfile
                {
                    Phone = dto.Phone.Trim(),
                    Address = dto.Address.Trim()
                };
            }
            else
            {
                user.BuyerProfile = new BuyerProfile
                {
                    Phone = dto.Phone.Trim(),
                    Address = dto.Address.Trim()
                };
            }

            var createResult = await _userManager.CreateAsync(
                user,
                dto.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    " ",
                    createResult.Errors.Select(error => error.Description));

                return ServiceResult<AuthResponseDto>.Failure(
                    ServiceError.Conflict,
                    errors);
            }

            var roleName = dto.Role.ToString().ToUpper();

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                roleName);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                var errors = string.Join(
                    " ",
                    roleResult.Errors.Select(error => error.Description));

                return ServiceResult<AuthResponseDto>.Failure(
                    ServiceError.Conflict,
                    errors);
            }

            return ServiceResult<AuthResponseDto>.Success(
                GenerateToken(user, dto.Role));
        }

        public async Task<ServiceResult<AuthResponseDto>> Login(
            LoginDto dto)
        {
            var email = dto.Email.Trim();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return ServiceResult<AuthResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "Invalid email or password.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(
                user,
                dto.Password);

            if (!passwordValid)
            {
                return ServiceResult<AuthResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "Invalid email or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var roleName = roles.FirstOrDefault();

            if (!Enum.TryParse<UserRole>(
                    roleName,
                    true,
                    out var role))
            {
                return ServiceResult<AuthResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "User role could not be determined.");
            }

            return ServiceResult<AuthResponseDto>.Success(
                GenerateToken(user, role));
        }

        private AuthResponseDto GenerateToken(
            ApplicationUser user,
            UserRole role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var key = jwtSettings["Key"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;

            var expiresInMinutes = int.Parse(
                jwtSettings["ExpiresInMinutes"]!);

            var expiresAt = DateTime.UtcNow.AddMinutes(
                expiresInMinutes);

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Name),

                new Claim(
                    ClaimTypes.Email,
                    user.Email ?? string.Empty),

                new Claim(
                    ClaimTypes.Role,
                    role.ToString())
            };

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return new AuthResponseDto
            {
                Token = tokenString,
                ExpiresAt = expiresAt,
                Id = user.Id,
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                Role = role
            };
        }
    }
}
using eRynok.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eRynok.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRoles(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole<int>>>();

            string[] roles =
            {
                "SELLER",
                "BUYER",
                "ADMIN"
            };

            foreach (var role in roles)
            {
                bool roleExists = await roleManager.RoleExistsAsync(role);

                if (!roleExists)
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole<int>(role));

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            $"Failed to create role {role}: " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }

            var adminEmail = configuration["Admin:Email"];
            var adminPassword = configuration["Admin:Password"];
            var adminName = configuration["Admin:Name"] ?? "Administrator";

            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new Exception("Admin credentials are not configured.");
            }

            var userManager = serviceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Name = adminName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    admin, adminPassword);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        "Failed to create admin: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "ADMIN"))
            {
                var result = await userManager.AddToRoleAsync(admin, "ADMIN");

                if (!result.Succeeded)
                {
                    throw new Exception(
                        "Failed to assign ADMIN role: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}

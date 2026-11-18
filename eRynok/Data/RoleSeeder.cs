using Microsoft.AspNetCore.Identity;

namespace eRynok.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRoles(IServiceProvider serviceProvider)
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
                    await roleManager.CreateAsync(
                        new IdentityRole<int>(role));
                }
            }
        }
    }
}

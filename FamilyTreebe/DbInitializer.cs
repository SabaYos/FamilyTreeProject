using LoginAPI.Models;
using Microsoft.AspNetCore.Identity;


namespace LoginAPI
{
    public class DbInitializer
    {
        public static async Task SeedRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            string[] roleNames = { "Admin", "FamilyMember", "Viewer" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"Role '{roleName}' created successfully!");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create role '{roleName}': {string.Join(", ", result.Errors)}");
                    }
                }
                else
                {
                    Console.WriteLine($"Role '{roleName}' already exists.");
                }
            }
        }
    }
}

using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;



namespace GP.Services
{
    

    public class RolesSeeder
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User","Doctor" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

}

using Microsoft.AspNetCore.Identity;
using ProcureFlow.Web.Security;

namespace ProcureFlow.Web.Data;

public static class IdentitySeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var role in new[] { AppRoles.Admin, AppRoles.Manager, AppRoles.Employee })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await CreateUserIfMissingAsync(userManager, "admin@procureflow.local", "AdminDemo!2026", AppRoles.Admin);
        await CreateUserIfMissingAsync(userManager, "manager@procureflow.local", "ManagerDemo!2026", AppRoles.Manager);
        await CreateUserIfMissingAsync(userManager, "employee@procureflow.local", "EmployeeDemo!2026", AppRoles.Employee);
    }

    private static async Task CreateUserIfMissingAsync(
        UserManager<IdentityUser> userManager,
        string email,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not create the demo account '{email}': {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}

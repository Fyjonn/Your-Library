using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using YourLibrary.Models;
using System;
using System.Threading.Tasks;

namespace YourLibrary.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "User" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            string adminEmail = "admin@yourlibrary.com";
            string adminPassword = "SecureAdmin2026!";

            //var adminUser = await userManager.FindByEmailAsync(adminEmail);

            //if (adminUser == null)
            //{
            //    var newAdmin = new ApplicationUser
            //    {
            //        UserName = adminEmail,
            //        Email = adminEmail,
            //        EmailConfirmed = true
            //    };

            //    var createAdmin = await userManager.CreateAsync(newAdmin, adminPassword);
            //    if (createAdmin.Succeeded)
            //    {
            //        await userManager.AddToRoleAsync(newAdmin, "Admin");
            //    }
            //}
            //else
            //{
            //    var isInRole = await userManager.IsInRoleAsync(adminUser, "Admin");
            //    if (!isInRole)
            //    {
            //        await userManager.AddToRoleAsync(adminUser, "Admin");
            //    }
            //}

            Console.WriteLine("=== ADMIN SEED START ===");

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            Console.WriteLine(adminUser == null
                ? "Admin user NOT FOUND"
                : "Admin user ALREADY EXISTS");

            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    DisplayName = "Administrator",
                    EmailConfirmed = true
                };

                var createAdmin = await userManager.CreateAsync(newAdmin, adminPassword);

                Console.WriteLine($"CreateAdmin Success: {createAdmin.Succeeded}");

                foreach (var error in createAdmin.Errors)
                {
                    Console.WriteLine($"CreateAdmin Error: {error.Description}");
                }

                if (createAdmin.Succeeded)
                {
                    var addRoleResult = await userManager.AddToRoleAsync(newAdmin, "Admin");

                    Console.WriteLine($"AddToRole Success: {addRoleResult.Succeeded}");

                    foreach (var error in addRoleResult.Errors)
                    {
                        Console.WriteLine($"AddToRole Error: {error.Description}");
                    }
                }
            }
            else
            {
                var isInRole = await userManager.IsInRoleAsync(adminUser, "Admin");

                Console.WriteLine($"Already in Admin role: {isInRole}");

                if (!isInRole)
                {
                    var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

                    Console.WriteLine($"Add Existing User To Role Success: {addRoleResult.Succeeded}");

                    foreach (var error in addRoleResult.Errors)
                    {
                        Console.WriteLine($"Add Existing User To Role Error: {error.Description}");
                    }
                }
            }

            Console.WriteLine("=== ADMIN SEED END ===");
        }
    }
}
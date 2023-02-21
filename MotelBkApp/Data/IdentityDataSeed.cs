using Microsoft.AspNetCore.Identity;
using MotelBkApp.Models;

namespace MotelBkApp.Data
{
    public class IdentityDataSeed
    {
        public static Task InitializeAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            string[] roleNamesList = new string[] { "Admin", "User", "Staff" };
            foreach (string roleName in roleNamesList)
            {
                if (!roleManager.RoleExistsAsync(roleName).Result)
                {
                    AppRole role = new AppRole();
                    role.Name = roleName;
                    IdentityResult result = roleManager.CreateAsync(role).Result;
                    // Warning: we ignore any errors that Create may return, they should be AT LEAST logged in!
                    foreach (IdentityError error in result.Errors)
                    {
                        // TODO: Log it!
                    }
                }
            }
            // CREATE admin -- Testing ONLY
            if (userManager.FindByNameAsync("admin@admin.com").Result == null)
            {
                AppUser user = new AppUser();
                user.UserName = "Admin";
                user.PhoneNumber = "2345678901";
                user.Email = "admin@admin.com";
                user.EmailConfirmed = true;
                user.FirstName = "AdminFirstName";
                user.LastName = "AdminLastName";
                user.DOB = DateOnly.Parse("1979-08-20");

                IdentityResult result = userManager.CreateAsync(user, "abcdefgA2@").Result;

                if (result.Succeeded)
                {
                    IdentityResult result2 = userManager.AddToRoleAsync(user, "Admin").Result;
                    if (!result2.Succeeded)
                    {
                        //FIXME: log the error
                    }
                }
                else
                {
                    //FIXME: log the error
                }
            }

            // CREATE staff -- Testing ONLY
            if (userManager.FindByNameAsync("staff@staff.com").Result == null)
            {
                AppUser user = new AppUser();
                user.UserName = "Staff";
                user.PhoneNumber = "2345678901";
                user.Email = "staff@staff.com";
                user.EmailConfirmed = true;
                user.FirstName = "StaffFirst";
                user.LastName = "StaffLast";
                user.DOB = DateOnly.Parse("1992-12-20");

                IdentityResult result = userManager.CreateAsync(user, "abcdefgA2@").Result;

                if (result.Succeeded)
                {
                    IdentityResult result2 = userManager.AddToRoleAsync(user, "Staff").Result;
                    if (!result2.Succeeded)
                    {
                        //FIXME: log the error
                    }
                }
                else
                {
                    //FIXME: log the error
                }
            }

            return Task.CompletedTask;
        }
    }
}

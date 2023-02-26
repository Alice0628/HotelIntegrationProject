using Microsoft.AspNetCore.Identity;
using MotelBookingApp.Models;


namespace MotelBookingApp.Data
{
    public class IdentityDataSeed
    {

        public static async Task InitializeAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
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
            string adminEmail = "admin@admin.com";
            string adminUserName = "Admin";
            string adminPhoneNumber = "2345678901";
            string adminPass = "abcdefgA2@";
            if (userManager.FindByNameAsync(adminEmail).Result == null)
            {
                AppUser user = new AppUser();
                user.UserName = adminUserName;
                user.PhoneNumber = adminPhoneNumber;
                user.Email = adminEmail;
                user.EmailConfirmed = true;
                user.FirstName = "AdminFirstName";
                user.LastName = "AdminLastName";
                user.DOB =DateTime.Parse("1979-08-20");

                IdentityResult result = userManager.CreateAsync(user, adminPass).Result;

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
        }
    }
}

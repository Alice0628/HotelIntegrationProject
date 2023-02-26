using MotelBookingApp.Data;
using MotelBookingApp.Models;
using MotelBookingApp.Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MotelBookingApp.Controllers
{
    public class AdminCustomerController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public AdminCustomerController(UserManager<AppUser> userManager, MotelDbContext context)
        {
            this._userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Detail(int? id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["CustomerInfo"] = "Customer not exists";
            }

            EditStaffVM viewCustomer = new EditStaffVM();

            viewCustomer.Id = user.Id;
            viewCustomer.FirstName = user.FirstName;
            viewCustomer.LastName = user.LastName;
            viewCustomer.UserName = user.UserName;
            viewCustomer.Email = user.Email;
            viewCustomer.DOB = DateTime.Parse(user.DOB.ToString());
            viewCustomer.PhoneNumber = user.PhoneNumber;

            return View(viewCustomer);
        }

        public async Task<IActionResult> CustomerList(string searchString)
        {
            var roleId = _context.Roles.Where(r => r.Name == "User").FirstOrDefault().Id;
            var roleList = await _context.UserRoles.Where(ur => ur.RoleId == roleId).ToListAsync();

            List<AppUser> customerList = new List<AppUser>();
            for (int i = 0; i < roleList.Count; i++)
            {
                var customer = _context.Users.Include("Motel").Where(s => s.Id == roleList[i].UserId).FirstOrDefault();
                customerList.Add(customer);
            }
            if (customerList == null)
            {
                TempData["CustomerListOption"] = "No Customer Exist!";
                return View(customerList);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                customerList = await _context.Users.Include("Motel").Where(s => s.Id != 1 && s.Motel.Id == null).Where(s =>
                    s.FirstName.ToLower().Contains(searchString.ToLower()) ||
                    s.LastName.ToLower().Contains(searchString.ToLower()) ||
                    s.Email.ToLower().Contains(searchString.ToLower()) ||
                    s.PhoneNumber.Contains(searchString)
                    ).ToListAsync();
                if (customerList.Count <= 0)
                {
                    TempData["CustomerListOption"] = "No Customer Found!";
                    return View(customerList);
                }
                return View(customerList);
            }
            return View(customerList);
        }
    }
}

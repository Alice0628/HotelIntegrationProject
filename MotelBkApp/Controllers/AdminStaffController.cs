using MotelBkApp.Data;
using MotelBkApp.Models;
using MotelBkApp.Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MotelBkApp.Controllers
{
    public class AdminStaffController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public AdminStaffController(UserManager<AppUser> userManager, MotelDbContext context)
        {
            this._userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
       
        public IActionResult Create() => View(new RegisterVM());

        [HttpPost]
        public async Task<IActionResult> Create(RegisterVM registerVM)
        {
            if (!ModelState.IsValid) return View(registerVM);

            var user = await _userManager.FindByEmailAsync(registerVM.Email);
            if (user != null)
            {
                TempData["StaffCreate"] = "This email address is already in use";
                return View(registerVM);
            }

            //if (Motel.Id == 0)
            //{
            //    TempData["StaffCreate"] = "Please choose a motel to sign to.";
            //    return View(registerVM);
            //}

            var newUser = new AppUser()
            {
                Email = registerVM.Email,
                UserName = registerVM.UserName,
                PhoneNumber = registerVM.PhoneNumber,
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                DOB = registerVM.DOB,
                //Motel.Id = registerVM.Motel.Id,
            };

            var newUserResponse = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (newUserResponse.Succeeded)
            {
                // sign user to "Staff"
                var signToStaff = await _userManager.AddToRoleAsync(newUser, "Staff");
                TempData["StaffOption"] = "Staff Created.";
                return RedirectToAction("Index", "AdminStaff");
            }
            foreach (var error in newUserResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(registerVM);

        }
    }
}

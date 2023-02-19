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

        public IActionResult Create()
        {
            List<Motel> motelList = _context.Motels.ToList();
            if (motelList.Count == 0)
            {
                TempData["StaffOption"] = "Please add airline first";
            }

            NewStaffVM newStaff = new NewStaffVM();
            newStaff.MotelList = motelList;
            return View(newStaff); 
        }

        [HttpPost]
        public async Task<IActionResult> Create(NewStaffVM newStaff)
        {
            List<Motel> motelList = await _context.Motels.ToListAsync();
            newStaff.MotelList = motelList;

            // validation first
            if (!ModelState.IsValid) return View(newStaff);

            // staff exists
            AppUser user = await _userManager.FindByEmailAsync(newStaff.Email);
            if (user != null)
            {
                TempData["StaffCreate"] = "This email address is already in use";
                return View(newStaff);
            }
            
            Motel motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == newStaff.Motel);

            var newUser = new AppUser()
            {
                Email = newStaff.Email,
                UserName = newStaff.UserName,
                PhoneNumber = newStaff.PhoneNumber,
                FirstName = newStaff.FirstName,
                LastName = newStaff.LastName,
                DOB = newStaff.DOB,
                Motel = motel,
            };

            var newUserResponse = await _userManager.CreateAsync(newUser, newStaff.Password);

            if (newUserResponse.Succeeded)
            {
                // sign user to "Staff"
                var signToStaff = await _userManager.AddToRoleAsync(newUser, "Staff");
                // set EmailConfirmation to true
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                var result = await _userManager.ConfirmEmailAsync(newUser, code);
                if (result.Succeeded)
                {
                    TempData["StaffOption"] = "Staff Created.";
                    return RedirectToAction("Index", "AdminStaff");
                }
            }
            foreach (var error in newUserResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(newStaff);
        }
    }
}

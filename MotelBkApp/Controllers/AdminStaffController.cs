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

        public async Task<IActionResult> Detail(int? id)
        {
            var user = await _context.Users.Include("Motel").FirstOrDefaultAsync(u => u.Id == id);
            if(user == null)
            {
                TempData["StaffInfo"] = "Staff not exists";
            }

            NewStaffVM newStaff = new NewStaffVM();
            newStaff.Id = user.Id;
            newStaff.FirstName = user.FirstName;
            newStaff.LastName = user.LastName;
            newStaff.UserName = user.UserName;
            newStaff.Email = user.Email;
            newStaff.DOB = DateOnly.Parse(user.DOB.ToString());
            newStaff.PhoneNumber = user.PhoneNumber;
            newStaff.Motel = user.Motel.Id;

            return View(newStaff);
        }

        public IActionResult Create()
        {
            List<Motel> motelList = _context.Motels.ToList();
            if (motelList.Count == 0)
            {
                TempData["StaffOption"] = "No staff exists";
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

        public async Task<IActionResult> StaffList(string searchString)
        {
            var staffList = await _context.Users.Include("Motel").Where(s => s.Motel.Id != null).ToListAsync();
            if (staffList == null)
            {
                TempData["StaffListOption"] = "No Staff Exist!";
                return View(staffList);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                // Search by fullname
                //List<string> fullNameList = new List<string>();
                //foreach (var staff in staffList)
                //{
                //    string fullName = staff.FirstName.ToLower() + " " + staff.LastName.ToLower();
                //    fullNameList.Add(fullName);
                //}

                staffList = await _context.Users.Include("Motel").Where(
                    s => s.Motel.Name.ToLower().Contains(searchString.ToLower()) ||
                    s.FirstName.ToLower().Contains(searchString.ToLower()) ||
                    s.LastName.ToLower().Contains(searchString.ToLower())
                    ).ToListAsync();

                if (staffList.Count <= 0)
                {
                    TempData["StaffListOption"] = "No Staff Found!";
                }
            }
            return View(staffList);
        }
    }
}

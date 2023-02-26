using MotelBookingApp.Data;
using MotelBookingApp.Models;
using MotelBookingApp.Data.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MotelBookingApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public ProfileController(UserManager<AppUser> userManager, MotelDbContext context)
        {
            this._userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Detail(int? id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                TempData["ProfileInfo"] = "User not exists";
            }

            EditStaffVM viewCustomer = new EditStaffVM();

            viewCustomer.Id = user.Id;
            viewCustomer.FirstName = user.FirstName;
            viewCustomer.LastName = user.LastName;
            viewCustomer.UserName = user.UserName;
            viewCustomer.Email = user.Email;
            viewCustomer.DOB = DateOnly.Parse(user.DOB.ToString());
            viewCustomer.PhoneNumber = user.PhoneNumber;

            return View(viewCustomer);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                EditStaffVM editStaff = new EditStaffVM();
                List<Motel> motelList = await _context.Motels.ToListAsync();
                editStaff.MotelList = motelList;

                var staff = await _context.Users.Include("Motel").FirstOrDefaultAsync(u => u.Id == id);
                if (staff == null)
                {
                    TempData["ProfileInfo"] = "User not exists";
                }
                editStaff.Id = staff.Id;
                editStaff.UserName = staff.UserName;
                editStaff.Email = staff.Email;
                editStaff.PhoneNumber = staff.PhoneNumber;
                editStaff.FirstName = staff.FirstName;
                editStaff.LastName = staff.LastName;
                editStaff.DOB = DateOnly.Parse(staff.DOB.ToString());

                return View(editStaff);
            }
            catch (SystemException ex)
            {
                TempData["UserEditOption"] = $"{ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, EditStaffVM editStaff)
        {
            try
            {
                List<Motel> motelList = await _context.Motels.ToListAsync();
                editStaff.MotelList = motelList;

                if (!ModelState.IsValid)
                {
                    TempData["UserEditOption"] = "error here";
                    return View(editStaff);
                }
                var staff = _context.Users.FirstOrDefault(u => u.Id == id);
                List<AppUser> userList = _context.Users.ToList<AppUser>();
                userList.Remove(staff);

                AppUser ifUser = userList.Find(u => u.Email == editStaff.Email);
                if (ifUser != null)
                {
                    TempData["UserEditOption"] = $"{editStaff.Email} has already existed";
                    return View(editStaff);
                }

                Motel motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == editStaff.Motel);

                staff.Id = editStaff.Id;
                staff.UserName = editStaff.UserName;
                staff.Email = editStaff.Email;
                staff.PhoneNumber = editStaff.PhoneNumber;
                staff.FirstName = editStaff.FirstName;
                staff.LastName = editStaff.LastName;
                staff.DOB = DateOnly.Parse(editStaff.DOB.ToString());

                _context.Users.Update(staff);
                await _context.SaveChangesAsync();

                TempData["ProfileInfo"] = "User Updated";
                return RedirectToAction($"Detail", "Profile", new { id = id });
            }
            catch (SystemException ex)
            {
                TempData["UserEditOption"] = $"{ex.Message}";
                return View();
            }
        }

    }
}

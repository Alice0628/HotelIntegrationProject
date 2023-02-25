//using MotelBookingApp.Data;
//using MotelBookingApp.Models;
//using MotelBookingApp.Data.ViewModels;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace MotelBookingApp.Controllers
//{
//    public class AdminStaffController : Controller
//    {
//        private readonly MotelDbContext _context;
//        private readonly UserManager<AppUser> _userManager;
//        public AdminStaffController(UserManager<AppUser> userManager, MotelDbContext context)
//        {
//            this._userManager = userManager;
//            _context = context;
//        }

//        public IActionResult Index()
//        {
//            return View();
//        }

//        public async Task<IActionResult> Detail(int? id)
//        {
//            var user = await _context.Users.Include("Motel").FirstOrDefaultAsync(u => u.Id == id);
//            if (user == null)
//            {
//                TempData["StaffInfo"] = "Staff not exists";
//            }

//            EditStaffVM editStaff = new EditStaffVM();

//            List<Motel> motelList = await _context.Motels.ToListAsync();
//            editStaff.MotelList = motelList;

//            editStaff.Id = user.Id;
//            editStaff.FirstName = user.FirstName;
//            editStaff.LastName = user.LastName;
//            editStaff.UserName = user.UserName;
//            editStaff.Email = user.Email;
//            editStaff.DOB = DateOnly.Parse(user.DOB.ToString());
//            editStaff.PhoneNumber = user.PhoneNumber;
//            editStaff.Motel = user.Motel.Id;

//            return View(editStaff);
//        }

//        public IActionResult Create()
//        {
//            List<Motel> motelList = _context.Motels.ToList();
//            if (motelList.Count == 0)
//            {
//                TempData["StaffOption"] = "No staff exists";
//            }

//            NewStaffVM newStaff = new NewStaffVM();
//            newStaff.MotelList = motelList;
//            return View(newStaff);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(NewStaffVM newStaff)
//        {
//            List<Motel> motelList = await _context.Motels.ToListAsync();
//            newStaff.MotelList = motelList;

//            // validation first
//            if (!ModelState.IsValid) return View(newStaff);

//            // staff exists
//            AppUser user = await _userManager.FindByEmailAsync(newStaff.Email);
//            if (user != null)
//            {
//                TempData["StaffCreate"] = "This email address is already in use";
//                return View(newStaff);
//            }

//            Motel motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == newStaff.Motel);

//            var newUser = new AppUser()
//            {
//                Email = newStaff.Email,
//                UserName = newStaff.UserName,
//                PhoneNumber = newStaff.PhoneNumber,
//                FirstName = newStaff.FirstName,
//                LastName = newStaff.LastName,
//                DOB = newStaff.DOB,
//                Motel = motel,
//            };

//            var newUserResponse = await _userManager.CreateAsync(newUser, newStaff.Password);

//            if (newUserResponse.Succeeded)
//            {
//                // sign user to "Staff"
//                var signToStaff = await _userManager.AddToRoleAsync(newUser, "Staff");
//                // set EmailConfirmation to true
//                var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
//                var result = await _userManager.ConfirmEmailAsync(newUser, code);
//                if (result.Succeeded)
//                {
//                    TempData["StaffListOption"] = "Staff Created.";
//                    return RedirectToAction("StaffList", "AdminStaff");
//                }
//            }
//            foreach (var error in newUserResponse.Errors)
//            {
//                ModelState.AddModelError(string.Empty, error.Description);
//            }
//            return View(newStaff);
//        }


//        //GET: AdminStaff/Edit/id
//        public async Task<IActionResult> Edit(int? id)
//        {
//            try
//            {
//                EditStaffVM editStaff = new EditStaffVM();
//                List<Motel> motelList = await _context.Motels.ToListAsync();
//                editStaff.MotelList = motelList;

//                var staff = await _context.Users.Include("Motel").FirstOrDefaultAsync(u => u.Id == id);
//                if (staff == null)
//                {
//                    TempData["StaffInfo"] = "Staff not exists";
//                }
//                editStaff.Id = staff.Id;
//                editStaff.UserName = staff.UserName;
//                editStaff.Email = staff.Email;
//                editStaff.PhoneNumber = staff.PhoneNumber;
//                editStaff.FirstName = staff.FirstName;
//                editStaff.LastName = staff.LastName;
//                editStaff.DOB = DateOnly.Parse(staff.DOB.ToString());
//                editStaff.Motel = staff.Motel.Id;

//                return View(editStaff);
//            }
//            catch (SystemException ex)
//            {
//                TempData["StaffEditInfo"] = $"{ex.Message}";
//                return View();
//            }
//        }

//        //POST: AdminStaff/Edit/id
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int? id, EditStaffVM editStaff)
//        {
//            try
//            {
//                List<Motel> motelList = await _context.Motels.ToListAsync();
//                editStaff.MotelList = motelList;

//                if (!ModelState.IsValid)
//                {
//                    TempData["StaffEditOption"] = "error here";
//                    return View(editStaff);
//                }
//                var staff = _context.Users.FirstOrDefault(u => u.Id == id);
//                List<AppUser> userList = _context.Users.ToList<AppUser>();
//                userList.Remove(staff);

//                AppUser ifUser = userList.Find(u => u.Email == editStaff.Email);
//                if (ifUser != null)
//                {
//                    TempData["StaffEditOption"] = $"{editStaff.Email} has already existed";
//                    return View(editStaff);
//                }

//                Motel motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == editStaff.Motel);

//                staff.Id = editStaff.Id;
//                staff.UserName = editStaff.UserName;
//                staff.Email = editStaff.Email;
//                staff.PhoneNumber = editStaff.PhoneNumber;
//                staff.FirstName = editStaff.FirstName;
//                staff.LastName = editStaff.LastName;
//                staff.DOB = DateOnly.Parse(editStaff.DOB.ToString());
//                staff.Motel = motel;

//                _context.Users.Update(staff);
//                await _context.SaveChangesAsync();

//                TempData["StaffInfo"] = "Staff Updated";
//                return RedirectToAction($"Detail", "AdminStaff", new { id = id });
//            }
//            catch (SystemException ex)
//            {
//                TempData["StaffEditOption"] = $"{ex.Message}";
//                return View();
//            }
//        }

//        // GET: AdminStaff/Delete/id
//        public async Task<IActionResult> Delete(int? id)
//        {
//            try
//            {
//                var deleteStaff = await _context.Users.FirstOrDefaultAsync(s => s.Id == id);
//                if (deleteStaff == null)
//                {
//                    return NotFound();
//                }
//                return View(deleteStaff);
//            }
//            catch (SystemException ex)
//            {
//                TempData["StaffDeleteOption"] = $"{ex.Message}";
//                return View();
//            }
//        }

//        // POST: AdminStaff/Delete/id
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ConfirmDelete(int id)
//        {
//            try
//            {
//                var deleteStaff = await _context.Users.FindAsync(id);
//                if (deleteStaff == null)
//                {
//                    TempData["StaffDeleteInfo"] = "Staff error!";
//                    return View(deleteStaff);
//                }
//                _context.Users.Remove(deleteStaff);
//                await _context.SaveChangesAsync();

//                TempData["StaffListOption"] = $"Staff has been deleted successfully";
//                return RedirectToAction($"StaffList", "AdminStaff");
//            }
//            catch (SystemException ex)
//            {
//                TempData["StaffDeleteInfo"] = $"{ex.Message}";
//                return View();
//            }
//        }


//        public async Task<IActionResult> StaffList(string searchString)
//        {
//            var roleId = _context.Roles.Where(r => r.Name == "Staff").FirstOrDefault().Id;
//            var roleList = await _context.UserRoles.Where(ur => ur.RoleId == roleId).ToListAsync();

//            List<AppUser> staffList = new List<AppUser>();
//            for (int i = 0; i < roleList.Count; i++)
//            {
//                var staff = _context.Users.Include("Motel").Where(s => s.Id == roleList[i].UserId).FirstOrDefault();
//                staffList.Add(staff);
//            }

//            if (staffList == null)
//            {
//                TempData["StaffListOption"] = "No Staff Exist!";
//                return View(staffList);
//            }

//            if (!String.IsNullOrEmpty(searchString))
//            {
//                staffList = await _context.Users.Include("Motel").Where(s => s.Motel != null).Where(s =>
//                    s.Motel.Name.ToLower().Contains(searchString.ToLower()) ||
//                    s.FirstName.ToLower().Contains(searchString.ToLower()) ||
//                    s.LastName.ToLower().Contains(searchString.ToLower())
//                    ).ToListAsync();

//                if (staffList.Count <= 0)
//                {
//                    TempData["StaffListOption"] = "No Staff Found!";
//                    return View(staffList);
//                }
//                return View(staffList);
//            }
//            return View(staffList);
//        }

//    }
//}

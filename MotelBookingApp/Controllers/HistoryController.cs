using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MotelBookingApp.Data;
using MotelBookingApp.Data.ViewModels;
using MotelBookingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MotelBookingApp.Controllers
{
    [Authorize]
    public class HistoryController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public HistoryController(UserManager<AppUser> userManager, MotelDbContext context)
        {
            this._userManager = userManager;
            _context = context;
        }

        // BookedRecord history
        public async Task<IActionResult> Detail(int? id, string searchString)
        {
            var userName = _userManager.GetUserName(User);
            var currentUser = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();

            if (currentUser.Id != id)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (_context.BookedRecords == null) return Problem("No purchases found.");

            // Customer can view their own history
            if (User.IsInRole("User"))
            {
                var purchases = await _context.BookedRecords
                .Include(br => br.Room).ThenInclude(r => r.Motel)
                .Include(br => br.Room).ThenInclude(r => r.RoomType)
                .Include(br => br.Booking).ThenInclude(b => b.AppUser)
                .Where(br => br.Booking.AppUser.Id == id).ToListAsync();

                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    purchases = await _context.BookedRecords.Where(br =>
                        br.Booking.AppUser.FirstName.ToLower().Contains(searchString) ||
                        br.Booking.AppUser.LastName.ToLower().Contains(searchString) ||
                        br.Room.Motel.Name.ToLower().Contains(searchString) ||
                        br.Room.RoomType.Name.ToLower().Contains(searchString) ||
                        br.Booking.PayTime.ToString().Contains(searchString) ||
                        br.CheckinDate.ToString().Contains(searchString) ||
                        br.CheckoutDate.ToString().Contains(searchString)
                        )
                        .Where(br => br.Booking.AppUser.Id == id)
                        .ToListAsync();

                    if (purchases.Count <= 0)
                    {
                        TempData["CHNotFound"] = "No related purchase Found!";
                    }
                }
                ViewBag.Count = HttpContext.Session.GetString("Count");
                return View(purchases);
            }

            // Admin can view all histories
            else if (User.IsInRole("Admin"))
            {
                var purchases = await _context.BookedRecords
                .Include(br => br.Room).ThenInclude(r => r.Motel)
                .Include(br => br.Room).ThenInclude(r => r.RoomType)
                .Include(br => br.Booking).ThenInclude(b => b.AppUser).ToListAsync();

                if (!String.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.ToLower();
                    purchases = await _context.BookedRecords.Where(br =>
                        br.Booking.AppUser.FirstName.ToLower().Contains(searchString) ||
                        br.Booking.AppUser.LastName.ToLower().Contains(searchString) ||
                        br.Room.Motel.Name.ToLower().Contains(searchString) ||
                        br.Room.RoomType.Name.ToLower().Contains(searchString) ||
                        br.Booking.PayTime.ToString().Contains(searchString) ||
                        br.CheckinDate.ToString().Contains(searchString) ||
                        br.CheckoutDate.ToString().Contains(searchString)
                        )
                        .Where(br => br.Booking.AppUser.Id == id)
                        .ToListAsync();

                    if (purchases.Count <= 0)
                    {
                        TempData["CHNotFound"] = "No related purchase Found!";
                    }
                }
                ViewBag.Count = HttpContext.Session.GetString("Count");
                return View(purchases);
            }

            else
            {
                return RedirectToAction("AccessDenied", "Account");
            }

        }
    }
}

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

        // Current user's BookedRecord history
        public async Task<IActionResult> CustomerHistory(int? id, string searchString)
        {
            var userName = _userManager.GetUserName(User);
            var currentUser = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();

            if (currentUser.Id != id)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (_context.BookedRecords == null) return Problem("No purchases found.");

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

        // All PurchasedFlight 
        //public async Task<IActionResult> AdminIndex(string searchString)
        //{
        //    if (_context.PurchasedFlights == null) return Problem("No purchases found.");
        //    var purchases = await _context.PurchasedFlights
        //        .Include(pf => pf.Flight).ThenInclude(f => f.Airline)
        //        .Include(pf => pf.Flight).ThenInclude(f => f.Origin)
        //        .Include(pf => pf.Flight).ThenInclude(f => f.Destination)
        //        .Include(pf => pf.Purchase).ThenInclude(u => u.User)
        //        .ToListAsync();

        //    if (!String.IsNullOrEmpty(searchString))
        //    {
        //        searchString = searchString.ToLower();
        //        purchases = await _context.PurchasedFlights.Where(pf =>
        //            pf.Purchase.User.UserName.ToLower().Contains(searchString) ||
        //            pf.Flight.Number.ToLower().Contains(searchString) ||
        //            pf.Flight.Airline.AirlineName.ToLower().Contains(searchString) ||
        //            pf.Flight.Origin.Name.ToLower().Contains(searchString) ||
        //            pf.Flight.Destination.Name.ToLower().Contains(searchString) ||
        //            pf.Purchase.WhenPaid.ToString().Contains(searchString) ||
        //            pf.DepartureDate.ToString().Contains(searchString)
        //            )
        //            .ToListAsync();

        //        if (purchases.Count <= 0)
        //        {
        //            TempData["notfound"] = "No related purchase Found!";
        //        }
        //    }
        //    return View(purchases);
        //}

    }
}

using Microsoft.AspNetCore.Mvc;
using MotelBookingApp.Data;
using MotelBookingApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace TravelBooking.Controllers
{
    //[Authorize]
    public class CartController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public List<BookingCart> CartList;

        public CartController(MotelDbContext context, UserManager<AppUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userName = _userManager.GetUserName(User); // userName is email
            var user = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();
            ViewBag.Total = this._context.BookingCarts.Include("Room").Include("AppUser").Where(u => u.AppUser.UserName == userName).Sum(room => (double)room.Room.Price).ToString();
            ViewBag.Count = HttpContext.Session.GetString("Count");
            CartList = _context.BookingCarts.Include("AppUser").Include("Room").Include("Room.RoomType").Where(u => u.AppUser.UserName == userName).ToList();
            return View(CartList);
        }


        public async Task<IActionResult> Remove(int Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            var removeRoom = await _context.BookingCarts
                .FirstOrDefaultAsync(m => m.Id == Id);
            if (removeRoom == null)
            {
                return NotFound();
            }

            _context.BookingCarts.Remove(removeRoom);
            await _context.SaveChangesAsync();

            var userName = _userManager.GetUserName(User); // userName is email
            var count = _context.BookingCarts.Include("AppUser").Where(u => u.AppUser.UserName == userName).ToList().Count.ToString();
            HttpContext.Session.SetString("Count", count);

            TempData["DeleteCartItem"] = "Removed planned room in cart successfully";

            return RedirectToAction("Cart", "Home");
        }
    }
}

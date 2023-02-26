using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotelBookingApp.Data;
using MotelBookingApp.Models;
using MotelBookingApp.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MotelBookingApp.Controllers
{

    public class HomeController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly BlobContainerClient _client;
        
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public HomeController(IConfiguration configuration, MotelDbContext context)
        {
            _context = context;
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
            
            _client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
        }
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.count = "0";
          
            return View();
        }

        [HttpPost,ActionName("Index")]
        public IActionResult LaunchSearch( DateTime checkin, string city, DateTime checkout)
        {
            ViewBag.checkin = checkin.Date.ToString();
 
            if (checkin.ToString().Equals("0001-01-01 12:00:00 AM"))
            {
                TempData["searchOption"] = "Please choose check in date";  
                return View(); 
            }
            if (checkout.ToString().Equals("0001-01-01 12:00:00 AM"))
            {
                TempData["searchOption"] = "Please choose check out date";
                return View();
            }
            if (string.IsNullOrEmpty(city))
            {
                TempData["searchOption"] = "Please choose city";
                return View();
            }
            HttpContext.Session.SetString("city", city);
            HttpContext.Session.SetString("checkin", checkin.ToString());
            HttpContext.Session.SetString("checkout",checkout.ToString());
            ViewBag.count = HttpContext.Session.GetString("count");
            return RedirectToAction(nameof(CityMotelList)); 
        }

        [HttpGet]
        public async Task<IActionResult> CityMotelList()
        {
            
            ViewBag.count = HttpContext.Session.GetString("count");
            var searchCity = HttpContext.Session.GetString("city");
            try
            {
                List<Motel> motels = await _context.Motels.Where(m => m.City == searchCity).ToListAsync();
                if (motels.Count > 0)
                {
                    return View(motels);
                }
                else
                {
                    TempData["searchOption"] = "Sorry,there is no result for your search.";
                    return RedirectToAction(nameof(Index));
                }
            } 
            
            catch (SystemException ex)
            {
                return View();
            }
            
            

        }

        [HttpGet]
        public async Task<IActionResult> CityMotelDetail(int id)
        {
           
            ViewBag.count = HttpContext.Session.GetString("count");
            var motelDetail = new MotelDetailModel();
            var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);
            var comments = await _context.Comments.Include("Motel").Where(c => c.Motel.Id == id).ToListAsync();
            motelDetail.Motel = motel;
            motelDetail.Comments = comments;
            return View(motelDetail );
        }


        [HttpGet]
        public async Task<IActionResult> AddAComment(int id)
        {
           
            var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);
            return View(motel);

        }
        [HttpPost,ActionName("AddAComment")]
        public async Task<IActionResult> AddCommentUpload(int id, string content, string score)
        {
 
            var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);

            var comment = new Comment
            {
                User = new AppUser(),
                Content = content,
                Score = score,
                Motel = motel
            };
      
             _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
           
            return RedirectToAction(nameof(CityMotelDetail), new { id = id });
            

        }

        [HttpGet]
        public async Task<IActionResult> SearchRoomList(int id)
        {
            List<Room> rooms = await _context.Rooms.Include("RoomType").Include("Motel").Where(m => m.Motel.Id == id).ToListAsync();

            return View(rooms);
         
        }


        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var userName = User.Identity.Name;

            List<BookingCart> cartItems = await _context.BookingCarts.Include("Room").Where(bc => bc.AppUser.UserName == userName).ToListAsync();

            return View(cartItems);

        }

        [HttpPost,ActionName("Cart")]
        public async Task<IActionResult> RemoveItem(int id)
        {
            var cartItem = await _context.BookingCarts.Include("Room").Where(bc => bc.Id  == id).FirstOrDefaultAsync();
            _context.BookingCarts.Remove(cartItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Cart));

        }


        [HttpPost,ActionName("SearchRoomList")]
        public async Task<ActionResult> AddToCart(int id)
        {

            Room room = await _context.Rooms.Include("RoomType").Include("Motel").Where(r => r.Id == id).FirstOrDefaultAsync();
            if(room == null)
                return View(new List<Room>());
            BookingCart bookingCart = new BookingCart
            { 
                CheckinDate = DateTime.Parse(HttpContext.Session.GetString("checkin")),
                CheckoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout")),
                Room = room
            };


            _context.BookingCarts.Add(bookingCart);
            await _context.SaveChangesAsync();
            room.IfAvailable = false;
            _context.Rooms.Update(room);
            _context.SaveChanges();
            var motelId = bookingCart.Room.Motel.Id;
            List<Room> allRooms = await _context.Rooms.Include("RoomType").Include("Motel").Where(m => m.Motel.Id == motelId).ToListAsync();
            var curCount = Convert.ToInt32(HttpContext.Session.GetString("count")) + 1;
            HttpContext.Session.SetString("count", curCount.ToString());
            ViewBag.Count = curCount;
            return View(allRooms);
        }


        public IActionResult Privacy()
        {
            return View();
        }


    }
}

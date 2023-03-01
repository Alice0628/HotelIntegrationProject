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
using Microsoft.AspNetCore.Identity;
using System.Runtime.InteropServices;

namespace MotelBookingApp.Controllers
{

    public class HomeController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly BlobContainerClient _client;
        private readonly UserManager<AppUser> _userManager;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public HomeController(IConfiguration configuration, MotelDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
            _userManager = userManager;
            _client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Count = HttpContext.Session.GetString("count");
            TempData["checkin"] = DateTime.Now.AddDays(1);
            TempData["checkout"] = DateTime.Now.AddDays(2);
            return View(); ;
        }

        [HttpPost, ActionName("Index")]
        public IActionResult LaunchSearch(DateTime checkin, string city, DateTime checkout)
        {

            if (string.IsNullOrEmpty(city))
            {
                TempData["searchOption"] = "Please choose city";
                TempData["checkin"] = checkin;
                TempData["checkout"] = checkout;
                return View();
            }
            TempData["city"] = city;

            if (checkin.ToString().Equals("0001-01-01 12:00:00 AM"))
            {
                TempData["searchOption"] = "Please choose check in date";
                TempData["city"] = city;
                TempData["checkout"] = checkout;
                return View();
            }
            TempData["checkin"] = checkin.ToString();
            if (checkout.ToString().Equals("0001-01-01 12:00:00 AM"))
            {
                TempData["searchOption"] = "Please choose check out date";
                TempData["city"] = city;
                TempData["checkin"] = checkin;
                return View();
            }
            TempData["checkout"] = checkout.ToString();
            if (checkin < DateTime.Now || checkout < DateTime.Now || checkout < checkin)
            {
                TempData["searchOption"] = "Please choose valid check in and check out date";
                TempData["city"] = city;
                TempData["checkin"] = checkin;
                TempData["checkout"] = checkout;
                return View();
            }

            HttpContext.Session.SetString("city", city);
            HttpContext.Session.SetString("checkin", checkin.ToString());
            HttpContext.Session.SetString("checkout", checkout.ToString());
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
                    var motelList = new List<MotelInputModel>();
                    foreach (var motel in motels)
                    {
                        string blobUrl = _client.Uri.ToString();
                        MotelInputModel newMotel = new MotelInputModel
                        {
                            Id = motel.Id,
                            Name = motel.Name,
                            Address = motel.Address,
                            Province = motel.Province,
                            City = motel.City,
                            PostalCode = motel.PostalCode,
                            ImageUrl = blobUrl + "/" + motel.ImageUrl
                        };
                        motelList.Add(newMotel);
                    }
                    return View(motelList);
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
            string blobUrl = _client.Uri.ToString();
            MotelInputModel newMotel = new MotelInputModel
            {
                Id = motel.Id,
                Name = motel.Name,
                Address = motel.Address,
                Province = motel.Province,
                City = motel.City,
                PostalCode = motel.PostalCode,
                ImageUrl = blobUrl + "/" + motel.ImageUrl
            };
            if (_context.Comments != null)
            {
                var comments = await _context.Comments.Include("Motel").Include("User").Where(c => c.Motel.Id == id).ToListAsync();
                motelDetail.Comments = comments;
            }

            motelDetail.Motel = newMotel;

            return View(motelDetail);
        }


        [HttpGet]
        public async Task<IActionResult> AddAComment(int id)
        {
            var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);
            return View(motel);
        }

        [HttpGet]
        public async Task<IActionResult> EditComment(int id)
        {
            var comment = await _context.Comments.Include("User").Include("Motel").FirstOrDefaultAsync(c => c.Id == id);
            return View(comment);
        }


        [HttpPost, ActionName("EditComment")]
        public async Task<IActionResult> EditCommentUpload(int id, Comment comment)
        {
            var curComment = await _context.Comments.Include("User").Include("Motel").FirstOrDefaultAsync(c => c.Id == id);
            curComment.Content = comment.Content;
            curComment.Score = comment.Score;
            _context.Comments.Update(curComment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CityMotelDetail), new { id = curComment.Motel.Id });


        }


        [HttpPost, ActionName("AddAComment")]
        public async Task<IActionResult> AddCommentUpload(int id, string content, int score)
        {

            var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);
            var userName = _userManager.GetUserName(User);
            var user = await _context.Users.Where(u => u.UserName == userName).FirstOrDefaultAsync();

            var comment = new Comment
            {
                User = user,
                Content = content,
                Score = score.ToString(),
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
            //string checkinStr = HttpContext.Session.GetString("checkin");
            //string checkoutStr = HttpContext.Session.GetString("checkout");
            //if (string.IsNullOrEmpty(checkinStr) || string.IsNullOrEmpty(checkoutStr)) {
            //    return RedirectToAction("Index", "Home");
            //}
            //var checkinDate = DateTime.Parse(HttpContext.Session.GetString("checkin"));
            //var checkoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout"));
            //foreach (var room in rooms)
            //{
            //    var bookedRoom = await _context.BookedRecords.Include("Room").Where(br => br.Room.Id == id && ((br.CheckinDate > checkinDate && br.CheckinDate < checkoutDate) || (br.CheckoutDate > checkinDate && br.CheckoutDate < checkoutDate))).FirstOrDefaultAsync();
            //    if (bookedRoom != null)
            //    {
            //        room.IfAvailable = false;
            //    }
            //}
            // use room input model
            return View(rooms);

        }


        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var userName = _userManager.GetUserName(User);
            List<BookingCart> cartItems = await _context.BookingCarts.Include("AppUser").Include("Room").Where(bc => bc.AppUser.UserName == userName).ToListAsync();

            ViewBag.Total = 0;
            ViewBag.Count = 0;

            foreach (var cartItem in cartItems)
            {
                ViewBag.Total += cartItem.Room.Price;
                ViewBag.Count++;
            }

            return View(cartItems);

        }

        [HttpPost, ActionName("Cart")]
        public async Task<IActionResult> RemoveItem(int id)
        {
            var cartItem = await _context.BookingCarts.Include("AppUser").Include("Room").Where(bc => bc.Id == id).FirstOrDefaultAsync();
            _context.BookingCarts.Remove(cartItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Cart));

        }


        [HttpPost, ActionName("Add")]
        public async Task<ActionResult> AddToCart(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            Room room = await _context.Rooms.Include("RoomType").Include("Motel").Where(r => r.Id == id).FirstOrDefaultAsync();
          

            BookingCart bookingCart = new BookingCart
            {
                AppUser = user,
                CheckinDate = DateTime.Parse(HttpContext.Session.GetString("checkin")),
                CheckoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout")),
                Room = room,
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
            //return View(allRooms);
            return RedirectToAction("Cart", "Home");

        }


        public IActionResult Privacy()
        {
            return View();

        }
    }

}
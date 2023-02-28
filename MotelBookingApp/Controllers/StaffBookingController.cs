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

    public class StaffBookingController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly BlobContainerClient _client;
        private readonly UserManager<AppUser> _userManager;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public StaffBookingController(IConfiguration configuration, MotelDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
            _userManager = userManager;
            _client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            StaffBookingVM searchModel = new StaffBookingVM();
            if(HttpContext.Session.GetString("count") == null)
            {
               HttpContext.Session.SetString("count", "0");
            }
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            searchModel.CheckinDate = DateTime.Now;
            searchModel.CheckoutDate = DateTime.Now;
            searchModel.RoomTypeList = roomTypeList;
            return View(searchModel);
        }

        [HttpPost, ActionName("Index")]
        public async Task<IActionResult> LaunchSearch( DateTime checkin, DateTime checkout,string roomType)
        {
            StaffBookingVM searchModel = new StaffBookingVM();
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            searchModel.RoomTypeList = roomTypeList;
            searchModel.CheckinDate = checkin;
            searchModel.CheckoutDate = checkout;
            searchModel.SearchType = roomType;
            //searchModel.City = roomType;

            //if (string.IsNullOrEmpty(city))
            //{
            //    TempData["searchOption"] = "Please input a city";
            //    return View(searchModel);
            //}
            
            if (searchModel.CheckinDate < DateTime.Now || searchModel.CheckoutDate < DateTime.Now || searchModel.CheckoutDate < searchModel.CheckinDate)
            {
                TempData["searchOption"] = "Please choose valid check in and check out date";
                return View(searchModel);
            }

            //HttpContext.Session.SetString("city", searchModel.City);
            HttpContext.Session.SetString("checkin", searchModel.CheckinDate.ToString());
            HttpContext.Session.SetString("checkout", searchModel.CheckoutDate.ToString());
            if (searchModel.CheckoutDate != null)
            {
                HttpContext.Session.SetString("roomtype", searchModel.CheckoutDate.ToString());
            }
           
            //HttpContext.Session.SetString("occupNum", occupNum.ToString());
            ViewBag.count = HttpContext.Session.GetString("count");
            try
            {
                //List<Motel> motels = await _context.Motels.Where(m => m.City == searchCity).ToListAsync();
                var userName = _userManager.GetUserName(User);
                var user = await _context.Users.Where(u => u.UserName == userName).FirstOrDefaultAsync();
                List<Room> rooms = await _context.Rooms.Include("Motel").Include("RoomType").Where(r => r.Motel.Name.ToLower()  ==  user.Motel.Name.ToLower()).ToListAsync();
                if (rooms.Count > 0)
                {
                    var roomList = new List<RoomInputModel>();
                    foreach (var room in rooms)
                    {
                        string blobUrl = _client.Uri.ToString();
                        RoomInputModel newRoom = new RoomInputModel
                        {
                            Id = room.Id,
                            RoomNum = room.RoomNum,
                            Price = room.Price,
                            MotelName = room.Motel.Name,
                            RoomTypeName = room.RoomType.Name,
                            RoomTypeImage = _client.Uri.ToString() + "/" + room.RoomType.ImageUrl
                        };
                        roomList.Add(newRoom);
                    }
                    searchModel.AvailableRooms = roomList;
                    return View(searchModel);
                }
                else
                {
                    TempData["searchOption"] = "Sorry,there is no result for your search.";
                    return View(searchModel);
                }
            }

            catch (SystemException ex)
            {
                return View(searchModel);
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
            var checkinDate = DateTime.Parse(HttpContext.Session.GetString("checkin"));
            var checkoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout"));
            List<Room> rooms = new List<Room>();

            if (HttpContext.Session.GetString("roomtype") != null)
            { 
                int roomTypeIndex = int.Parse(HttpContext.Session.GetString("roomtype"));
                var curType = await _context.RoomTypes.Where(rt => rt.Id == roomTypeIndex).FirstOrDefaultAsync();
                rooms = await _context.Rooms.Include("RoomType").Include("Motel").Where(m => m.Motel.Id == id && m.RoomType.Name == curType.Name).ToListAsync();
            }
            else
            {
                rooms = await _context.Rooms.Include("RoomType").Include("Motel").Where(m => m.Motel.Id == id).ToListAsync();
            }

            foreach (var room in rooms)
                {
                var bookedRoom = await _context.BookedRecords.Include("Room").Where(br => br.Room.Id == id && ((br.CheckinDate > checkinDate && br.CheckinDate < checkoutDate) || (br.CheckoutDate > checkinDate && br.CheckoutDate < checkoutDate))).FirstOrDefaultAsync();
                var cartRoom = await _context.BookingCarts.Include("Room").Where(bc => bc.Room.Id == id && ((bc.CheckinDate > checkinDate && bc.CheckinDate < checkoutDate) || (bc.CheckoutDate > checkinDate && bc.CheckoutDate < checkoutDate))).FirstOrDefaultAsync();
                if (bookedRoom != null || cartRoom != null)
                {
                    rooms.Remove(room);
                }
            }
            var displayRooms = new List<RoomInputModel>();
            string blobUrl = _client.Uri.ToString();
            foreach (var r in rooms)
            {
                RoomInputModel newRoom = new RoomInputModel
                {
                    Id = r.Id,
                    RoomNum = r.RoomNum,
                    Price = r.Price,
                    MotelName = r.Motel.Name,
                    RoomTypeName = r.RoomType.Name,
                    RoomTypeImage = _client.Uri.ToString() + "/" + r.RoomType.ImageUrl
                };
                displayRooms.Add(newRoom);
            }
            return View(displayRooms);
        }


        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var userName = User.Identity.Name;

            List<BookingCart> cartItems = await _context.BookingCarts.Include("Room").Where(bc => bc.AppUser.UserName == userName).ToListAsync();

            return View(cartItems);

        }

        [HttpPost, ActionName("Cart")]
        public async Task<IActionResult> RemoveItem(int id)
        {
            var cartItem = await _context.BookingCarts.Include("Room").Where(bc => bc.Id == id).FirstOrDefaultAsync();
            _context.BookingCarts.Remove(cartItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Cart));

        }




        [HttpPost, ActionName("SearchRoomList")]
        public async Task<ActionResult> AddToCart(int id)
        {

            Room room = await _context.Rooms.Include("RoomType").Include("Motel").Where(r => r.Id == id).FirstOrDefaultAsync();
            if (room == null)
                return View(new List<Room>());
            BookingCart bookingCart = new BookingCart
            {
                CheckinDate = DateTime.Parse(HttpContext.Session.GetString("checkin")),
                CheckoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout")),
                Room = room
            };


            _context.BookingCarts.Add(bookingCart);
            await _context.SaveChangesAsync();
           
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

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
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs.Models;

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
            ViewBag.count = HttpContext.Session.GetString("count");
            if (HttpContext.Session.GetString("checkin") == null)
            {
                HttpContext.Session.SetString("count", "0");
                var roomTypeList = await _context.RoomTypes.ToListAsync();
                searchModel.CheckinDate = DateTime.Now;
                searchModel.CheckoutDate = DateTime.Now;
                searchModel.RoomTypeList = roomTypeList;
                return View(searchModel);
            }
            else
            {
                ViewBag.CheckinDate = HttpContext.Session.GetString("checkin");
                ViewBag.RoomType = HttpContext.Session.GetString("roomType");
                var roomTypeList = await _context.RoomTypes.ToListAsync();
                searchModel.RoomTypeList = roomTypeList;
                searchModel.CheckinDate = DateTime.Parse(HttpContext.Session.GetString("checkin"));
                searchModel.CheckoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout"));
                searchModel.RoomTypeName = HttpContext.Session.GetString("roomType");
                var availableRooms = GetRooms(searchModel.CheckinDate, searchModel.CheckoutDate, searchModel.RoomTypeName).Result;
                searchModel.AvailableRooms = availableRooms;
                return View(searchModel);
            }
        }

        public async Task<List<RoomInputModel>> GetRooms(DateTime? checkin, DateTime? checkout, string? roomType)
        {

            try
            {
                //List<Motel> motels = await _context.Motels.Where(m => m.City == searchCity).ToListAsync();
                var userName = _userManager.GetUserName(User);
                var user = await _context.Users.Include("Motel").Where(u => u.UserName == userName).FirstOrDefaultAsync();
                List<Room> rooms = await _context.Rooms.Include("Motel").Include("RoomType").Where(r => r.Motel.Name == user.Motel.Name && r.RoomType.Name == roomType).ToListAsync();
                ViewBag.RoomMotel = user.Motel.Name;
                var unavailableList = new List<Room>();
                if (rooms.Count > 0)
                {
                    foreach (var r in rooms)
                    {
                        var bookedroom = await _context.BookedRecords.Include("Room").Where(br => br.Room.Id == r.Id && ((br.CheckinDate >= checkin && br.CheckinDate <= checkout) || (br.CheckoutDate >= checkin && br.CheckoutDate <= checkout))).FirstOrDefaultAsync();
                        var bookingroom = await _context.BookingCarts.Include("Room").Where(bc => bc.Room.Id == r.Id && ((bc.CheckinDate >= checkin && bc.CheckinDate <= checkout) || (bc.CheckoutDate >= checkin && bc.CheckoutDate <= checkout))).FirstOrDefaultAsync();
                        if (bookedroom != null || bookingroom != null)
                        {
                            unavailableList.Add(r);
                        }
                    }
                    foreach (var ur in unavailableList)
                    {
                        rooms.Remove(ur);
                    }
                    var roomList = new List<RoomInputModel>();
                    if (rooms.Count > 0)
                    {
                        foreach (var room in rooms)
                        {
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
                        return roomList;
                    }
                    else
                    {
                       
                        return null;
                    }

                }
                else
                {
                    
                    return null;
                }
            }

            catch (SystemException ex)
            {
                TempData["searchOption"] = ex.Message;
                return null;
            }
        }


        [HttpPost, ActionName("Index")]
        public async Task<IActionResult> LaunchSearch(DateTime checkin, DateTime checkout, string roomType)
        {  
            HttpContext.Session.SetString("checkin", checkin.ToString());
            HttpContext.Session.SetString("checkout", checkout.ToString());
            HttpContext.Session.SetString("roomType", roomType);
            StaffBookingVM searchModel = new StaffBookingVM();
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            searchModel.RoomTypeList = roomTypeList;
            searchModel.CheckinDate = checkin;
            searchModel.CheckoutDate = checkout;
            searchModel.SearchType = roomType;
            

            if (searchModel.CheckinDate < DateTime.Now || searchModel.CheckoutDate < DateTime.Now || searchModel.CheckoutDate < searchModel.CheckinDate)
            {
                TempData["searchOption"] = "Please choose valid check in and check out date";
                return View(searchModel);
            }

            //HttpContext.Session.SetString("city", searchModel.City);


            var availableRooms = GetRooms(checkin, checkout, roomType).Result;
            if (availableRooms != null) { 
            searchModel.AvailableRooms = availableRooms;
            }
            else
            {
                TempData["searchOption"] = "Sorry,there is no result for your search.";
            }
            return View(searchModel);

        }
        //HttpContext.Session.SetString("occupNum", occupNum.ToString());



        //[HttpGet]
        //public async Task<IActionResult> CityMotelDetail(int id)
        //{

        //    ViewBag.count = HttpContext.Session.GetString("count");
        //    var motelDetail = new MotelDetailModel();
        //    var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);
        //    string blobUrl = _client.Uri.ToString();
        //    MotelInputModel newMotel = new MotelInputModel
        //    {
        //        Id = motel.Id,
        //        Name = motel.Name,
        //        Address = motel.Address,
        //        Province = motel.Province,
        //        City = motel.City,
        //        PostalCode = motel.PostalCode,
        //        ImageUrl = blobUrl + "/" + motel.ImageUrl
        //    };
        //    if (_context.Comments != null)
        //    {
        //        var comments = await _context.Comments.Include("Motel").Include("User").Where(c => c.Motel.Id == id).ToListAsync();
        //        motelDetail.Comments = comments;
        //    }

        //    motelDetail.Motel = newMotel;

        //    return View(motelDetail);
        //}



        //[HttpGet]
        //public async Task<IActionResult> CityMotelDetail(int id)
        //{

        //    ViewBag.count = HttpContext.Session.GetString("count");
        //    var motelDetail = new MotelDetailModel();
        //    var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);
        //    string blobUrl = _client.Uri.ToString();
        //    MotelInputModel newMotel = new MotelInputModel
        //    {
        //        Id = motel.Id,
        //        Name = motel.Name,
        //        Address = motel.Address,
        //        Province = motel.Province,
        //        City = motel.City,
        //        PostalCode = motel.PostalCode,
        //        ImageUrl = blobUrl + "/" + motel.ImageUrl
        //    };
        //    if (_context.Comments != null)
        //    {
        //        var comments = await _context.Comments.Include("Motel").Include("User").Where(c => c.Motel.Id == id).ToListAsync();
        //        motelDetail.Comments = comments;
        //    }

        //    motelDetail.Motel = newMotel;

        //    return View(motelDetail);
        //}


        //[HttpGet]
        //public async Task<IActionResult> AddAComment(int id)
        //{
        //    var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);
        //    return View(motel);
        //}

        //[HttpGet]
        //public async Task<IActionResult> EditComment(int id)
        //{
        //    var comment = await _context.Comments.Include("User").Include("Motel").FirstOrDefaultAsync(c => c.Id == id);
        //    return View(comment);
        //}


        //[HttpPost, ActionName("EditComment")]
        //public async Task<IActionResult> EditCommentUpload(int id, Comment comment)
        //{
        //    var curComment = await _context.Comments.Include("User").Include("Motel").FirstOrDefaultAsync(c => c.Id == id);
        //    curComment.Content = comment.Content;
        //    curComment.Score = comment.Score;
        //    _context.Comments.Update(curComment);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(CityMotelDetail), new { id = curComment.Motel.Id });


        //}


        //[HttpPost, ActionName("AddAComment")]
        //public async Task<IActionResult> AddCommentUpload(int id, string content, int score)
        //{

        //    var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);
        //    var userName = _userManager.GetUserName(User);
        //    var user = await _context.Users.Where(u => u.UserName == userName).FirstOrDefaultAsync();

        //    var comment = new Comment
        //    {
        //        User = user,
        //        Content = content,
        //        Score = score.ToString(),
        //        Motel = motel
        //    };

        //    _context.Comments.Add(comment);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(CityMotelDetail), new { id = id });
        //}

        //[HttpGet]
        //public async Task<IActionResult> SearchRoomList(int id)
        //{   
        //    var checkinDate = DateTime.Parse(HttpContext.Session.GetString("checkin"));
        //    var checkoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout"));
        //    List<Room> rooms = new List<Room>();

        //    if (HttpContext.Session.GetString("roomtype") != null)
        //    { 
        //        int roomTypeIndex = int.Parse(HttpContext.Session.GetString("roomtype"));
        //        var curType = await _context.RoomTypes.Where(rt => rt.Id == roomTypeIndex).FirstOrDefaultAsync();
        //        rooms = await _context.Rooms.Include("RoomType").Include("Motel").Where(m => m.Motel.Id == id && m.RoomType.Name == curType.Name).ToListAsync();
        //    }
        //    else
        //    {
        //        rooms = await _context.Rooms.Include("RoomType").Include("Motel").Where(m => m.Motel.Id == id).ToListAsync();
        //    }

        //    foreach (var room in rooms)
        //        {
        //        var bookedRoom = await _context.BookedRecords.Include("Room").Where(br => br.Room.Id == id && ((br.CheckinDate > checkinDate && br.CheckinDate < checkoutDate) || (br.CheckoutDate > checkinDate && br.CheckoutDate < checkoutDate))).FirstOrDefaultAsync();
        //        var cartRoom = await _context.BookingCarts.Include("Room").Where(bc => bc.Room.Id == id && ((bc.CheckinDate > checkinDate && bc.CheckinDate < checkoutDate) || (bc.CheckoutDate > checkinDate && bc.CheckoutDate < checkoutDate))).FirstOrDefaultAsync();
        //        if (bookedRoom != null || cartRoom != null)
        //        {
        //            rooms.Remove(room);
        //        }
        //    }
        //    var displayRooms = new List<RoomInputModel>();
        //    string blobUrl = _client.Uri.ToString();
        //    foreach (var r in rooms)
        //    {
        //        RoomInputModel newRoom = new RoomInputModel
        //        {
        //            Id = r.Id,
        //            RoomNum = r.RoomNum,
        //            Price = r.Price,
        //            MotelName = r.Motel.Name,
        //            RoomTypeName = r.RoomType.Name,
        //            RoomTypeImage = _client.Uri.ToString() + "/" + r.RoomType.ImageUrl
        //        };
        //        displayRooms.Add(newRoom);
        //    }
        //    return View(displayRooms);
        //}

        [HttpGet]
        public async Task<IActionResult> RoomDetail(int id)
        {
            var room = await _context.Rooms.Include("Motel").Include("RoomType").FirstOrDefaultAsync(r => r.Id == id);
            RoomInputModel newRoom = new RoomInputModel
            {
                Id = room.Id,
                RoomNum = room.RoomNum,
                Price = room.Price,
                MotelName = room.Motel.Name,
                RoomType = room.RoomType.Id,
                RoomTypeImage = _client.Uri.ToString() + "/" + room.RoomType.ImageUrl,
                RoomTypeDescription = room.RoomType.Description
            };
            return View(newRoom);

        }


        [HttpPost, ActionName("RoomDetail")]
        public async Task<IActionResult> AddToCart(int id)
        {
            DateTime checkinDate = DateTime.Parse(HttpContext.Session.GetString("checkin"));
            DateTime checkoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout"));
            try
            {
                
                Room room = await _context.Rooms.Include("RoomType").Include("Motel").Where(r => r.Id == id).FirstOrDefaultAsync();
                if (room == null)
                {
                    TempData["addtocart"] = "no room found";
                    return View();
                }

                var userName = _userManager.GetUserName(User);
                var user = await _context.Users.Include("Motel").Where(u => u.UserName == userName).FirstOrDefaultAsync();
                BookingCart bookingCart = new BookingCart
                {
                    AppUser = user,
                    CheckinDate = checkinDate,
                    CheckoutDate = checkoutDate,
                    Room = room
                };

                _context.BookingCarts.Add(bookingCart);
                await _context.SaveChangesAsync();
                var count = int.Parse(HttpContext.Session.GetString("count")) + 1;
                HttpContext.Session.SetString("count", count.ToString());
                TempData["addtocart"] = $"Room {room.RoomNum} has been added to cart";
                return RedirectToAction(nameof(Index));
            }
            catch (SystemException ex)
            {
                TempData["addtocart"] = ex.Message;
                return View();

            }

        }


        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            ViewBag.count = HttpContext.Session.GetString("count");
            var userName = _userManager.GetUserName(User);

            List<BookingCart> cartItems = await _context.BookingCarts.Include("AppUser").Include("Room").Where(bc => bc.AppUser.UserName == userName).ToListAsync();

            decimal subTotal = 0;
            foreach (var cartItem in cartItems)
            {
                subTotal = subTotal + cartItem.Room.Price * (cartItem.CheckoutDate - cartItem.CheckinDate).Days;
            }
            var tax = ((double)subTotal) * 0.15;
            ViewBag.tax = tax.ToString("0.00");
            var Total = ((double)subTotal + tax).ToString("0.00");
            ViewBag.Total = Total;
            return View(cartItems);

        }





        [HttpPost, ActionName("Cart")]
        public async Task<IActionResult> RemoveItem(int id)
        {
            var cartItem = await _context.BookingCarts.Where(bc => bc.Id == id).FirstOrDefaultAsync();
            if (cartItem == null)
            {
                return View();
            }
            _context.BookingCarts.Remove(cartItem);
            await _context.SaveChangesAsync();
            var count = int.Parse(HttpContext.Session.GetString("count")) - 1;
            HttpContext.Session.SetString("count", count.ToString());
            ViewBag.count = count.ToString();
            return View();

        }




        //[HttpPost,ActionName("Index")]
        //public async Task<IActionResult> AddDirectly(int id)
        //{
        //    TempData["addtocart"] = "here you are";
        //    try
        //    {
        //        ViewBag.count = 1;
        //        Room room = await _context.Rooms.Include("RoomType").Include("Motel").Where(r => r.Id == id).FirstOrDefaultAsync();
        //        if (room == null)
        //            return View();
        //        var userName = _userManager.GetUserName(User);
        //        var user = await _context.Users.Include("Motel").Where(u => u.UserName == userName).FirstOrDefaultAsync();
        //        BookingCart bookingCart = new BookingCart
        //        {
        //            AppUser = user,
        //            CheckinDate = DateTime.Parse(HttpContext.Session.GetString("checkin")),
        //            CheckoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout")),
        //            Room = room
        //        };
        //        _context.BookingCarts.Add(bookingCart);
        //        await _context.SaveChangesAsync();

        //        TempData["addtocart"] = $"Room {room.RoomNum} has been added to cart";
        //        return View();
        //    }
        //    catch(SystemException ex)
        //    {
        //        TempData["addtocart"] = ex.Message;
        //        return View();
        //    }
        //return View()

        //var motelId = bookingCart.Room.Motel.Id;
        //List<Room> allRooms = await _context.Rooms.Include("RoomType").Include("Motel").Where(m => m.Motel.Id == motelId).ToListAsync();
        //var curCount = Convert.ToInt32(HttpContext.Session.GetString("count")) + 1;
        //HttpContext.Session.SetString("count", curCount.ToString());
        //ViewBag.Count = curCount;
        //return View(allRooms);
        //}


     
    }
}



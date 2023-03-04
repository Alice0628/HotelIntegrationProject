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
using Azure.Identity;


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
        public async Task<IActionResult> Index()
        {
            if(HttpContext.Session.GetString("Count") ==null)
            {
                HttpContext.Session.SetString("Count", "0");
            }
            ViewBag.Count = HttpContext.Session.GetString("Count");
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            ViewBag.RoomTypeList = roomTypeList;
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("city"))){
                ViewBag.city = HttpContext.Session.GetString("city");
            }
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("checkin")))
            {
                ViewBag.checkin = HttpContext.Session.GetString("checkin");
            }
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("checkout")))
            {
                ViewBag.checkout = HttpContext.Session.GetString("checkout");
            }
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("roomType")))
            {
                ViewBag.roomType = HttpContext.Session.GetString("roomType");
            }
            return View(); ;
        }


        [HttpPost, ActionName("Index")]
        public async Task<IActionResult> LaunchSearch(DateTime checkin, string city, DateTime checkout, string roomType)
        {
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            ViewBag.RoomTypeList = roomTypeList;
            ViewBag.Count = HttpContext.Session.GetString("Count");
            if (string.IsNullOrEmpty(city))
            {
                TempData["searchOption"] = "Please choose city";
                return View();
            }
            else { 
            HttpContext.Session.SetString("city", city);
            ViewBag.city = HttpContext.Session.GetString("city");
            }
            if (checkin.ToString().Equals("0001-01-01 12:00:00 AM"))
            {
                TempData["searchOption"] = "Please choose check in date";
                return View();
            }
            else { 
            HttpContext.Session.SetString("checkin", checkin.ToString("yyyy-MM-dd"));
            ViewBag.checkin = HttpContext.Session.GetString("checkin");
            }
            if (checkout.ToString().Equals("0001-01-01 12:00:00 AM"))
            {
                TempData["searchOption"] = "Please choose check out date";
                return View();
            }
            else
            {
                HttpContext.Session.SetString("checkout", checkout.ToString("yyyy-MM-dd"));
                ViewBag.checkout = HttpContext.Session.GetString("checkout");
            }
            if (string.IsNullOrEmpty(roomType))
            {
                TempData["searchOption"] = "Please choose roomType";
                return View();
            }
            else
            {
                HttpContext.Session.SetString("roomType", roomType);
                ViewBag.roomType = HttpContext.Session.GetString("roomType");
            }
            if (checkin < DateTime.Now || checkout < DateTime.Now || checkout < checkin)
            {
                TempData["searchOption"] = "Please choose valid check in and check out date";
                return View();
            }
            return RedirectToAction(nameof(CityMotelList));

        }

        [HttpGet]
        public async Task<IActionResult> CityMotelList()
        {
            ViewBag.city = HttpContext.Session.GetString("city");
            ViewBag.roomType = HttpContext.Session.GetString("roomType");
            ViewBag.checkin = HttpContext.Session.GetString("checkin");
            ViewBag.checkout = HttpContext.Session.GetString("checkout");
            ViewBag.Count = HttpContext.Session.GetString("Count");
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            ViewBag.RoomTypeList = roomTypeList;
            try
            {
                List<Motel> motels = await _context.Motels.Where(m => m.City == HttpContext.Session.GetString("city")).ToListAsync();
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
                    TempData["searchResOption"] = "Sorry,there is no result for your search.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (SystemException ex)
            {
                return View();
            }
        }

        [HttpPost,ActionName("CityMotelList")]
        public async Task<IActionResult> ReSearch(DateTime checkin, string city, DateTime checkout, string roomType)
        {
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            ViewBag.RoomTypeList = roomTypeList;
            ViewBag.Count = HttpContext.Session.GetString("Count");
            if (string.IsNullOrEmpty(city))
            {
                TempData["searchOption"] = "Please choose city";
                return View();
            }
            else
            {
                HttpContext.Session.SetString("city", city);
                ViewBag.city = HttpContext.Session.GetString("city");
            }
            if (checkin.ToString().Equals("0001-01-01 12:00:00 AM"))
            {
                TempData["searchOption"] = "Please choose check in date";
                return View();
            }
            else
            {
                HttpContext.Session.SetString("checkin", checkin.ToString("yyyy-MM-dd"));
                ViewBag.checkin = HttpContext.Session.GetString("checkin");
            }
            if (checkout.ToString().Equals("0001-01-01 12:00:00 AM"))
            {
                TempData["searchOption"] = "Please choose check out date";
                return View();
            }
            else
            {
                HttpContext.Session.SetString("checkout", checkout.ToString("yyyy-MM-dd"));
                ViewBag.checkout = HttpContext.Session.GetString("checkout");
            }
            if (string.IsNullOrEmpty(roomType))
            {
                TempData["searchOption"] = "Please choose roomType";
                return View();
            }
            else
            {
                HttpContext.Session.SetString("roomType", roomType);
                ViewBag.roomType = HttpContext.Session.GetString("roomType");
            }
            if (checkin < DateTime.Now || checkout < DateTime.Now || checkout < checkin)
            {
                TempData["searchOption"] = "Please choose valid check in and check out date";
                return View();
            }
            try
            {
                List<Motel> motels = await _context.Motels.Where(m => m.City == HttpContext.Session.GetString("city")).ToListAsync();
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
                    TempData["searchResOption"] = "Sorry,there is no result for your search.";
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

            ViewBag.Count = HttpContext.Session.GetString("Count");
            var motelDetail = new MotelDetailModel();
            var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);
            string blobUrl = _client.Uri.ToString();
            var userName = _userManager.GetUserName(User);
            var favMotel = await _context.FavoriteMotelLists.Include("Motel").Include("Owner").Where(fm => fm.Motel.Id == id && fm.Owner.UserName == userName).FirstOrDefaultAsync();
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
            if(favMotel != null)
            {
                newMotel.IfFaivorite = true;
            }
            if (await _context.Comments.Include("Motel").Where(c => c.Motel.Id == id).ToListAsync() != null)
            {
                var comments = await _context.Comments.Include("Motel").Include("User").Where(c => c.Motel.Id == id).ToListAsync();
                motelDetail.Comments = comments;
            }

            motelDetail.Motel = newMotel;

            return View(motelDetail);
        }

        [HttpGet]
        public async Task<IActionResult> RemoveComment(int id)
        {
           
                var comment = await _context.Comments.Include("User").Include("Motel").Where(c => c.Id == id).FirstOrDefaultAsync();
                
                return View(comment);
                //return RedirectToAction(nameof(CityMotelDetail), new { id = id2 });
            
        }



        [HttpPost,ActionName("RemoveComent")]
        public async Task<IActionResult> RemoveCommentConfirm(int id1)
        {
            try { 
            var comment = await _context.Comments.Include("AppUser").Include("Motel").Where(c => c.Id == id1).FirstOrDefaultAsync();
                _context.Comments.Remove(comment);
                return View();
                //return RedirectToAction(nameof(CityMotelDetail), new { id =id2 });
            }
            catch (SystemException ex)
            {
                TempData["removeComment"] = ex.Message;
                return View();
                //return RedirectToAction(nameof(CityMotelDetail), new { id = id2 });
            }
        }

        [Authorize(Roles = "User")]
        [HttpPost,ActionName("CityMotelDetail")]
        public async Task<IActionResult> FavOperate(int id)
        {
            try
            {
                var userName = _userManager.GetUserName(User);

                var motel = await _context.Motels.Where(m => m.Id == id).FirstOrDefaultAsync();
                var favMotel = await _context.FavoriteMotelLists.Include("Owner").Include("Motel").Where(fm => fm.Motel.Id == id && fm.Owner.UserName == userName).FirstOrDefaultAsync();
                if (favMotel != null) 
                {   
                    _context.FavoriteMotelLists.Remove(favMotel);
                   
                }
                else
                {
                    var user = await _context.Users.Where(u => u.UserName == userName).FirstOrDefaultAsync();
                    FavoriteMotelList newfavMotel = new FavoriteMotelList
                    {
                        Owner = user,
                        Motel = motel
                    };
                    _context.FavoriteMotelLists.Add(newfavMotel);
                    
                }
                await _context.SaveChangesAsync();
               
                return RedirectToAction(nameof(CityMotelDetail), new { id = id });
            }
            catch (SystemException ex)
            {
                TempData["removeFav"] = ex.Message;
        
                return RedirectToAction(nameof(CityMotelDetail), new { id = id });
            }
        }




        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> AddAComment(int id)
        {
            var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);
            return View(motel);
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> EditComment(int id)
        {
            var comment = await _context.Comments.Include("User").Include("Motel").FirstOrDefaultAsync(c => c.Id == id);
            return View(comment);
        }

        [Authorize(Roles = "User")]
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

        [Authorize(Roles = "User")]
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
            ViewBag.Count = HttpContext.Session.GetString("Count");
            var checkin = DateTime.Parse(HttpContext.Session.GetString("checkin"));
            var checkout = DateTime.Parse(HttpContext.Session.GetString("checkout"));
            List<Room> rooms = await _context.Rooms.Include("RoomType").Include("Motel").Where(r => r.Motel.Id == id && r.RoomType.Name == HttpContext.Session.GetString("roomType")).ToListAsync();
            try
            {
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
                        return View(roomList);
                    }
                    else
                    {
                        TempData["roomRes"] = "Sorry,no available rooms";
                        return View();
                    }

                }
                else
                {
                    TempData["roomRes"] = "Sorry,no available rooms";
                    return View();
                }
            }

            catch (SystemException ex)
            {
                TempData["searchOption"] = ex.Message;
                return View();
            }
        }


        [HttpGet]
        public async Task<IActionResult> RoomDetail(int id)
        {
            ViewBag.Count = HttpContext.Session.GetString("Count");
            ViewBag.Count = HttpContext.Session.GetString("Count");
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

        [Authorize(Roles = "User,Staff")]
        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            ViewBag.Count = HttpContext.Session.GetString("Count");
            var userName = _userManager.GetUserName(User);

            List<BookingCart> cartItems = await _context.BookingCarts.Include("AppUser").Include("Room").Where(bc => bc.AppUser.UserName == userName).ToListAsync();

            decimal subTotal = 0;
            foreach (var cartItem in cartItems)
            {
                subTotal = subTotal + cartItem.Room.Price * (cartItem.CheckoutDate - cartItem.CheckinDate).Days;
            }
            ViewBag.Total = subTotal;
            return View(cartItems);
        }

        [Authorize(Roles = "User,Staff")]
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
            return RedirectToAction(nameof(Cart));
        }

        [Authorize(Roles = "User")]
        [HttpPost, ActionName("RoomDetail")]
        public async Task<ActionResult> AddToCart(int id)
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
                var count = int.Parse(HttpContext.Session.GetString("Count")) + 1;
                HttpContext.Session.SetString("Count", count.ToString());
                TempData["addtocart"] = $"Room {room.RoomNum} has been added to cart";
                var motelId = room.Motel.Id;
                return RedirectToAction("SearchRoomList", new { id = motelId });
            }
            catch (SystemException ex)
            {
                TempData["addtocart"] = ex.Message;
                return View();

            }
        }


        [Authorize(Roles = "User,Staff")]
        [HttpGet]
        public async Task<IActionResult> FavoriteMotelList(int id)
        {
            ViewBag.Count = HttpContext.Session.GetString("Count");
            var favoriteMotelList = await _context.FavoriteMotelLists.Include("Owner").Include("Motel").Where(fm => fm.Owner.Id == id).ToListAsync();  
            var motelList = new List<Motel>();
            foreach (var fm in favoriteMotelList)
            {
                motelList.Add(fm.Motel);
            }
            return View(motelList);
        }

        // todo
        [Authorize(Roles = "User,Staff")]
        [HttpPost,ActionName("DeletFavmotel")]
        public async Task<IActionResult> DeleteFavoriteMote(int id)
        {   
            var userName = _userManager.GetUserName(User); 
            var user = await _context.Users.Where(u => u.UserName == userName).FirstOrDefaultAsync();
            var favoriteMotel = await _context.FavoriteMotelLists.Include("Owner").Include("Motel").Where(fm => fm.Motel.Id == id && fm.Owner.UserName == userName).FirstOrDefaultAsync();
            _context.FavoriteMotelLists.Remove(favoriteMotel);
            await _context.SaveChangesAsync();
            return RedirectToAction("FavoriteMotelList", new {id = user.Id });
        }

        [Authorize(Roles = "User,Staff")]
        [HttpGet]
        public  async Task<IActionResult> DeletFavmotel(int id)
        {
            ViewBag.Count = HttpContext.Session.GetString("Count");
            var motel = await _context.Motels.Where(m => m.Id == id).FirstOrDefaultAsync();
            MotelInputModel curMotel = new MotelInputModel()
            {
                Id = motel.Id,
                Name = motel.Name,
                Address = motel.Address,
                Province = motel.Province,
                City = motel.City,
                PostalCode = motel.PostalCode,
                ImageUrl = _client.Uri.ToString() + "/" + motel.ImageUrl
            };
            return View(curMotel);
          
        }






        public IActionResult Privacy()
        {
            return View();
        }


    }
}
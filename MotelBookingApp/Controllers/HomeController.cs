
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotelBookingApp.Data;
using MotelBookingApp.Models;
using MotelBookingApp.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity;
using Geocoding;
using Geocoding.Google;
using GoogleApi.Entities.Search.Video.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

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
            if (_userManager.GetUserName != null)
            {
                var userName = _userManager.GetUserName(User); 
                var count = _context.BookingCarts.Include("AppUser").Where(bc => bc.AppUser.UserName == userName).ToList().Count.ToString();
                HttpContext.Session.SetString("Count", count);
            }
            ViewBag.Count = HttpContext.Session.GetString("Count");
            var searchModel = new CustomerSearchVM();
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            searchModel.RoomTypeList = roomTypeList;
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("city")))
            {
                searchModel.City = HttpContext.Session.GetString("city");
            }
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("checkin")))
            {
                searchModel.CheckinDate = DateTime.Parse(HttpContext.Session.GetString("checkin"));
            }
            else 
            {
                searchModel.CheckinDate = DateTime.Now;
            }
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("checkout")))
            {
                searchModel.CheckoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout"));
            }
            else
            {
                searchModel.CheckoutDate = DateTime.Now;
            }
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("roomType")))
            {
                searchModel.SearchType = HttpContext.Session.GetString("roomType");
            }
            return View(searchModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DateTime checkin, string city, DateTime checkout, string roomType)
        {
            var searchModel = new CustomerSearchVM();
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            ViewBag.Count = HttpContext.Session.GetString("Count");
            searchModel.RoomTypeList = roomTypeList;
            searchModel.City = city;
            searchModel.CheckinDate = checkin;
            searchModel.CheckoutDate = checkout;
            searchModel.SearchType = roomType;
            if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(roomType))
            {
                TempData["searchOption"] = "Please input all searching conditions";
                return View(searchModel);
            }

            if (checkin < DateTime.Now || checkout < DateTime.Now || checkout < checkin)
            {
                TempData["searchOption"] = "Please choose valid check in and check out date"; 
                return View(searchModel);
            }

            HttpContext.Session.SetString("city", city);

            HttpContext.Session.SetString("checkin", checkin.Date.ToString("yyyy-MM-dd"));

            HttpContext.Session.SetString("checkout", checkout.Date.ToString("yyyy-MM-dd"));

            HttpContext.Session.SetString("roomType", roomType);

            return RedirectToAction("CityMotelList");

        }
        [HttpGet]
        public async Task<IActionResult> CityMotelList()
        {
            var searchModel = new CustomerSearchVM();
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            ViewBag.Count = HttpContext.Session.GetString("Count");
            searchModel.RoomTypeList = roomTypeList;
            if(HttpContext.Session.GetString("checkin") != null)
            {
                searchModel.CheckinDate = DateTime.Parse(HttpContext.Session.GetString("checkin"));
            }else
            {
                searchModel.CheckinDate = DateTime.Now;
            }
            if (HttpContext.Session.GetString("checkout") != null)
            {
                searchModel.CheckoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout"));
            }
            else
            {
                searchModel.CheckoutDate = DateTime.Now;
            }
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("city")))
            {
                searchModel.City = HttpContext.Session.GetString("city");
            }
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("roomType")))
            {
                searchModel.SearchType = HttpContext.Session.GetString("roomType");
            }
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("checkin")) ||
               string.IsNullOrEmpty(HttpContext.Session.GetString("checkout")) ||
               string.IsNullOrEmpty(HttpContext.Session.GetString("city")) ||
               string.IsNullOrEmpty(HttpContext.Session.GetString("roomType")))
            {
                TempData["searchResOption"] = "Please input all searching condition";
                return View(searchModel);
            }
            try
            {
                List<Motel> motels = await _context.Motels.Where(m => m.City == HttpContext.Session.GetString("city")).ToListAsync();
                if (motels.Count > 0)
                {
                    var motelList = new List<MotelInputModel>();
                    //var addressList = new List<string>();
                    var motelLocations = new List<Location>();
                    IGeocoder geocoder = new GoogleGeocoder() { ApiKey = "AIzaSyCZClJxke6nBFR5PImzPBpjdUZn8FxxhDU" };
                    string city = "";
                    foreach (var motel in motels)
                    {
                      
                        MotelInputModel newMotel = new MotelInputModel
                        {
                            Id = motel.Id,
                            Name = motel.Name,
                            Address = motel.Address,
                            Province = motel.Province,
                            City = motel.City,
                            PostalCode = motel.PostalCode,
                            ImageUrl = _client.Uri.ToString() + "/" + motel.ImageUrl
                        };
                        var comments = await _context.Comments.Include("Motel").Include("User").Where(c => c.Motel.Id == motel.Id).ToListAsync();
                        if (comments.Count > 0)
                        {
                            int totalScore = 0;
                            foreach (var c in comments)
                            {
                                totalScore += int.Parse(c.Score);
                            }
                            var score = totalScore / comments.Count;
                            newMotel.Score = score;
                        }
                        motelList.Add(newMotel);
                        string address =motel.Address + "," + motel.City + "," + motel.Province;       
                        var motelLocation = await geocoder.GeocodeAsync(address);
                        city = motel.City;
                        Location targetMotel = new Location
                        {
                            Latitude = motelLocation.First().Coordinates.Latitude,
                            Longitude = motelLocation.First().Coordinates.Longitude,
                            Address = address
                        };
                        motelLocations.Add(targetMotel);
                    }
                    var  cityCenter = await geocoder.GeocodeAsync(city);
                    Location center = new Location
                    {
                        Latitude = cityCenter.First().Coordinates.Latitude,
                        Longitude = cityCenter.First().Coordinates.Longitude,
                        Address = city
                    };
                    ViewBag.center = center;
                    ViewBag.motelLocations = motelLocations;
                    searchModel.AvailableMotels = motelList;
                    return View(searchModel);
                }
                else
                {
                    TempData["searchResOption"] = "Sorry,there is no result for your search.";
                    return View(searchModel);
                }
            }
            catch (SystemException ex)
            {
                TempData["searchResOption"] = ex.Message;
                return View(searchModel);
            }
        }

        public class Location
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Address { get; set; }
        }


        [HttpPost,ActionName("CityMotelList")]
        public async Task<IActionResult> ReSearch(DateTime checkin, string city, DateTime checkout, string roomType)
        {
            StaffBookingVM searchModel = new StaffBookingVM();
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            searchModel.RoomTypeList = roomTypeList;
            searchModel.City = city;
            searchModel.CheckinDate = checkin;
            searchModel.CheckoutDate = checkout;
            searchModel.SearchType = roomType; ;
            ViewBag.Count = HttpContext.Session.GetString("Count");
            if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(roomType))
            {
                TempData["searchResOption"] = "Please input all searching conditions";
                return View(searchModel);
            }

            if (checkin < DateTime.Now || checkout < DateTime.Now || checkout < checkin)
            {
                TempData["searchResOption"] = "Please choose valid check in and check out date";
                return View(searchModel);
            }

            HttpContext.Session.SetString("city", city);

            HttpContext.Session.SetString("checkin", checkin.Date.ToString("yyyy-MM-dd"));

            HttpContext.Session.SetString("checkout", checkout.Date.ToString("yyyy-MM-dd"));

            HttpContext.Session.SetString("roomType", roomType);

            return RedirectToAction("CityMotelList");
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
            var comments = await _context.Comments.Include("Motel").Include("User").Where(c => c.Motel.Id == id).ToListAsync();
            if (comments.Count > 0) { 
                motelDetail.Comments = comments;
                int totalScore = 0;
                foreach (var c in comments)
                {
                    totalScore += int.Parse(c.Score);
                }
                var score = totalScore / comments.Count;
                newMotel.Score = score;
            }
            motelDetail.Motel = newMotel;
            var address = motel.Address + "," + motel.City + "," + motel.Province + motel.PostalCode;
            @ViewBag.Address = address;
            IGeocoder geocoder = new GoogleGeocoder() { ApiKey = "AIzaSyCZClJxke6nBFR5PImzPBpjdUZn8FxxhDU" };
            var addresses = await geocoder.GeocodeAsync(address);

            ViewBag.Latitude = addresses.First().Coordinates.Latitude;
            ViewBag.Longitude = addresses.First().Coordinates.Longitude;
            return View(motelDetail);
        }


        [HttpGet]
        public async Task<IActionResult> RemoveComment(int id)
        {
                var comment = await _context.Comments.Include("User").Include("Motel").Where(c => c.Id == id).FirstOrDefaultAsync(); 
                return View(comment);  
        }



        [HttpPost,ActionName("RemoveComment")]
        public async Task<IActionResult> RemoveCommentConfirm(int id)
        {
            int motelId;
            var comment = await _context.Comments.Include("User").Include("Motel").Where(c => c.Id == id).FirstOrDefaultAsync();
            try { 
                motelId = comment.Motel.Id;
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("CityMotelDetail", new { id = motelId });
            }
            catch (SystemException ex)
            {
                TempData["removeComment"] = ex.Message;
                return View(comment);
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
                TempData["opeFav"] = ex.Message;
        
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


        [HttpGet]
        public async Task<IActionResult> SearchRoomList(int id)
        {
            ViewBag.Count = HttpContext.Session.GetString("Count");

            if (HttpContext.Session.GetString("checkin") == null || HttpContext.Session.GetString("checkout") == null)
            {
                TempData["alertMsg"] = "Please select CheckIn and CheckOut date first";
                return RedirectToAction("Index", "Home");
            }

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
                                Motel = room.Motel,
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
                        return View(new List<RoomInputModel>());
                    }

                }
                else
                {
                    TempData["roomRes"] = "Sorry,no available rooms";
                    return View(new List<RoomInputModel>());
                }
            }
            catch (SystemException ex)
            {
                TempData["roomRes"] = ex.Message;
                return View(new List<RoomInputModel>());
            }
        }


        [HttpGet]
        public async Task<IActionResult> RoomDetail(int id)
        {
            ViewBag.Count = HttpContext.Session.GetString("Count");
            var room = await _context.Rooms.Include("Motel").Include("RoomType").FirstOrDefaultAsync(r => r.Id == id);
            var motel = await _context.Motels.FirstOrDefaultAsync(r => r.Id == room.Motel.Id);
            RoomInputModel newRoom = new RoomInputModel
            {
                Id = room.Id,
                RoomNum = room.RoomNum,
                Price = room.Price,
                Motel = motel,
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
            List<BookingCart> cartItems = new List<BookingCart>();
            if (User.IsInRole("Staff")){
                var email = HttpContext.Session.GetString("userEmail");
                if(email == null)
                {
                    TempData["CartOption"] = "No customer specified";
                    return View(new List<BookingCart>());
                }
                cartItems = await _context.BookingCarts.Include("AppUser").Include("Room").Where(bc => bc.AppUser.Email == email).ToListAsync();
            }
            else { 
            var userName = _userManager.GetUserName(User);
            cartItems = await _context.BookingCarts.Include("AppUser").Include("Room").Where(bc => bc.AppUser.UserName == userName).ToListAsync();
            }
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

        [Authorize(Roles = "User,User,Staff")]
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
                // todo
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


        [Authorize(Roles = "User")]
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
        [Authorize(Roles = "User")]
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

        [Authorize(Roles = "User")]
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
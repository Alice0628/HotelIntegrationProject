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
    [Authorize(Roles = "Staff")]
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
        [AllowAnonymous]
        public IActionResult Index() => View(new RegisterVM());

        [HttpPost, ActionName("Index")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterVM registerVM)
        {
            var user = await _userManager.FindByEmailAsync(registerVM.Email);
            if (user != null)
            {
                HttpContext.Session.SetString("userEmail", registerVM.Email);
                return RedirectToAction(nameof(SearchRoom));
            }
            if (!ModelState.IsValid) return View(registerVM);
            var newUser = new AppUser()
            {
                Email = registerVM.Email,
                UserName = registerVM.UserName,
                PhoneNumber = registerVM.PhoneNumber,
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                DOB = registerVM.DOB,
            };
            var newUserResponse = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (newUserResponse.Succeeded)
            {
                // sign user to "User"
                var signToUser = await _userManager.AddToRoleAsync(newUser, "User");

                if (signToUser.Succeeded)
                {
                    // set EmailConfirmation to true
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    var result = await _userManager.ConfirmEmailAsync(newUser, code);
                    HttpContext.Session.SetString("userEmail", registerVM.Email);
                    return RedirectToAction(nameof(SearchRoom));
                }
                else
                {
                    TempData["CreateCustomer"] = "Registration failed";
                    await _userManager.DeleteAsync(newUser);
                    return View(registerVM);
                }
            }
            foreach (var error in newUserResponse.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(registerVM);

        }

        [HttpGet]
        public async Task<IActionResult> SearchRoom()
        {
            StaffBookingVM searchModel = new StaffBookingVM();
            ViewBag.count = HttpContext.Session.GetString("count");
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            searchModel.RoomTypeList = roomTypeList;
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
            var availableRooms = GetRooms(searchModel.CheckinDate, searchModel.CheckoutDate, searchModel.SearchType).Result;
            if(availableRooms != null && availableRooms.Count > 0)
            {
                searchModel.AvailableRooms = availableRooms;
            }   
            return View(searchModel);
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
                TempData["staffsearchOption"] = ex.Message;
                return null;
            }
        }


        [HttpPost, ActionName("SearchRoom")]
        public async Task<IActionResult> LaunchSearch(DateTime checkin, DateTime checkout, string roomType)
        {
            StaffBookingVM searchModel = new StaffBookingVM();
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            searchModel.RoomTypeList = roomTypeList;
            searchModel.CheckinDate = checkin;
            searchModel.CheckoutDate = checkout;
            searchModel.SearchType = roomType;

            if (string.IsNullOrEmpty(roomType))
            {
                TempData["staffSearchOption"] = "Please input roomType";
                return View(searchModel);
            }
            if (checkin < DateTime.Now || checkout < DateTime.Now || checkout < checkin)
            {
                TempData["staffSearchOption"] = "Please choose valid check in and check out date";
                return View(searchModel);
            }
            HttpContext.Session.SetString("checkin", checkin.ToString());
            HttpContext.Session.SetString("checkout", checkout.ToString());
            HttpContext.Session.SetString("roomType", roomType);

            var availableRooms = GetRooms(checkin, checkout, roomType).Result;
            if (availableRooms != null && availableRooms.Count > 0)
            {
                searchModel.AvailableRooms = availableRooms;
            }
            else
            {
                TempData["staffSearchOption"] = "Sorry,there is no result for your search.";
            }
            return View(searchModel);
        }


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
            DateTime checkinDate;
            DateTime checkoutDate;
            if (HttpContext.Session.GetString("checkin") != null && HttpContext.Session.GetString("checkout") != null)
            {
                checkinDate = DateTime.Parse(HttpContext.Session.GetString("checkin"));
                checkoutDate = DateTime.Parse(HttpContext.Session.GetString("checkout"));
                try
                {

                    Room room = await _context.Rooms.Include("RoomType").Include("Motel").Where(r => r.Id == id).FirstOrDefaultAsync();
                    if (room == null)
                    {
                        TempData["addtocart"] = "no room found";
                        return View();
                    }
                    var email = HttpContext.Session.GetString("userEmail");
                    if (email != null)
                    {
                        var user = await _context.Users.Include("Motel").Where(u => u.Email == email).FirstOrDefaultAsync();
                        if (user == null)
                        {
                            TempData["CreateCustomer"] = "User does not exist";
                            return RedirectToAction(nameof(Index));
                        }
                        var count = _context.BookingCarts.Include("AppUser").Where(bc => bc.AppUser.Email == email).ToList().Count.ToString();
                        HttpContext.Session.SetString("count", count);
                        BookingCart bookingCart = new BookingCart
                        {
                            AppUser = user,
                            CheckinDate = checkinDate,
                            CheckoutDate = checkoutDate,
                            Room = room
                        };

                        _context.BookingCarts.Add(bookingCart);
                        await _context.SaveChangesAsync();

                        var newCount = int.Parse(HttpContext.Session.GetString("count")) + 1;
                        HttpContext.Session.SetString("count", newCount.ToString());
                        ViewBag.Count = newCount;
                        HttpContext.Session.SetString("count", count.ToString());
                        TempData["addtocart"] = $"Room {room.RoomNum} has been added to cart";
                        return RedirectToAction(nameof(SearchRoom));
                    }
                    TempData["CreateCustomer"] = "User does not exist";
                    return RedirectToAction(nameof(Index));
                }
                catch (SystemException ex)
                {
                    TempData["addtocart"] = ex.Message;
                    return View();
                }
            }
            else
            {
                TempData["addtocart"] = "please choose valide checkin and checkout date";
                return RedirectToAction(nameof(SearchRoom));
            }
           
        }


        //[HttpGet]
        //public async Task<IActionResult> Cart()
        //{
        //    ViewBag.count = HttpContext.Session.GetString("count");
        //    var userName = _userManager.GetUserName(User);

        //    List<BookingCart> cartItems = await _context.BookingCarts.Include("AppUser").Include("Room").Where(bc => bc.AppUser.UserName == userName).ToListAsync();

        //    decimal subTotal = 0;
        //    foreach (var cartItem in cartItems)
        //    {
        //        subTotal = subTotal + cartItem.Room.Price * (cartItem.CheckoutDate - cartItem.CheckinDate).Days;
        //    }
        //    var tax = ((double)subTotal) * 0.15;
        //    ViewBag.tax = tax.ToString("0.00");
        //    var Total = ((double)subTotal + tax).ToString("0.00");
        //    ViewBag.Total = Total;
        //    return View(cartItems);

        //}





        //[HttpPost, ActionName("Cart")]
        //public async Task<IActionResult> RemoveItem(int id)
        //{
        //    var cartItem = await _context.BookingCarts.Where(bc => bc.Id == id).FirstOrDefaultAsync();
        //    if (cartItem == null)
        //    {
        //        return View();
        //    }
        //    _context.BookingCarts.Remove(cartItem);
        //    await _context.SaveChangesAsync();
        //    var count = int.Parse(HttpContext.Session.GetString("count")) - 1;
        //    HttpContext.Session.SetString("count", count.ToString());
        //    ViewBag.count = count.ToString();
        //    return View();

        //}

    }
}



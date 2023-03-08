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
using Stripe;

namespace MotelBookingApp.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffStaticController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly BlobContainerClient _client;
        private readonly UserManager<AppUser> _userManager;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public StaffStaticController(UserManager<AppUser> userManager, IConfiguration configuration, MotelDbContext context)
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
            BookedRecordDisplayVM recordModel = new BookedRecordDisplayVM();
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            recordModel.RoomTypeList = roomTypeList;
            var userName = _userManager.GetUserName(User);
            var user = await _context.AppUsers.Include("Motel").Where(u => u.UserName == userName).FirstOrDefaultAsync();
            var bookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name).ToListAsync();
            recordModel.BookedRooms = bookedRooms;
            


            string[] months = new[] { "January", "February", "March", "April", "May", "June" };
            int[] recordData = new int[6];
            for (int i = 0; i < months.Length; i++)
            {
                recordData[i] = bookedRooms.Count(br => br.CheckinDate.Month == i + 1);
            }

            ViewBag.monthLabels = months;
            ViewBag.monthDatasetLabel = "Records-Month";
            ViewBag.monthDatasetData = recordData;

            string[] roomTypes = new string[roomTypeList.Count];
            for (int i = 0; i < roomTypeList.Count; i++)
            {
                roomTypes[i] = roomTypeList[i].Name;
            }
            int[] roomTypeCountData = new int[6];
            for (int i = 0; i < roomTypes.Length; i++)
            {
                roomTypeCountData[i] = bookedRooms.Count(br => br.Room.RoomType.Name == roomTypes[i]);
            }
            ViewBag.roomTypeLabels = roomTypes;
            ViewBag.roomTypeDatasetLabel = "Records-RoomType";
            ViewBag.roomTypeDatasetData = roomTypeCountData;

            //string[] months = new[] { "January", "February", "March", "April", "May", "June" };

            decimal[] turnoverData = new decimal[6];
            for (int i = 0; i < months.Length; i++)
            {
                var records = bookedRooms.FindAll(br => br.CheckinDate.Month == i + 1);
                decimal amount = 0;
                if (records != null)
                {
                    foreach (var r in records)
                    {
                        amount += r.Booking.TotalAmount;
                    }
                    turnoverData[i] = amount;
                }
                else
                {
                    turnoverData[i] = 0;
                }
            }
            ViewBag.turnoverLabels = months;
            ViewBag.turnoverDatasetLabel = "Turnover-Month";
            ViewBag.turnoverDatasetData = turnoverData;

            return View(recordModel);
        }

        [HttpPost, ActionName("Index")]
        public async Task<IActionResult> LaunchSearch(DateTime checkin, DateTime checkout, string roomType, string username)
        {
            BookedRecordDisplayVM recordModel = new BookedRecordDisplayVM();
            var searchBookedRooms = new List<BookedRecord>();
            var roomTypeList = await _context.RoomTypes.ToListAsync();
            recordModel.RoomTypeList = roomTypeList;
            var userName = _userManager.GetUserName(User);
            var user = await _context.Users.Include("Motel").Where(u => u.UserName == userName).FirstOrDefaultAsync();
            var bookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name).ToListAsync();
            string[] months = new[] { "January", "February", "March", "April", "May", "June" };
            int[] recordData = new int[6];
            for (int i = 0; i < months.Length; i++)
            {
                recordData[i] = bookedRooms.Count(br => br.CheckinDate.Month == i + 1);
            }

            ViewBag.monthLabels = months;
            ViewBag.monthDatasetLabel = "Records-Month";
            ViewBag.monthDatasetData = recordData;

            string[] roomTypes = new string[roomTypeList.Count];
            for (int i = 0; i < roomTypeList.Count; i++)
            {
                roomTypes[i] = roomTypeList[i].Name;
            }
            int[] roomTypeCountData = new int[6];
            for (int i = 0; i < roomTypes.Length; i++)
            {
                roomTypeCountData[i] = bookedRooms.Count(br => br.Room.RoomType.Name == roomTypes[i]);
            }
            ViewBag.roomTypeLabels = roomTypes;
            ViewBag.roomTypeDatasetLabel = "Records-RoomType";
            ViewBag.roomTypeDatasetData = roomTypeCountData;

            //string[] months = new[] { "January", "February", "March", "April", "May", "June" };

            decimal[] turnoverData = new decimal[6];
            for (int i = 0; i < months.Length; i++)
            {
                var records = bookedRooms.FindAll(br => br.CheckinDate.Month == i + 1);
                decimal amount = 0;
                if (records != null)
                {
                    foreach (var r in records)
                    {
                        amount += r.Booking.TotalAmount;
                    }
                    turnoverData[i] = amount;
                }
                else
                {
                    turnoverData[i] = 0;
                }
            }
            ViewBag.turnoverLabels = months;
            ViewBag.turnoverDatasetLabel = "Turnover-Month";
            ViewBag.turnoverDatasetData = turnoverData;

            // by nothing
            if (string.IsNullOrEmpty(username) && checkin.Year == 0001 && checkout.Year == 0001 && string.IsNullOrEmpty(roomType))
            {
                searchBookedRooms = bookedRooms;
            }
            //by username
            else if (!string.IsNullOrEmpty(username) && checkin.Year == 0001 && checkout.Year == 0001 && string.IsNullOrEmpty(roomType))
            {
                recordModel.UserName = username;
                searchBookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Booking.AppUser.UserName == username).ToListAsync();
            }
            //by date period
            else if (string.IsNullOrEmpty(username) && checkin.Year != 0001 && checkout.Year != 0001 && string.IsNullOrEmpty(roomType))
            {
                recordModel.CheckinDate = checkin;
                recordModel.CheckoutDate = checkout;
                if (checkin > checkout)
                {
                    TempData["staffStatic"] = "check out date should be after checkin date ";
                    return View(recordModel);
                }
                searchBookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.CheckinDate > checkin && br.CheckoutDate < checkout).ToListAsync();
            }
            //by room type
            else if (string.IsNullOrEmpty(username) && checkin.Year == 0001 && checkout.Year == 0001 && !string.IsNullOrEmpty(roomType))
            {
                recordModel.SearchType = roomType;
                searchBookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Room.RoomType.Name == roomType).ToListAsync();
            }
            //by username and date period
            else if (!string.IsNullOrEmpty(username) && checkin.Year != 0001 && checkout.Year != 0001 && string.IsNullOrEmpty(roomType))
            {
                recordModel.UserName = username;
                recordModel.CheckinDate = checkin;
                recordModel.CheckoutDate = checkout;
                if (checkin > checkout)
                {
                    TempData["staffStatic"] = "check out date should be after checkin date ";
                    return View(recordModel);
                }
                searchBookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Booking.AppUser.UserName == username && (br.CheckinDate > checkin && br.CheckoutDate < checkout)).ToListAsync();
            }
            // by username and room type
            else if (!string.IsNullOrEmpty(username) && checkin.Year == 0001 && checkout.Year == 0001 && !string.IsNullOrEmpty(roomType))
            {
                recordModel.UserName = username;
                recordModel.RoomTypeName = roomType;
                searchBookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Booking.AppUser.UserName == username && br.Room.RoomType.Name == roomType).ToListAsync(); 
            }
            // by date period and room type
            else if (string.IsNullOrEmpty(username) && checkin.Year != 0001 && checkout.Year != 0001 && !string.IsNullOrEmpty(roomType))
            {
                recordModel.CheckinDate = checkin;
                recordModel.CheckoutDate = checkout;
                recordModel.RoomTypeName = roomType;
                if (checkin > checkout)
                {
                    TempData["staffStatic"] = "check out date should be after checkin date ";
                    return View(recordModel);
                }
                searchBookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Room.RoomType.Name == roomType && (br.CheckinDate > checkin && br.CheckoutDate < checkout)).ToListAsync(); 
            }
            // by username and date period and room type
            else if (!string.IsNullOrEmpty(username) && checkin.Year != 0001 && checkout.Year != 0001 && !string.IsNullOrEmpty(roomType))
            {
                recordModel.UserName = username;
                recordModel.CheckinDate = checkin;
                recordModel.CheckoutDate = checkout;
                recordModel.RoomTypeName = roomType;
                if (checkin > checkout)
                {
                    TempData["staffStatic"] = "check out date should be after checkin date ";
                    return View(recordModel);
                }
                searchBookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Booking.AppUser.UserName == username && br.Room.RoomType.Name == roomType && (br.CheckinDate > checkin && br.CheckoutDate < checkout)).ToListAsync();
            }
            else
            {
                TempData["staffStatic"] = "Please choose valid checkin and checkout date";
                return View(recordModel);
            }
            if (searchBookedRooms == null)
            {
                TempData["staffStatic"] = "No result found for your research";
                return View(recordModel);
            }
            recordModel.BookedRooms = searchBookedRooms;
            return View(recordModel);
        }



    }
}
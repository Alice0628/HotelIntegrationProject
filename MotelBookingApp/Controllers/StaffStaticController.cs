﻿using System;
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


namespace MotelBookingApp.Controllers
{
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
            var userName = _userManager.GetUserName(User);
            var user = await _context.Users.Include("Motel").Where(u => u.UserName == userName).FirstOrDefaultAsync();
            var bookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name).ToListAsync();
            recordModel.BookedRooms = bookedRooms;
            return View(recordModel);
        }

        //public async Task<List<RoomInputModel>> GetRooms(DateTime? checkin, DateTime? checkout, string? roomType)
        //{

        //    try
        //    {
        //        //List<Motel> motels = await _context.Motels.Where(m => m.City == searchCity).ToListAsync();
        //        var userName = _userManager.GetUserName(User);
        //        var user = await _context.Users.Include("Motel").Where(u => u.UserName == userName).FirstOrDefaultAsync();
        //        List<Room> rooms = await _context.Rooms.Include("Motel").Include("RoomType").Where(r => r.Motel.Name == user.Motel.Name && r.RoomType.Name == roomType).ToListAsync();
        //        ViewBag.RoomMotel = user.Motel.Name;
        //        var unavailableList = new List<Room>();
        //        if (rooms.Count > 0)
        //        {
        //            foreach (var r in rooms)
        //            {
        //                var bookedroom = await _context.BookedRecords.Include("Room").Where(br => br.Room.Id == r.Id && ((br.CheckinDate >= checkin && br.CheckinDate <= checkout) || (br.CheckoutDate >= checkin && br.CheckoutDate <= checkout))).FirstOrDefaultAsync();
        //                var bookingroom = await _context.BookingCarts.Include("Room").Where(bc => bc.Room.Id == r.Id && ((bc.CheckinDate >= checkin && bc.CheckinDate <= checkout) || (bc.CheckoutDate >= checkin && bc.CheckoutDate <= checkout))).FirstOrDefaultAsync();
        //                if (bookedroom != null || bookingroom != null)
        //                {
        //                    unavailableList.Add(r);
        //                }
        //            }
        //            foreach (var ur in unavailableList)
        //            {
        //                rooms.Remove(ur);
        //            }
        //            var roomList = new List<RoomInputModel>();
        //            if (rooms.Count > 0)
        //            {
        //                foreach (var room in rooms)
        //                {
        //                    RoomInputModel newRoom = new RoomInputModel
        //                    {
        //                        Id = room.Id,
        //                        RoomNum = room.RoomNum,
        //                        Price = room.Price,
        //                        MotelName = room.Motel.Name,
        //                        RoomTypeName = room.RoomType.Name,
        //                        RoomTypeImage = _client.Uri.ToString() + "/" + room.RoomType.ImageUrl
        //                    };

        //                    roomList.Add(newRoom);
        //                }
        //                return roomList;
        //            }
        //            else
        //            {

        //                return null;
        //            }

        //        }
        //        else
        //        {

        //            return null;
        //        }
        //    }

        //    catch (SystemException ex)
        //    {
        //        TempData["searchOption"] = ex.Message;
        //        return null;
        //    }
        //}


        [HttpPost, ActionName("Index")]
        public async Task<IActionResult> LaunchSearch(DateTime? checkin, DateTime? checkout, string? roomType, string? username)
        {
            var userName = _userManager.GetUserName(User);
            var user = await _context.Users.Include("Motel").Where(u => u.UserName == userName).FirstOrDefaultAsync();
            BookedRecordDisplayVM recordModel = new BookedRecordDisplayVM();
            var bookedRooms = new List<BookedRecord>();
            // by nothing
            if (string.IsNullOrEmpty(username) && checkin.ToString() == "0001-01-01 12:00:00 AM" && checkout.ToString() == "0001-01-01 12:00:00 AM" && string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index");
            }
            //by username
            else if (!string.IsNullOrEmpty(username) && checkin.ToString() == "0001-01-01 12:00:00 AM" && checkout.ToString() == "0001-01-01 12:00:00 AM" && string.IsNullOrEmpty(roomType))
            {
                recordModel.UserName = username;
                bookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Booking.AppUser.UserName == username).ToListAsync();
                recordModel.BookedRooms = bookedRooms;
            }
            //by date period
            else if (string.IsNullOrEmpty(username) && checkin.ToString() != "0001-01-01 12:00:00 AM" && checkout.ToString() != "0001-01-01 12:00:00 AM" && string.IsNullOrEmpty(roomType))
            {
                recordModel.CheckinDate = checkin;
                recordModel.CheckoutDate = checkout;
                if (checkin > checkout)
                {
                    TempData["dateInvalide"] = "check out date should be after checkin date ";
                    return View(recordModel);
                }
                bookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.CheckinDate > checkin && br.CheckoutDate < checkout).ToListAsync();
                recordModel.BookedRooms = bookedRooms;
            }
            //by room type
            else if (string.IsNullOrEmpty(username) && checkin.ToString() == "0001-01-01 12:00:00 AM" && checkout.ToString() == "0001-01-01 12:00:00 AM" && !string.IsNullOrEmpty(roomType))
            {
                recordModel.RoomTypeName = roomType;
                bookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Room.RoomType.Name == roomType).ToListAsync();
                recordModel.BookedRooms = bookedRooms;
                return View(recordModel);
            }
            //by username and date period
            else if (!string.IsNullOrEmpty(username) && checkin.ToString() != "0001-01-01 12:00:00 AM" && checkout.ToString() != "0001-01-01 12:00:00 AM" && string.IsNullOrEmpty(roomType))
            {
                recordModel.UserName = username;
                recordModel.CheckinDate = checkin;
                recordModel.CheckoutDate = checkout;
                if (checkin > checkout)
                {
                    TempData["dateInvalide"] = "check out date should be after checkin date ";
                    return View(recordModel);
                }
                bookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Booking.AppUser.UserName == username && (br.CheckinDate > checkin && br.CheckoutDate < checkout)).ToListAsync();
            }
            // by username and room type
            else if (!string.IsNullOrEmpty(username) && checkin.ToString() == "0001-01-01 12:00:00 AM" && checkout.ToString() == "0001-01-01 12:00:00 AM" && !string.IsNullOrEmpty(roomType))
            {
                recordModel.UserName = username;
                recordModel.RoomTypeName = roomType;
                bookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Booking.AppUser.UserName == username && br.Room.RoomType.Name == roomType).ToListAsync();
            }
            // by date period and room type
            else if (string.IsNullOrEmpty(username) && checkin.ToString() != "0001-01-01 12:00:00 AM" && checkout.ToString() == "0001-01-01 12:00:00 AM" && !string.IsNullOrEmpty(roomType))
            {

                recordModel.CheckinDate = checkin;
                recordModel.CheckoutDate = checkout;
                recordModel.RoomTypeName = roomType;
                if (checkin > checkout)
                {
                    TempData["dateInvalide"] = "check out date should be after checkin date ";
                    return View(recordModel);
                }

                bookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Room.RoomType.Name == roomType && (br.CheckinDate > checkin && br.CheckoutDate < checkout)).ToListAsync();
            }
            // by username and date period and room type
            else if (!string.IsNullOrEmpty(username) && checkin.ToString() != "0001-01-01 12:00:00 AM" && checkout.ToString() != "0001-01-01 12:00:00 AM" && !string.IsNullOrEmpty(roomType))
            {

                recordModel.UserName = username;
                recordModel.CheckinDate = checkin;
                recordModel.CheckoutDate = checkout;
                recordModel.RoomTypeName = roomType;
                if (checkin > checkout)
                {
                    TempData["dateInvalide"] = "check out date should be after checkin date ";
                    return View(recordModel);
                }
                bookedRooms = await _context.BookedRecords.Include("Room").Include("Booking").Where(br => br.Room.Motel.Name == user.Motel.Name && br.Booking.AppUser.UserName == username && br.Room.RoomType.Name == roomType && (br.CheckinDate > checkin && br.CheckoutDate < checkout)).ToListAsync();
            }
            else
            {
                TempData["dateInvalide"] = "Please choose valid checkin and checkout date";
                return View(recordModel);
            }
            if (bookedRooms == null)
            {
                TempData["dateInvalide"] = "No result found for your research";
                return View(recordModel);
            }
            recordModel.BookedRooms = bookedRooms;
            string[] months = new[] { "January", "February", "March", "April", "May", "June", "July" };
            int[] recordData = new int[6];
            for (int i = 0; i < months.Length; i++) {
                recordData[i] = bookedRooms.Count(br => br.CheckinDate.Month == i);
            }

            // static data
            var data = new ChartData
            {
                Labels = months,
                DatasetLabel = "Records",
                DatasetData =  recordData
            };

            ViewBag.Labels = data.Labels;
            ViewBag.DatasetLabel = data.DatasetLabel;
            ViewBag.DatasetData = data.DatasetData;

            return View();

            return View(recordModel);
        }
    }
}
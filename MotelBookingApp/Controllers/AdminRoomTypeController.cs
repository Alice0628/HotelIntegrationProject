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
using System.Drawing.Drawing2D;
using Microsoft.AspNetCore.Identity;
using MotelBookingApp.Iservice;
using MotelBookingApp.Service;

namespace MotelBookingApp.Controllers
{
    public class AdminRoomTypeController : Controller
    {
        private IAdminRoomTypeService _roomTypeService;

        public AdminRoomTypeController(IAdminRoomTypeService repository)
        {
            _roomTypeService = repository;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roomTypes = await _roomTypeService.GetAllRoomTypes();
            foreach (RoomType rt in roomTypes)
            {
                rt.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{rt.ImageUrl}";
            }
            return View(roomTypes);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string searchKeyWord)
        {
            List<RoomType> roomTypes = await _roomTypeService.SearchRoomTypeAsync(searchKeyWord);
            if (roomTypes == null)
            {
                TempData["no result"] = $"no room type {searchKeyWord}";
                return View();
            }
            foreach (RoomType rt in roomTypes)
            {
                rt.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{rt.ImageUrl}";
            }
            return View(roomTypes);
        }
        // GET: AdminroomType/Details/5
        public async Task<IActionResult> Detail(int id)
        {
            RoomTypeInputModel newRoomType = await _roomTypeService.GetEditRoomType(id);

            if (newRoomType == null)
                return View(new MotelInputModel());
        
            return View(newRoomType);
        }

        // GET: AdminroomType/Create
        public IActionResult Create()
        {
            RoomTypeInputModel newRoomType = new RoomTypeInputModel();
            int maxIndex = _roomTypeService.FindMaxIndex().Result;
            newRoomType.Id = maxIndex + 1;
            return View(newRoomType);

        }

        // POST: AdminroomType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomTypeInputModel newRoomType)
        {

            if (newRoomType.TypeImage == null)
            {
                TempData["noImage"] = $"No Image";

                return View(newRoomType);
            }
            await _roomTypeService.AddRoomType(newRoomType);
            return RedirectToAction(nameof(Index));
        }

        // GET: AdminroomType/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            TempData["id"] = id;
            RoomTypeInputModel newRoomType = await _roomTypeService.GetEditRoomType(id);

            if (newRoomType == null)
                return View(new MotelInputModel());
            else
                return View(newRoomType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomTypeInputModel newRoomType, int id)
        {
            bool res = _roomTypeService.UpdateRoomType(newRoomType).Result;
            if (res)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(newRoomType);
            }


        }


        // GET: AdminroomType/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            RoomType roomType = await _roomTypeService.SingleRoomType(id);

            if (roomType == null)
                return View(new Motel());
            else
            {
                roomType.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{roomType.ImageUrl}";

                return View(roomType);
            }


        }

        // POST: AdminroomType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var res = _roomTypeService.DeleteRoomType(id).Result;
            if (res)
                return RedirectToAction("index");
            else
                return RedirectToAction("Delete");
 
        }
    }
}




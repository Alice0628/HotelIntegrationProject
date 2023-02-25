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
using Microsoft.AspNetCore.Components.Forms;

namespace MotelBookingApp.Controllers
{
    public class StaffRoomController : Controller
    {
        private readonly IStaffRoomService _staffRoomService;
        private readonly IAdminRoomTypeService _adminRoomTypeService;

        public StaffRoomController(IStaffRoomService repository,IAdminRoomTypeService adminRoomTypeService)
        {
            _staffRoomService = repository;
            _adminRoomTypeService = adminRoomTypeService;
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rooms = await _staffRoomService.GetAllRooms();
            //foreach (RoomType r in rooms)
            //{
            //    r.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{r.ImageUrl}";
            //}
            return View(rooms);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string searchKeyWord)
        {
            List<Room> rooms = await _staffRoomService.SearchRoomAsync(searchKeyWord);
            if (rooms == null)
            {
                TempData["no result"] = $"no room type {searchKeyWord}";
                return View();
            }
            //foreach (Room r in rooms)
            //{
            //    r.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{r.ImageUrl}";
            //}
            return View(rooms);
        }
        // GET: AdminroomType/Details/5
        public async Task<IActionResult> Detail(int id)
        {
            RoomInputModel newRoom = await _staffRoomService.GetEditRoom(id);

            if (newRoom == null)
                return View(new MotelInputModel());
            else
                //newRoom.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{newRoomType.ImageUrl}";
            return View(newRoom);
        }

        // GET: AdminroomType/Create
        public IActionResult Create()
        {
            RoomInputModel newRoom = new RoomInputModel();
            List<RoomType> roomTypeList = _adminRoomTypeService.GetAllRoomTypes().Result;
            newRoom.RoomTypeList = roomTypeList;
            int maxIndex = _staffRoomService.FindMaxIndex().Result;
            newRoom.Id = maxIndex + 1;
            return View(newRoom);

        }

        // POST: AdminroomType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomInputModel newRoom)
        {
 
            await _staffRoomService.AddRoom(newRoom);
            return RedirectToAction(nameof(Index));
        }

        // GET: AdminroomType/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
      
            RoomInputModel newRoom = await _staffRoomService.GetEditRoom(id);

            if (newRoom == null)
                return View(new RoomInputModel());
            else
                return View(newRoom);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomInputModel newRoom, int id)
        {
            bool res = _staffRoomService.UpdateRoom(newRoom).Result;
            if (res)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(newRoom);
            }


        }


        // GET: AdminroomType/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            Room room = await _staffRoomService.SingleRoom(id);

            if (room == null)
                return View(new Motel());
            else
            {
                //room.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{roomType.ImageUrl}";

                return View(room);
            }


        }

        // POST: AdminroomType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var res = _staffRoomService.DeleteRoom(id).Result;
            if (res)
                return RedirectToAction("index");
            else
                return RedirectToAction("Delete");

        }
    }
}




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

namespace MotelBookingApp.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffRoomController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly BlobContainerClient _client;
        private readonly UserManager<AppUser> _userManager;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
      
        public StaffRoomController(UserManager<AppUser> userManager, IConfiguration configuration, MotelDbContext context)
        {
            _context = context;
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
            _userManager = userManager;
            _client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
        }

        // GET: AdminAirport
        public async Task<IActionResult> Index()
        {
            var userName = _userManager.GetUserName(User);
            var user = _context.Users.Include("Motel").Where(u => u.UserName == userName).FirstOrDefault();
            return _context.Rooms != null ?
                       View(await _context.Rooms.Include("RoomType").Include("Motel").Where(r => r.Motel.Name == user.Motel.Name).ToListAsync()):
                       Problem("Entity set 'MotelBookingAppContext.Rooms'  is null.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string searchWord)
        {
            if (searchWord == null)
            {
                var userName = _userManager.GetUserName(User);
                var user = _context.Users.Include("Motel").Where(u => u.UserName == userName).FirstOrDefault();
                return _context.Rooms != null ?
                           View(await _context.Rooms.Include("RoomType").Include("Motel").Where(r => r.Motel.Name == user.Motel.Name).ToListAsync()) :
                           Problem("Entity set 'MotelDbContext.Rooms'  is null.");
            }
            else
            {
                searchWord = searchWord.ToLower();
                List<Room> searcheRes = await _context.Rooms.Include("RoomType").Include("Motel").Where(a => a.RoomNum.ToLower().Contains(searchWord) || a.RoomType.Name.ToLower().Contains(searchWord) || a.Motel.Name.ToLower().Contains(searchWord)).ToListAsync();
                
                if (searcheRes.Count == 0)
                {
                    TempData["RoomOption"] = "No search results";
                }
                return View(searcheRes);
            }
        }

        // GET: AdminAirport/Create
        public async Task<IActionResult> Create()
        {
            List<RoomType> roomTypeList = await _context.RoomTypes.ToListAsync();
            RoomInputModel newRoom = new RoomInputModel();
            newRoom.RoomTypeList = roomTypeList;    
            return View(newRoom);
        }

        // POST: AdminAirport/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomInputModel newRoom)
        {
          
            try
            {
                Room ifRoom = await _context.Rooms.FirstOrDefaultAsync(a => a.RoomNum == newRoom.RoomNum);

                if (ifRoom != null)
                {
                    TempData["MotelCreateOption"] = $"Room {newRoom.RoomNum}has already existed";
                    return View(newRoom);
                }
                if (!ModelState.IsValid)
                {
                    return View(newRoom);
                }
            
                RoomType roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == newRoom.RoomType);
                var userName = _userManager.GetUserName(User);
                var user = _context.Users.Include("Motel").Where(u => u.UserName == userName).FirstOrDefault();
                Motel motel = await _context.Motels.FirstOrDefaultAsync(m => m.Name == user.Motel.Name);

                Room room = new Room()
                {
                    RoomNum = newRoom.RoomNum,
                    Price = newRoom.Price,
                    RoomType = roomType,
                    Motel = motel,
                    IfAvailable= true,
                };
                _context.Rooms.Add(room);
                await _context.SaveChangesAsync();
                TempData["RoomOption"] = $"{room.RoomNum} has been created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (SystemException ex)
            {
                TempData["RoomCreateOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: AdminAirport/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var room = await _context.Rooms.Include("Motel").Include("RoomType").FirstOrDefaultAsync(m => m.Id == id);
                
                List<RoomType> roomTypeList = await _context.RoomTypes.ToListAsync();
                string blobUrl = _client.Uri.ToString();
                RoomInputModel newRoom = new RoomInputModel
                {
                    Id= room.Id,
                    RoomNum = room.RoomNum,
                    Price = room.Price,
                    MotelName = room.Motel.Name,
                    RoomType = room.RoomType.Id,
                    RoomTypeList = roomTypeList,
                    RoomTypeImage = _client.Uri.ToString() + "/" + room.RoomType.ImageUrl

                };

                return View(newRoom);
            }
            catch (SystemException ex)
            {
                TempData["AirportEditOption"] = $"{ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomInputModel editRoom, int id)
        {
            if (!ModelState.IsValid)
            {
                return View(editRoom);
            }
            try
            {
                var room = _context.Rooms.Include("RoomType").Include("Motel").FirstOrDefault(a => a.Id == id);
                List<Room> roomsList = _context.Rooms.ToList<Room>();
                roomsList.Remove(room);
                // if Number exists
                Room ifRoom = roomsList.Find(a => a.RoomNum == editRoom.RoomNum);
                if (ifRoom != null)
                {
                    TempData["RoomEditOption"] = $"Room Name {editRoom.RoomNum} has already existed";
                    return View(editRoom);
                }
                RoomType roomType = await _context.RoomTypes.FirstOrDefaultAsync(rt => rt.Id == editRoom.RoomType);
                room.RoomNum = editRoom.RoomNum;
                room.Price = editRoom.Price;
                room. RoomType = roomType;
                _context.Rooms.Update(room);
                await _context.SaveChangesAsync();
                TempData["RoomOption"] = $"{room.RoomNum} has been Edited successfully";
                return RedirectToAction(nameof(Index));
            }


            catch (SystemException ex)
            {
                TempData["RoomEditOption"] = $"{ex.Message}";
                return View();
            }
        }


        // GET: AdminAirport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var room = await _context.Rooms.Include("RoomType").Include("Motel")
                    .FirstOrDefaultAsync(m => m.Id == id);
   
                return View(room);
            }
            catch (SystemException ex)
            {
                TempData["RoomDeleteOption"] = $"{ex.Message}";
                return View();
            }
        }

        // POST: StaffRoom/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var room = await _context.Rooms.Include("RoomType").Include("Motel").FirstOrDefaultAsync(a => a.Id == id);
                if (room != null)
                {
                    //var blobClient = new BlobClient( _storageConnectionString,  _storageContainerName, newAirport.LogoImage.FileName);


                    var bookedRecord = await _context.BookedRecords.Include("Room").FirstOrDefaultAsync(br => br.Room.RoomNum == room.RoomNum && br.CheckoutDate > DateTime.Now);

                    if (bookedRecord != null)
                    {
                        TempData["RoomDeleteOption"] = "The room is in use or will be in use, can not delete it";
                        return View(room);
                    }
                     
                    _context.Rooms.Remove(room);
                    TempData["RoomOption"] = $"Room {room.RoomNum} has been deleted successfully";
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                TempData["RoomDeleteOption"] = $"Room {id} does not exist";
                return View();

            }
            catch (SystemException ex)
            {
                TempData["AirportDeleteOption"] = $"{ex.Message}";
                return View();
            }
        }

    }
}





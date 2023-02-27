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

namespace MotelBookingApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminRoomTypeController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly BlobContainerClient _client;
 
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public AdminRoomTypeController(IConfiguration configuration, MotelDbContext context)
        {
            _context = context;
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
            
            _client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
        }

        // GET: AdminAirport
        public async Task<IActionResult> Index()
        {
            return _context.Motels != null ?
                       View(await _context.RoomTypes.ToListAsync()) :
                       Problem("Entity set 'MotelBookingAppContext.Airports'  is null.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string searchWord)
        {
            if (searchWord == null)
            {
                return _context.RoomTypes != null ?
                           View(await _context.RoomTypes.ToListAsync()) :
                           Problem("Entity set 'MotelDbContext.Airports'  is null.");
            }
            else
            {
                searchWord = searchWord.ToLower();
                List<RoomType> searcheRes = await _context.RoomTypes.Where(a => a.Name.ToLower().Equals(searchWord)).ToListAsync();
                if (searcheRes.Count == 0)
                {
                    TempData["roomTypeOption"] = "No search results";
                }
                return View(searcheRes);
            }
        }
        // GET: AdminAirport/Details/5
        public async Task<IActionResult> Detail(int? id)
        {
            try
            {
                var roomType = await _context.RoomTypes
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (roomType == null)
                {
                    TempData["room type not exist"] = $"room type {id} does not exist";
                    return View();
                }
                RoomTypeInputModel curRoomType = new RoomTypeInputModel
                {
                    Id = roomType.Id,
                    Name = roomType.Name,
                    Sleep = roomType.Sleep,
                    Description = roomType.Description,
                    ImageUrl = _client.Uri.ToString() + "/" + roomType.ImageUrl
                };
             
                return View(curRoomType);
            }
            catch (SystemException ex)
            {
                TempData["AirportOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: AdminAirport/Create
        public IActionResult Create()
        {
            return View(new RoomTypeInputModel());
        }

        // POST: AdminAirport/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoomTypeInputModel newRoomType)
        {
            try
            {
                RoomType ifRoomType = await _context.RoomTypes.FirstOrDefaultAsync(a => a.Name == newRoomType.Name);

                if (ifRoomType != null)
                {
                    TempData["MotelCreateOption"] = $"Room Type {newRoomType.Name} has already existed";
                    return View(newRoomType);
                }
                if (!ModelState.IsValid)
                {
                    return View(newRoomType);
                }
                string fileName = newRoomType.TypeImage.FileName.Trim();
                // Create a BlobClient using the Blob storage connection string
                var blobClient = new BlobClient(_storageConnectionString, _storageContainerName, fileName);

                // Upload the image data to the Blob storage
                using (var stream = newRoomType.TypeImage.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream);
                }
                RoomType roomType = new RoomType()
                {
                   
                    Name = newRoomType.Name,

                    Description = newRoomType.Description,

                    Sleep = newRoomType.Sleep,

                    ImageUrl = newRoomType.TypeImage.FileName

                };
                _context.RoomTypes.Add(roomType);
                await _context.SaveChangesAsync();
                TempData["RoomTypeOption"] = $"{roomType.Name} has been created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (SystemException ex)
            {
                TempData["MotelCreateOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: AdminAirport/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var roomType = await _context.RoomTypes.FirstOrDefaultAsync(m => m.Id == id);
                string blobUrl = _client.Uri.ToString();
                RoomTypeInputModel newRoomType = new RoomTypeInputModel
                {
                    Id = roomType.Id,
                    Name = roomType.Name,
                    Sleep = roomType.Sleep,
                    Description = roomType.Description,
                    ImageUrl = blobUrl + "/" + roomType.ImageUrl
                };

            return View(newRoomType);
        }
            catch (SystemException ex)
            {
                TempData["AirportEditOption"] = $"{ex.Message}";
                return View();
    }
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(RoomTypeInputModel editRoomType, int id)
{
    if (!ModelState.IsValid)
    {
        return View(editRoomType);
    }
    try
    {
        var roomType = _context.RoomTypes.FirstOrDefault(a => a.Id == id);
        List<RoomType> roomTypesList = _context.RoomTypes.ToList<RoomType>();
        roomTypesList.Remove(roomType);
        // if Number exists
        RoomType ifRoomType = roomTypesList.Find(a => a.Name == editRoomType.Name);
        if (ifRoomType != null)
        {
            TempData["MotelEditOption"] = $"Room Type {editRoomType.Name} has already existed";
            return View(editRoomType);
        }

        // Create a BlobClient using the Blob storage connection string
        if (editRoomType.TypeImage != null)
        {
            string fileName = editRoomType.TypeImage.FileName.Trim();
            //var blobClient = new BlobClient( _storageConnectionString,  _storageContainerName, newAirport.LogoImage.FileName);
            // BlobContainerClient client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
            BlobClient file = _client.GetBlobClient(roomType.ImageUrl);

            await file.DeleteAsync();

            BlobClient newfile = _client.GetBlobClient(fileName);
            // Upload the image data to the Blob storage

            // Upload the image data to the Blob storage
            using (var stream = editRoomType.TypeImage.OpenReadStream())
            {
                await newfile.UploadAsync(stream);
            }


            //    var image = new BlobDto
            //           {
            //             FileName = newAirport.LogoImage.FileName,
            //             ContentType = newAirport.LogoImage.ContentType,
            //             URL = client.Uri.ToString()
            //           };
            roomType.ImageUrl = fileName;
        }
                roomType.Name = editRoomType.Name;
                roomType.Sleep = editRoomType.Sleep;
                roomType.Description = editRoomType.Description;
                _context.RoomTypes.Update(roomType);
        await _context.SaveChangesAsync();
        TempData["RoomTypeOption"] = $"{roomType.Name} has been Edited successfully";
        return RedirectToAction(nameof(Index));
    }


    catch (SystemException ex)
    {
        TempData["AirportEditOption"] = $"{ex.Message}";
        return View();
    }
}


// GET: AdminAirport/Delete/5
public async Task<IActionResult> Delete(int? id)
{
    try
    {
        var roomType = await _context.RoomTypes
            .FirstOrDefaultAsync(m => m.Id == id);
        string blobUrl = _client.Uri.ToString();
        RoomTypeInputModel newRoomType = new RoomTypeInputModel
        {
            Id = roomType.Id,
            Name = roomType.Name,

            Description = roomType.Description,

            Sleep = roomType.Sleep,
 
            ImageUrl = blobUrl + "/" + roomType.ImageUrl
        };
        return View(newRoomType);
    }
    catch (SystemException ex)
    {
        TempData["AirportDeleteOption"] = $"{ex.Message}";
        return View();
    }
}

// POST: AdminAirport/Delete/5
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    try
    {
        var roomType = await _context.RoomTypes.FirstOrDefaultAsync(a => a.Id == id);
        if (roomType != null)
        {
            //var blobClient = new BlobClient( _storageConnectionString,  _storageContainerName, newAirport.LogoImage.FileName);


            var bookedRecord = await _context.BookedRecords.Include("Room").FirstOrDefaultAsync(br => br.Room.RoomType.Name == roomType.Name && br.CheckoutDate > DateTime.Now);

            if (bookedRecord != null)
            {
                TempData["MotelDeleteOption"] = "There are rooms in use or will be in use related to this Type, can not delete it";

                string blobUrl = _client.Uri.ToString();
                RoomTypeInputModel newRoomType = new RoomTypeInputModel
                {
                    Id = roomType.Id,
                    Name = roomType.Name,

                    Description = roomType.Description,

                    Sleep = roomType.Sleep,

                    ImageUrl = blobUrl + "/" + roomType.ImageUrl
                    
                };
                return View(newRoomType);

            }

            BlobClient file = _client.GetBlobClient(roomType.ImageUrl);

            await file.DeleteAsync();

            _context.RoomTypes.Remove(roomType);
            TempData["RoomTypeOption"] = $"Motel {roomType.Name} has been deleted successfully";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        TempData["RoomTypeDeleteOption"] = $"RoomType {id} does not exist";
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





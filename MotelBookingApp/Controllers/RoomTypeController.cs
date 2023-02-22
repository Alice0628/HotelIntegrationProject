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


namespace MotelBookingApp.Controllers
{
    public class RoomTypeController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly ILogger<RoomTypeController> _logger;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public RoomTypeController(IConfiguration configuration, MotelDbContext context, ILogger<RoomTypeController> logger)
        {
            _context = context;
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
            _logger = logger;
            
        }

        // GET: AdminroomType
        public async Task<IActionResult> Index()
        {
            var roomTypes = await _context.Types.Include("Motel").ToListAsync();
            foreach (var rt in roomTypes)
            {
                rt.ImageUrl = Url.Content("~/images/" + rt.ImageUrl);
            }
            return View(roomTypes);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string searchWord)
        {
            if (searchWord == null)
            {   
                //to do
               return RedirectToAction("Index");
            }
            else
            {
                searchWord = searchWord.ToLower();
                List<RoomType> searcheRes = await _context.Types.Where(a => a.Name.ToLower().Contains(searchWord) || a.Sleep.ToString().Equals(searchWord)).ToListAsync();
                if (searcheRes.Count == 0)
                {
                    TempData["RoomTypeOption"] = "No search results";
                }
                foreach (var rt in searcheRes)
                {
                    rt.ImageUrl = Url.Content("~/images/" + rt.ImageUrl);
                }
                return View(searcheRes);
            }
        }
        // GET: AdminroomType/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                var roomType = await _context.Types.Include("Motel")
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (roomType == null)
                {
                    TempData["RoomType not exist"] = $"RoomType {id} does not exist";
                    return View();
                }

                // to do
                //roomType.ImageUrl = _client.Uri.ToString() + "/" + roomType.ImageUrl;

                //// Replace with the latitude and longitude of the location you want to center the map on
                //int latitudedegreePosition = roomType.Latitude.IndexOf('°');
                //string latitudeStr = roomType.Latitude.Substring(0, latitudedegreePosition);
                //double lat = Convert.ToDouble(latitudeStr);

                //int longitudedegreePosition = roomType.Longitude.IndexOf('°');
                //string longitudeStr = roomType.Longitude.Substring(0, longitudedegreePosition);
                //double lng = Convert.ToDouble(longitudeStr);
                //ViewData["APIKey"] = "AIzaSyBDEkj_2tqmLNrPm43uCX0_PjuWU0inMq4";
                //ViewData["Latitude"] = lat;
                //ViewData["Longitude"] = lng;
                roomType.ImageUrl = Url.Content("~/images/" + roomType.ImageUrl);
                return View(roomType);
            }
            catch (SystemException ex)
            {
                TempData["roomTypeOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: AdminroomType/Create
        public IActionResult Create()
        {   
            return View(new NewTypeVM());
            
        }

        // POST: AdminroomType/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewTypeVM newType)
        {
            try
            {
                RoomType ifroomType = await _context.Types.FirstOrDefaultAsync(a => a.Name == newType.Name);

                if (ifroomType != null)
                {
                    TempData["roomTypeCreateOption"] = $"roomType {newType.Name} has already existed";
                    return View(newType);
                }
                if (!ModelState.IsValid)
                {
                    return View(newType);
                }
                if (newType.Image != null && newType.Image.Length > 0)
                {
                    var fileName = Path.GetFileName(newType.Image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await newType.Image.CopyToAsync(fileStream);
                    }

                    Motel curMotel = await _context.Motels.FirstOrDefaultAsync(m => m.Name == "to do");

                    RoomType roomType = new RoomType()
                    {

                        Name = newType.Name,
                        Price = newType.Price,
                        ImageUrl = newType.Image.FileName,
                        Sleep = newType.Sleep,
                        Amount = newType.Amount,
                        Motel = curMotel,
                        Description = newType.Description
                        // to do
                        //Motel = new Motel()
                    };
                    _context.Types.Add(roomType);
                    await _context.SaveChangesAsync();
                }
  
                return RedirectToAction(nameof(Index));
            }
            catch (SystemException ex)
            {
                TempData["roomTypeCreateOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: AdminroomType/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var roomType = await _context.Types.Include("Motel").FirstOrDefaultAsync(f => f.Id == id);
                var editType = new NewTypeVM
                {
                    ImageUrl = Url.Content("~/images/" + roomType.ImageUrl),
                    Name = roomType.Name,
                    Price = roomType.Price,
                    Sleep = roomType.Sleep,
                    Amount = roomType.Amount,
                    Description = roomType.Description,
                    Image = null
                };

                return View(editType);
            }
            catch (SystemException ex)
            {
                TempData["roomTypeEditOption"] = $"{ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NewTypeVM newType, int id)
        {
             
            if (!ModelState.IsValid)
            {
                return View(newType);
            }
            try
            {
      
                RoomType roomType = await  _context.Types.FirstOrDefaultAsync(a => a.Id == id);
                if (newType.Image != null && newType.Image.Length > 0)
                {
                    var fileNameOld = Path.GetFileName(roomType.ImageUrl);
                    var fileNameNew = Path.GetFileName(newType.Image.FileName);
                    var filePathOld = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileNameOld);
                    var filePathNew = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileNameNew);
                    if (System.IO.File.Exists(filePathOld))
                    {
                        System.IO.File.Delete(filePathOld);
                    }
                    using (var fileStream = new FileStream(filePathNew, FileMode.Create))
                    {
                        await newType.Image.CopyToAsync(fileStream);
                    }
                    roomType.ImageUrl = newType.Image.FileName;
                }  
                    roomType.Name = newType.Name;
                    roomType.Price = newType.Price;
                    roomType.Sleep = newType.Sleep;
                    roomType.Amount = newType.Amount;
             
                _context.Types.Update(roomType);
                await _context.SaveChangesAsync();
                TempData["RoomTypeOption"] = $"{roomType.Name} has been Edited successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (SystemException ex)
            {
                TempData["roomTypeEditOption"] = $"{ex.Message}";
                return View();
            }
        }


        // GET: AdminroomType/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var roomType = await _context.Types.Include("Motel").FirstOrDefaultAsync(f => f.Id == id);
                var editType = new NewTypeVM
                {
                    ImageUrl = Url.Content("~/images/" + roomType.ImageUrl),
                    Name = roomType.Name,
                    Price = roomType.Price,
                    Sleep = roomType.Sleep,
                    Amount = roomType.Amount,
                    Description = roomType.Description,
                    Image = null
                };

                return View(editType);
            }
            catch (SystemException ex)
            {
                TempData["roomTypeEditOption"] = $"{ex.Message}";
                return View();
            }
        }

        // POST: AdminroomType/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var roomType = await _context.Types.Include("Motel").FirstOrDefaultAsync(a => a.Id == id);
                if (roomType != null)
                {
                    // todo 
                    //var blobClient = new BlobClient( _storageConnectionString,  _storageContainerName, newroomType.LogoImage.FileName);

                    //Flight flightOrigin = await _context.Flights.FirstOrDefaultAsync(f => f.Origin.Name == roomType.Name);
                    //Flight flightDestination = await _context.Flights.FirstOrDefaultAsync(f => f.Destination.Name == roomType.Name);

                    //if (flightOrigin != null || flightDestination != null)
                    //{
                    //    TempData["roomTypeDeleteOption"] = "There are flights related to this roomType, can not delete it";
                    //     roomType.ImageUrl = _client.Uri.ToString() + "/" + roomType.ImageUrl;
                    //    return View(roomType);
                    //}

                    //BlobClient file = _client.GetBlobClient(roomType.ImageUrl);

                    //await file.DeleteAsync();

                    var fileNameOld = Path.GetFileName(roomType.ImageUrl);
                   
                    var filePathOld = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileNameOld);
                   
                    if (System.IO.File.Exists(filePathOld))
                    {
                        System.IO.File.Delete(filePathOld);
                    }
                    _context.Types.Remove(roomType);
                    TempData["TypeOption"] = $"Room Type {roomType.Name} has been deleted successfully";
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                TempData["TypeDeleteOption"] = $"RoomType {id} does not exist";
                return View();

            }
            catch (SystemException ex)
            {
                TempData["TypeDeleteOption"] = $"{ex.Message}";
                return View();
            }
        }

    }
}





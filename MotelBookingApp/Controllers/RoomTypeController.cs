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
            return _context.Types != null ?
                       View(await _context.Types.Include("Motel").ToListAsync()) :
                       Problem("Entity set  is null.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string searchWord)
        {
            if (searchWord == null)
            {   
                //to do
                return _context.Types != null ?
                           View(await _context.Types.Include("Motel").Where(a => a.Motel.Name == "to do").ToListAsync()) :
                           Problem("Entity set is null.");
            }
            else
            {
                searchWord = searchWord.ToLower();
                List<RoomType> searcheRes = await _context.Types.Where(a => a.Name.ToLower().Contains(searchWord)).ToListAsync();
                if (searcheRes.Count == 0)
                {
                    TempData["RoomTypeOption"] = "No search results";
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
                var ImageUrl = "";
                if (newType.Image != null && newType.Image.Length > 0)
                {
                    var fileName = Path.GetFileName(newType.Image.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await newType.Image.CopyToAsync(fileStream);
                    }
                     ImageUrl = Url.Content("~/images/" + fileName);

                RoomType roomType = new RoomType()
                {

                    Name = newType.Name,
                    Price = newType.Price,
                    ImageUrl = ImageUrl,
                    Sleep = newType.Sleep,
                    Amount = newType.Amount,
                    Description = newType.Description,
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
              

                return View(roomType);
            }
            catch (SystemException ex)
            {
                TempData["roomTypeEditOption"] = $"{ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoomType roomType, int id)
        {
            if (!ModelState.IsValid)
            {
                return View(roomType);
            }
            try
            {
                roomType = _context.Types.Include("Motel").FirstOrDefault(a => a.Id == id);
                List<RoomType> roomTypesList = _context.Types.Include("Motel").ToList<RoomType>();
                roomTypesList.Remove(roomType);
                // if Number exists
                RoomType ifTypeName = roomTypesList.Find(a => a.Name == roomType.Name );
                if (ifTypeName != null)
                {
                    TempData["roomTypeEditOption"] = $"roomType Name {roomType.Name} has already existed";
                    return View(roomType);
                }
             
                //to do
                // Create a BlobClient using the Blob storage connection string
                //if (roomType.LogoImage != null)
                //{
                //    string fileName = roomType.LogoImage.FileName.Trim();
                //    //var blobClient = new BlobClient( _storageConnectionString,  _storageContainerName, newroomType.LogoImage.FileName);
                //    // BlobContainerClient client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
                //    BlobClient file = _client.GetBlobClient(roomType.ImageUrl);

                //    await file.DeleteAsync();

                //    BlobClient newfile = _client.GetBlobClient(fileName);
                //    // Upload the image data to the Blob storage

                //    // Upload the image data to the Blob storage
                //    using (var stream = editroomType.LogoImage.OpenReadStream())
                //    {
                //        await newfile.UploadAsync(stream);
                //    }


                    //    var image = new BlobDto
                    //           {
                    //             FileName = newroomType.LogoImage.FileName,
                    //             ContentType = newroomType.LogoImage.ContentType,
                    //             URL = client.Uri.ToString()
                    //           };
                     
         

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
                var roomType = await _context.Types.Include("Motel")
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (roomType == null)
                {
                    return NotFound();
                }
                //roomType.ImageUrl = _client.Uri.ToString() + "/" + roomType.ImageUrl;
                return View(roomType);
            }
            catch (SystemException ex)
            {
                TempData["TypeDeleteOption"] = $"{ex.Message}";
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





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
using System.Net;

namespace MotelBookingApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminMotelController : Controller
    {
        private readonly MotelDbContext _context;
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly BlobContainerClient _client;

        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public AdminMotelController(IConfiguration configuration, MotelDbContext context)
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
                       View(await _context.Motels.ToListAsync()) :
                       Problem("Entity set 'MotelBookingAppContext.Airports'  is null.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string searchWord)
        {
            if (searchWord == null)
            {
                return _context.Motels != null ?
                           View(await _context.Motels.ToListAsync()) :
                           Problem("Entity set 'MotelDbContext.Airports'  is null.");
            }
            else
            {
                searchWord = searchWord.ToLower();
                List<Motel> searcheRes = await _context.Motels.Where(a => a.Name.ToLower().Contains(searchWord) || a.City.ToLower().Contains(searchWord)).ToListAsync();
                if (searcheRes.Count == 0)
                {
                    TempData["MotelOption"] = "No search results";
                }
                return View(searcheRes);
            }
        }
        // GET: AdminAirport/Details/5
        public async Task<IActionResult> Detail(int? id)
        {
            try
            {
                var motelDetail = new MotelDetailModel();
                var motel = await _context.Motels
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (motel == null)
                {
                    TempData["MotelPotion"] = $"motel {id} does not exist";
                    return View();
                }
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
                if (await _context.Comments.Include("Motel").Where(c => c.Motel.Id == id).ToListAsync() != null)
                {
                    var comments = await _context.Comments.Include("Motel").Include("User").Where(c => c.Motel.Id == id).ToListAsync();
                    motelDetail.Comments = comments;
                }

                motelDetail.Motel = curMotel;
                return View(motelDetail);
            }
            catch (SystemException ex)
            {
                TempData["MotelOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: AdminAirport/Create
        public IActionResult Create()
        {
            return View(new MotelInputModel());
        }

        // POST: AdminAirport/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MotelInputModel newMotel)
        {
            try
            {
                Motel ifMotel = await _context.Motels.FirstOrDefaultAsync(a => a.Name == newMotel.Name && a.City == newMotel.City);

                if (ifMotel != null)
                {
                    TempData["MotelOption"] = $"Motel {newMotel.Name} in {newMotel.City}has already existed";
                    return View(newMotel);
                }
                if (!ModelState.IsValid)
                {
                    return View(newMotel);
                }
                string fileName = newMotel.MotelImage.FileName.Trim();
                // Create a BlobClient using the Blob storage connection string
                var blobClient = new BlobClient(_storageConnectionString, _storageContainerName, fileName);

                // Upload the image data to the Blob storage
                using (var stream = newMotel.MotelImage.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream);
                }
                Motel motel = new Motel()
                {
                    Name = newMotel.Name,

                    Address = newMotel.Address,

                    City = newMotel.City,

                    Province = newMotel.Province,

                    PostalCode = newMotel.PostalCode,

                    ImageUrl = newMotel.MotelImage.FileName
                };
                _context.Motels.Add(motel);
                await _context.SaveChangesAsync();
                TempData["MotlOption"] = $"{motel.Name} has been created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (SystemException ex)
            {
                TempData["MotelOption"] = $"{ex.Message}";
                return View();
            }
        }

        // GET: AdminAirport/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var motel = await _context.Motels.FirstOrDefaultAsync(m => m.Id == id);
                string blobUrl = _client.Uri.ToString();
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

                return View(newMotel);
            }
            catch (SystemException ex)
            {
                TempData["MotelOption"] = $"{ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MotelInputModel editMotel, int id)
        {
            if (!ModelState.IsValid)
            {
                return View(editMotel);
            }
            try
            {
                var motel = _context.Motels.FirstOrDefault(a => a.Id == id);
                List<Motel> motelsList = _context.Motels.ToList<Motel>();
                motelsList.Remove(motel);
                // if Number exists
                Motel ifMotel = motelsList.Find(a => a.Name == editMotel.Name && a.City == editMotel.City);
                if (ifMotel != null)
                {
                    TempData["MotelOption"] = $"Airport Name {editMotel.Name} has already existed";
                    return View(editMotel);
                }

                // Create a BlobClient using the Blob storage connection string
                if (editMotel.MotelImage != null)
                {
                    string fileName = editMotel.MotelImage.FileName.Trim();
                    //var blobClient = new BlobClient( _storageConnectionString,  _storageContainerName, newAirport.LogoImage.FileName);
                    // BlobContainerClient client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
                    BlobClient file = _client.GetBlobClient(motel.ImageUrl);

                    await file.DeleteAsync();

                    BlobClient newfile = _client.GetBlobClient(fileName);
                    // Upload the image data to the Blob storage

                    // Upload the image data to the Blob storage
                    using (var stream = editMotel.MotelImage.OpenReadStream())
                    {
                        await newfile.UploadAsync(stream);
                    }


                    //    var image = new BlobDto
                    //           {
                    //             FileName = newAirport.LogoImage.FileName,
                    //             ContentType = newAirport.LogoImage.ContentType,
                    //             URL = client.Uri.ToString()
                    //           };
                    motel.ImageUrl = fileName;
                }
                motel.Name = editMotel.Name;
                motel.Address = editMotel.Address;
                motel.City = editMotel.City;
                motel.Province = editMotel.Province;
                motel.PostalCode = editMotel.PostalCode;
                _context.Motels.Update(motel);
                await _context.SaveChangesAsync();
                TempData["MotelOption"] = $"{motel.Name} has been Edited successfully";
                return RedirectToAction(nameof(Index));
            }


            catch (SystemException ex)
            {
                TempData["MotelOption"] = $"{ex.Message}";
                return View();
            }
        }


        // GET: AdminAirport/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var motel = await _context.Motels
                    .FirstOrDefaultAsync(m => m.Id == id);
                string blobUrl = _client.Uri.ToString();
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
                return View(newMotel);
            }
            catch (SystemException ex)
            {
                TempData["MotelOption"] = $"{ex.Message}";
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
                var motel = await _context.Motels.FirstOrDefaultAsync(a => a.Id == id);
                if (motel != null)
                {
                    //var blobClient = new BlobClient( _storageConnectionString,  _storageContainerName, newAirport.LogoImage.FileName);


                    var bookedRecord = await _context.BookedRecords.Include("Room").FirstOrDefaultAsync(br => br.Room.Motel.Name == motel.Name && br.Room.Motel.City == motel.City && br.CheckoutDate > DateTime.Now);

                    if (bookedRecord != null)
                    {
                        TempData["MotelOption"] = "There are rooms in use or will be in use related to this Motel, can not delete it";

                        string blobUrl = _client.Uri.ToString();
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
                        return View(newMotel);

                    }

                    BlobClient file = _client.GetBlobClient(motel.ImageUrl);

                    await file.DeleteAsync();

                    _context.Motels.Remove(motel);
                    TempData["MotelOption"] = $"Motel {motel.Name} has been deleted successfully";
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                TempData["MotelOption"] = $"Motel {id} does not exist";
                return View();

            }
            catch (SystemException ex)
            {
                TempData["MotelOption"] = $"{ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> FavoriteMotelList()
        {
            ViewBag.Count = HttpContext.Session.GetString("Count");
            var favoriteMotelList = await _context.FavoriteMotelLists.Include("Owner").Include("Motel").ToListAsync();
            return View(favoriteMotelList);
        }
    }
}





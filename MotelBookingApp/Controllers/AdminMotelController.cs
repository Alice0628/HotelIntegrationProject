using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotelBookingApp.Data;
using MotelBookingApp.Models;
using MotelBookingApp.Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Azure.Storage.Blobs;
using System.Net;
//    public class AdminMotelController : Controller
//    {
//        private IMotelService _motelService;

//        public AdminMotelController(IMotelService repository)
//        {
        private readonly MotelDbContext _context;
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly BlobContainerClient _client;
       
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

        public AdminMotelController(IConfiguration configuration, MotelDbContext context)
//        [HttpGet]
//        public async Task<ActionResult> Index()
//        {
//            var motels = await _motelService.GetAll();
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);
//        }

            return _context.Motels != null ?
                       View(await _context.Motels.ToListAsync()) :
                       Problem("Entity set 'MotelBookingAppContext.Airports'  is null.");
        }
//                TempData["no result"] = $"no motel {searchKeyWord}";
//                return View();
//            }
//            foreach (var m in motels)
//            {
            if (searchWord == null)
//        }
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
                    TempData["motelOption"] = "No search results";
                }
                return View(searcheRes);
            }
        }
        // GET: AdminAirport/Details/5
        public async Task<IActionResult> Detail(int? id)
        {
            try
            {
                var motel = await _context.Motels
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (motel == null)
                {
                    TempData["motel not exist"] = $"motel {id} does not exist";
                    return View();
                }
                string blobUrl = _client.Uri.ToString();
                MotelInputModel curMotel = new MotelInputModel()
                {
                    Id = motel.Id,
                    Name = motel.Name,
                    Address = motel.Address,
                    Province = motel.Province,
                    City = motel.City,
                    PostalCode = motel.PostalCode,
                    ImageUrl = blobUrl + "/" + motel.ImageUrl
                };
                return View(curMotel);

//                MotelInputModel newMotel = new MotelInputModel();
//                return View(newMotel);

//        }

//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
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
//        public async Task<IActionResult> Edit(MotelInputModel newMotel)
                if (ifMotel != null)
                {
                    TempData["MotelCreateOption"] = $"Motel {newMotel.Name} in {newMotel.City}has already existed";
                    return View(newMotel);
                }
                if (!ModelState.IsValid)
                {
                    return View(newMotel);
                }
                string fileName = newMotel.MotelImage.FileName.Trim();
                // Create a BlobClient using the Blob storage connection string
                var blobClient = new BlobClient(_storageConnectionString, _storageContainerName, fileName);
//        {
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
                TempData["AirportOption"] = $"{motel.Name} has been created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (SystemException ex)
            {
                TempData["MotelCreateOption"] = $"{ex.Message}";
                return View();
            }
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

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
//        {
            catch (SystemException ex)
            {
                TempData["AirportEditOption"] = $"{ex.Message}";
                return View();
            }
//                return RedirectToAction("Delete");

        [HttpPost]
//}
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
                    TempData["MotelEditOption"] = $"Airport Name {editMotel.Name} has already existed";
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
                TempData["AirportOption"] = $"{motel.Name} has been Edited successfully";
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
                var motel = await _context.Motels.FirstOrDefaultAsync(a => a.Id == id);
                if (motel != null)
                {
                    //var blobClient = new BlobClient( _storageConnectionString,  _storageContainerName, newAirport.LogoImage.FileName);


                    var bookedRecord = await _context.BookedRecords.Include("Room").FirstOrDefaultAsync(br => br.Room.Motel.Name == motel.Name && br.Room.Motel.City == motel.City && br.CheckoutDate > DateTime.Now) ;

                    if (bookedRecord != null  )
                    {
                        TempData["MotelDeleteOption"] = "There are rooms in use or will be in use related to this Motel, can not delete it";
                       
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

                TempData["AirportDeleteOption"] = $"Airport {id} does not exist";
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




//        public async Task<IActionResult> Edit(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Edit(MotelInputModel newMotel)
//        {

//                if (ModelState.IsValid)
//                {
//                    bool res = _motelService.UpdateMotel(newMotel).Result;
//                    if (res)
//                    {
//                        return RedirectToAction("Index");
//                    }
//                    else
//                    {
//                        return View(newMotel);
//                    }
//                }
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(MotelInputModel newMotel)
//        {
//            if (ModelState.IsValid)
//            {
//                await _motelService.Add(newMotel);
//                return RedirectToAction(nameof(Index));
//            }
//            return View();

//        }

//        [HttpGet]
//        public async Task<IActionResult> Delete(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Detail(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }


//        [HttpPost,ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirm(int id)
//        {
//                var res = _motelService.DeleteMotel(id).Result;
//            if (res)
//                return RedirectToAction("index");
//            else
//                return RedirectToAction("Delete");

//        }
//    }
//}


using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Runtime.Internal;
using MotelBookingApp.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Diagnostics;
//using MotelBookingApp.Iservice;
////using Amazon.DyncamoDBv2.Model.Internal.MarshallTransformations;
//using MotelBookingApp.Data.ViewModels;

//namespace AwsTest.Controllers
//{
//    public class AdminMotelController : Controller
//    {
//        private IMotelService _motelService;

//        public AdminMotelController(IMotelService repository)
//        {
//            _motelService = repository;
//        }

//        [HttpGet]
//        public async Task<ActionResult> Index()
//        {
//            var motels = await _motelService.GetAll();
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);
//        }

//        [HttpPost]
//        public async Task<ActionResult> Index(string searchKeyWord)
//        {

//            List<Motel> motels = await _motelService.SearchItemAsync(searchKeyWord);
//            if (motels == null)
//            {
//                TempData["no result"] = $"no motel {searchKeyWord}";
//                return View();
//            }
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);
//        }



//        [HttpGet]
//        public IActionResult Create()
//        {

//                MotelInputModel newMotel = new MotelInputModel();
//                return View(newMotel);

//        }

//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Edit(MotelInputModel newMotel)
//        {

//                if (ModelState.IsValid)
//                {
//                    bool res = _motelService.UpdateMotel(newMotel).Result;
//                    if (res)
//                    {
//                        return RedirectToAction("Index");
//                    }
//                    else
//                    {
//                        return View(newMotel);
//                    }
//                }
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(MotelInputModel newMotel)
//        {
//            if (ModelState.IsValid)
//            {
//                await _motelService.Add(newMotel);
//                return RedirectToAction(nameof(Index));
//            }
//            return View();

//        }

//        [HttpGet]
//        public async Task<IActionResult> Delete(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Detail(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }


//        [HttpPost,ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirm(int id)
//        {
//                var res = _motelService.DeleteMotel(id).Result;
//            if (res)
//                return RedirectToAction("index");
//            else
//                return RedirectToAction("Delete");

//        }
//    }
//}


using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Runtime.Internal;
using MotelBookingApp.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Diagnostics;
//using MotelBookingApp.Iservice;
////using Amazon.DyncamoDBv2.Model.Internal.MarshallTransformations;
//using MotelBookingApp.Data.ViewModels;

//namespace AwsTest.Controllers
//{
//    public class AdminMotelController : Controller
//    {
//        private IMotelService _motelService;

//        public AdminMotelController(IMotelService repository)
//        {
//            _motelService = repository;
//        }

//        [HttpGet]
//        public async Task<ActionResult> Index()
//        {
//            var motels = await _motelService.GetAll();
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);
//        }

//        [HttpPost]
//        public async Task<ActionResult> Index(string searchKeyWord)
//        {

//            List<Motel> motels = await _motelService.SearchItemAsync(searchKeyWord);
//            if (motels == null)
//            {
//                TempData["no result"] = $"no motel {searchKeyWord}";
//                return View();
//            }
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);
//        }



//        [HttpGet]
//        public IActionResult Create()
//        {

//                MotelInputModel newMotel = new MotelInputModel();
//                return View(newMotel);

//        }

//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Edit(MotelInputModel newMotel)
//        {

//                if (ModelState.IsValid)
//                {
//                    bool res = _motelService.UpdateMotel(newMotel).Result;
//                    if (res)
//                    {
//                        return RedirectToAction("Index");
//                    }
//                    else
//                    {
//                        return View(newMotel);
//                    }
//                }
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(MotelInputModel newMotel)
//        {
//            if (ModelState.IsValid)
//            {
//                await _motelService.Add(newMotel);
//                return RedirectToAction(nameof(Index));
//            }
//            return View();

//        }

//        [HttpGet]
//        public async Task<IActionResult> Delete(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Detail(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }


//        [HttpPost,ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirm(int id)
//        {
//                var res = _motelService.DeleteMotel(id).Result;
//            if (res)
//                return RedirectToAction("index");
//            else
//                return RedirectToAction("Delete");

//        }
//    }
//}


using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Runtime.Internal;
using MotelBookingApp.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Diagnostics;
//using MotelBookingApp.Iservice;
////using Amazon.DyncamoDBv2.Model.Internal.MarshallTransformations;
//using MotelBookingApp.Data.ViewModels;

//namespace AwsTest.Controllers
//{
//    public class AdminMotelController : Controller
//    {
//        private IMotelService _motelService;

//        public AdminMotelController(IMotelService repository)
//        {
//            _motelService = repository;
//        }

//        [HttpGet]
//        public async Task<ActionResult> Index()
//        {
//            var motels = await _motelService.GetAll();
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);
//        }

//        [HttpPost]
//        public async Task<ActionResult> Index(string searchKeyWord)
//        {

//            List<Motel> motels = await _motelService.SearchItemAsync(searchKeyWord);
//            if (motels == null)
//            {
//                TempData["no result"] = $"no motel {searchKeyWord}";
//                return View();
//            }
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);
//        }



//        [HttpGet]
//        public IActionResult Create()
//        {

//                MotelInputModel newMotel = new MotelInputModel();
//                return View(newMotel);

//        }

//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Edit(MotelInputModel newMotel)
//        {

//                if (ModelState.IsValid)
//                {
//                    bool res = _motelService.UpdateMotel(newMotel).Result;
//                    if (res)
//                    {
//                        return RedirectToAction("Index");
//                    }
//                    else
//                    {
//                        return View(newMotel);
//                    }
//                }
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(MotelInputModel newMotel)
//        {
//            if (ModelState.IsValid)
//            {
//                await _motelService.Add(newMotel);
//                return RedirectToAction(nameof(Index));
//            }
//            return View();

//        }

//        [HttpGet]
//        public async Task<IActionResult> Delete(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Detail(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }


//        [HttpPost,ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirm(int id)
//        {
//                var res = _motelService.DeleteMotel(id).Result;
//            if (res)
//                return RedirectToAction("index");
//            else
//                return RedirectToAction("Delete");

//        }
//    }
//}


using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Runtime.Internal;
using MotelBookingApp.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Diagnostics;
//using MotelBookingApp.Iservice;
////using Amazon.DyncamoDBv2.Model.Internal.MarshallTransformations;
//using MotelBookingApp.Data.ViewModels;

//namespace AwsTest.Controllers
//{
//    public class AdminMotelController : Controller
//    {
//        private IMotelService _motelService;

//        public AdminMotelController(IMotelService repository)
//        {
//            _motelService = repository;
//        }

//        [HttpGet]
//        public async Task<ActionResult> Index()
//        {
//            var motels = await _motelService.GetAll();
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);
//        }

//        [HttpPost]
//        public async Task<ActionResult> Index(string searchKeyWord)
//        {

//            List<Motel> motels = await _motelService.SearchItemAsync(searchKeyWord);
//            if (motels == null)
//            {
//                TempData["no result"] = $"no motel {searchKeyWord}";
//                return View();
//            }
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);
//        }



//        [HttpGet]
//        public IActionResult Create()
//        {

//                MotelInputModel newMotel = new MotelInputModel();
//                return View(newMotel);

//        }

//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Edit(MotelInputModel newMotel)
//        {

//                if (ModelState.IsValid)
//                {
//                    bool res = _motelService.UpdateMotel(newMotel).Result;
//                    if (res)
//                    {
//                        return RedirectToAction("Index");
//                    }
//                    else
//                    {
//                        return View(newMotel);
//                    }
//                }
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(MotelInputModel newMotel)
//        {
//            if (ModelState.IsValid)
//            {
//                await _motelService.Add(newMotel);
//                return RedirectToAction(nameof(Index));
//            }
//            return View();

//        }

//        [HttpGet]
//        public async Task<IActionResult> Delete(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Detail(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }


//        [HttpPost,ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirm(int id)
//        {
//                var res = _motelService.DeleteMotel(id).Result;
//            if (res)
//                return RedirectToAction("index");
//            else
//                return RedirectToAction("Delete");

//        }
//    }
//}


using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Runtime.Internal;
using MotelBookingApp.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Diagnostics;
//using MotelBookingApp.Iservice;
////using Amazon.DyncamoDBv2.Model.Internal.MarshallTransformations;
//using MotelBookingApp.Data.ViewModels;

//namespace AwsTest.Controllers
//{
//    public class AdminMotelController : Controller
//    {
//        private IMotelService _motelService;

//        public AdminMotelController(IMotelService repository)
//        {
//            _motelService = repository;
//        }

//        [HttpGet]
//        public async Task<ActionResult> Index()
//        {
//            var motels = await _motelService.GetAll();
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);
//        }

//        [HttpPost]
//        public async Task<ActionResult> Index(string searchKeyWord)
//        {

//            List<Motel> motels = await _motelService.SearchItemAsync(searchKeyWord);
//            if (motels == null)
//            {
//                TempData["no result"] = $"no motel {searchKeyWord}";
//                return View();
//            }
//            foreach (var m in motels)
//            {
//                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
//            }
//            return View(motels);
//        }



//        [HttpGet]
//        public IActionResult Create()
//        {

//                MotelInputModel newMotel = new MotelInputModel();
//                return View(newMotel);

//        }

//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Edit(MotelInputModel newMotel)
//        {

//                if (ModelState.IsValid)
//                {
//                    bool res = _motelService.UpdateMotel(newMotel).Result;
//                    if (res)
//                    {
//                        return RedirectToAction("Index");
//                    }
//                    else
//                    {
//                        return View(newMotel);
//                    }
//                }
//            return View();
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(MotelInputModel newMotel)
//        {
//            if (ModelState.IsValid)
//            {
//                await _motelService.Add(newMotel);
//                return RedirectToAction(nameof(Index));
//            }
//            return View();

//        }

//        [HttpGet]
//        public async Task<IActionResult> Delete(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Detail(int id)
//        {
//            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

//            if (newMotel == null)
//                return View(new MotelInputModel());
//            else
//                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
//            return View(newMotel);
//        }


//        [HttpPost,ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirm(int id)
//        {
//                var res = _motelService.DeleteMotel(id).Result;
//            if (res)
//                return RedirectToAction("index");
//            else
//                return RedirectToAction("Delete");

//        }
//    }
//}


using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Runtime.Internal;
using MotelBookingApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MotelBookingApp.Iservice;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using MotelBookingApp.Data.ViewModels;
using MotelBookingApp.Data.ViewModels;
using MotelBookingApp.Iservice;
using MotelBookingApp.Models;

namespace AwsTest.Controllers
{

    public class AdminMotelController : Controller
    {
        private IAdminMotelService _motelService;

        public AdminMotelController(IAdminMotelService repository)
        {
            _context = context;
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
             
            _client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
        }

        // GET: AdminAirport
        public async Task<IActionResult> Index()
        {
            var motels = await _motelService.GetAllMotels();
            foreach (Motel m in motels)
            {
                m.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{m.ImageUrl}";
            }
            return View(motels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string searchWord)
        {

            List<Motel> motels = await _motelService.SearchItemAsync(searchKeyWord);
            if (motels == null)
            {
                TempData["no result"] = $"no motel {searchKeyWord}";
                return View();
            }
            foreach (Motel m in motels)
            {
                m.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{m.ImageUrl}";
            }
            catch (SystemException ex)
            {
                TempData["MotelOption"] = $"{ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            MotelInputModel newmotel = await _motelService.GetEditMotel(id);

            if (newmotel == null)
                return View(new MotelInputModel());
            else
                
            return View(newmotel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            MotelInputModel newMotel = new MotelInputModel();
            int maxIndex = _motelService.FindMaxIndex().Result;
            newMotel.Id = maxIndex + 1;
            return View(newMotel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            TempData["id"] = id;
            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

            if (newMotel == null)
                return View(new MotelInputModel());
            else
                return View(newMotel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MotelInputModel newMotel)
        {
            bool res = _motelService.UpdateMotel(newMotel).Result;
            if (res)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(newMotel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MotelInputModel newMotel)
        {
            if (newMotel.MotelImage == null)
            {
                TempData["noImage"] = $"No Image";

                return View(newMotel);
            }
            await _motelService.AddMotel(newMotel);
            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Motel motel = await _motelService.SingleMotel(id);

            if (motel == null)
                return View(new Motel());
            else
            {
                motel.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{motel.ImageUrl}";

                return View(motel);
            }
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var res = _motelService.DeleteMotel(id).Result;
            if (res)
                return RedirectToAction("index");
            else
                return RedirectToAction("Delete");

        }
    }
}





















































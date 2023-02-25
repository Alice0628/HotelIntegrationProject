////using Amazon.DynamoDBv2.Model;
////using Amkcazon.DynamoDBv2;
////using Amazon.Runtime.Internal;
//using MotelBookingApp.Models;
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

namespace MotelBookingApp.Controllers
{
    public class AdminMotelController : Controller
    {
        private IAdminMotelService _motelService;

        public AdminMotelController(IAdminMotelService repository)
        {
            _motelService = repository;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var motels = await _motelService.GetAllMotels();
            foreach (Motel m in motels)
            {
                m.ImageUrl = $"https://motelbooking.s3.us-east-2.amazonaws.com/{m.ImageUrl}";
            }
            return View(motels);
        }

        [HttpPost]
        public async Task<ActionResult> Index(string searchKeyWord)
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
            return View(motels);
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





















































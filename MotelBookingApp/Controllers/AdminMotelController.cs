//using Amazon.DynamoDBv2.Model;
//using Amkcazon.DynamoDBv2;
//using Amazon.Runtime.Internal;
using MotelBookingApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MotelBookingApp.Iservice;
//using Amazon.DyncamoDBv2.Model.Internal.MarshallTransformations;
using MotelBookingApp.Data.ViewModels;

namespace AwsTest.Controllers
{
    public class AdminMotelController : Controller
    {
        private IMotelService _motelService;

        public AdminMotelController(IMotelService repository)
        {
            _motelService = repository;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var motels = await _motelService.GetAll();
            foreach (var m in motels)
            {
                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
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
            foreach (var m in motels)
            {
                m.ImageUrl = Url.Content("~/images/" + m.ImageUrl);
            }
            return View(motels);
        }



        [HttpGet]
        public IActionResult Create()
        {
    
                MotelInputModel newMotel = new MotelInputModel();
                return View(newMotel);
         
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

            if (newMotel == null)
                return View(new MotelInputModel());
            else
                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
            return View(newMotel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MotelInputModel newMotel)
        {
          
                if (ModelState.IsValid)
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MotelInputModel newMotel)
        {
            if (ModelState.IsValid)
            {
                await _motelService.Add(newMotel);
                return RedirectToAction(nameof(Index));
            }
            return View();

        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

            if (newMotel == null)
                return View(new MotelInputModel());
            else
                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
            return View(newMotel);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            MotelInputModel newMotel = await _motelService.GetEditMotel(id);

            if (newMotel == null)
                return View(new MotelInputModel());
            else
                newMotel.ImageUrl = Url.Content("~/images/" + newMotel.ImageUrl);
            return View(newMotel);
        }


        [HttpPost,ActionName("Delete")]
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



























 
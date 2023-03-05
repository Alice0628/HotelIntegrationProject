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
    public class StaffMotelController : Controller
    {

        private readonly MotelDbContext _context;
    private readonly string _storageConnectionString;
    private readonly string _storageContainerName;
    private readonly BlobContainerClient _client;

    private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

    public StaffMotelController(IConfiguration configuration, MotelDbContext context)
    {
        _context = context;
        _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
        _storageContainerName = configuration.GetValue<string>("BlobContainerName");
        _client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
    }
        public async Task<IActionResult> Index(int? id)
        {
            try
            {
                var motelDetail = new MotelDetailModel();
                var motel = await _context.Motels
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (motel == null)
                {
                    TempData["motel not exist"] = $"motel {id} does not exist";
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
    }
}

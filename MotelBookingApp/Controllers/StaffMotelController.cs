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
using Microsoft.AspNetCore.Identity;

namespace MotelBookingApp.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffMotelController : Controller
    {

    private readonly MotelDbContext _context;
    private readonly string _storageConnectionString;
    private readonly string _storageContainerName;
    private readonly BlobContainerClient _client;
    private readonly UserManager<AppUser> _userManager;

        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;

    public StaffMotelController(IConfiguration configuration, UserManager<AppUser> userManager, MotelDbContext context)
    {
        _context = context;
        _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
        _storageContainerName = configuration.GetValue<string>("BlobContainerName");
        _client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
        _userManager = userManager;
    }
        public async Task<IActionResult> Index()
        {
            try
            {
                var userName = _userManager.GetUserName(User);
                var user = await _context.Users.Include("Motel").Where(u => u.UserName == userName).FirstOrDefaultAsync();
                var motelDetail = new MotelDetailModel();
                var motel = await _context.Motels
                    .FirstOrDefaultAsync(m => m.Name ==user.Motel.Name);
                if (motel == null)
                {
                    TempData["motel not exist"] = $"motel {motel.Name} does not exist";
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
                var comments = await _context.Comments.Include("Motel").Include("User").Where(c => c.Motel.Name == motel.Name).ToListAsync();
                if (comments.Count > 0)
                {
                    motelDetail.Comments = comments;
                    int totalScore = 0;
                    foreach (var c in comments)
                    { 
                        totalScore += int.Parse(c.Score);
                    }
                    var score = totalScore/ comments.Count;
                    motelDetail.Score = score;
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

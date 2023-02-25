
using Microsoft.AspNetCore.Mvc;
using MotelBookingApp.Iservice;
using MotelBookingApp.Models;



namespace MotelBookingApp.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IAdminMotelService _adminMotelService;

        public CustomerController(IAdminMotelService adminMotelService)
        {
            _adminMotelService= adminMotelService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchResMotel()
        {
            List<Motel> searchMotels =_adminMotelService.SearchItemAsync("Laval").Result;
            return View(searchMotels);
        }



    }
}

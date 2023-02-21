using Microsoft.AspNetCore.Mvc;

namespace MotelBookingApp.Controllers
{
    public class AdminMotelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

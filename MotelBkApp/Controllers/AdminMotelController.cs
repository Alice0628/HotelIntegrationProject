using Microsoft.AspNetCore.Mvc;

namespace MotelBkApp.Controllers
{
    public class AdminMotelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

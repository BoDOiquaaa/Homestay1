using Microsoft.AspNetCore.Mvc;

namespace Homestay1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

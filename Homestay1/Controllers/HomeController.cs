using Homestay1.Models;
using Homestay1.Repositories;
using Homestay1.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Homestay1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeRepository _homeRepo;
        public HomeController(IHomeRepository homeRepo)
        {
            _homeRepo = homeRepo;
        }

        public async Task<IActionResult> Index()
        {
            var homestays = await _homeRepo.GetAllHomestaysAsync();
            return View(homestays);
        }
    }
}
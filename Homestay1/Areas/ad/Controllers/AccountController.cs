using Homestay1.Repositories;
using Homestay1.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Homestay1.Areas.ad.Controllers
{
    [Area("ad")]
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepo;

        public AccountController(IAccountRepository accountRepo)
        {
            _accountRepo = accountRepo;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // trả về view chứa form AJAX
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LoginAjax([FromBody] LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return Json(new { success = false, errors });
            }

            var user = await _accountRepo.AuthenticateAsync(vm.Email, vm.Password);
            if (user == null)
            {
                return Json(new { success = false, errors = new[] { "Email hoặc mật khẩu không đúng." } });
            }

            if (user.RoleID != 2)
            {
                return Json(new { success = false, errors = new[] { "Bạn không có quyền truy cập." } });
            }

            // lưu session
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetInt32("RoleID", user.RoleID);

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Users", new { area = "ad" }) });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
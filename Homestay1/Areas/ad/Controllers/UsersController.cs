// Controllers/UsersController.cs
using Homestay1.Data;
using Homestay1.Models.Entities;
using Homestay1.Repositories;
using Homestay1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Homestay1.Areas.ad.Controllers
{
    [Area("ad")]
    public class UsersController : Controller
    {
        private readonly IUserRepository _repo;
        private readonly ApplicationDbContext _db;

        public UsersController(IUserRepository repo, ApplicationDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        /// <summary>
        /// Đổ danh sách Roles từ DB vào ViewBag.Roles
        /// </summary>
        private async Task PopulateRolesAsync()
        {
            var roles = await _db.Roles
                                 .OrderBy(r => r.RoleName)
                                 .Select(r => new SelectListItem
                                 {
                                     Value = r.RoleID.ToString(),
                                     Text = r.RoleName
                                 })
                                 .ToListAsync();

            ViewBag.Roles = new SelectList(roles, "Value", "Text");
        }

        // GET: /ad/Users/Create
        public async Task<IActionResult> Create()
        {
            await PopulateRolesAsync();
            return View(new UserViewModel());
        }

        // POST: /ad/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel vm)
        {
            // 1. Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                await PopulateRolesAsync();
                return View(vm);
            }

            // 2. Kiểm tra quyền nếu cần (ví dụ chỉ RoleID 2 được thêm)
            if (vm.RoleID != 2)
            {
                ModelState.AddModelError("RoleID", "Bạn không có quyền thêm user với vai trò này.");
                await PopulateRolesAsync();
                return View(vm);
            }

            // 3. Kiểm tra trùng Email
            bool exists = await _db.Users.AnyAsync(u => u.Email == vm.Email);
            if (exists)
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại.");
                await PopulateRolesAsync();
                return View(vm);
            }

            // 4. Tạo entity và lưu
            var user = new User
            {
                RoleID = vm.RoleID,
                FullName = vm.FullName,
                Email = vm.Email,
                Password = vm.Password, // nhớ hash password ở production
                Phone = vm.Phone
            };
            await _repo.AddAsync(user);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index(string search)
        {
            ViewBag.Search = search;
            var list = await _repo.GetAllAsync(search);
            return View(list);
        }

       

        public async Task<IActionResult> Edit(int id)
        {
            var u = await _repo.GetByIdAsync(id);
            if (u == null) return NotFound();
            await PopulateRolesAsync();
            var vm = new UserViewModel
            {
                UserID = u.UserID,
                RoleID = u.RoleID,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel vm)
        {
            if (!ModelState.IsValid) { await PopulateRolesAsync(); return View(vm); }
            var u = await _repo.GetByIdAsync(vm.UserID.Value);
            u.RoleID = vm.RoleID;
            u.FullName = vm.FullName;
            u.Email = vm.Email;
            u.Phone = vm.Phone;
            await _repo.UpdateAsync(u);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
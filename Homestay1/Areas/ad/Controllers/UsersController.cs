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
        /// Đổ danh sách Roles từ DB vào ViewBag.Roles dưới dạng SelectList
        /// </summary>
        private async Task PopulateRolesAsync(int? selectedRoleID = null)
        {
            var allRoles = await _db.Roles.OrderBy(r => r.RoleName).ToListAsync();
            if (selectedRoleID.HasValue)
                ViewBag.Roles = new SelectList(allRoles, "RoleID", "RoleName", selectedRoleID.Value);
            else
                ViewBag.Roles = new SelectList(allRoles, "RoleID", "RoleName");
        }

        // GET: /ad/Users/Create
        public async Task<IActionResult> Create()
        {
            await PopulateRolesAsync();
            return View(new UserViewModel());
        }

        // POST: /ad/Users/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel vm)
        {
            // Manual validation
            var errors = new List<string>();
            if (vm.RoleID <= 0)
            {
                errors.Add("Vui lòng chọn vai trò");
                ModelState.AddModelError("RoleID", "Vui lòng chọn vai trò");
            }
            if (string.IsNullOrWhiteSpace(vm.FullName))
            {
                errors.Add("Vui lòng nhập họ và tên");
                ModelState.AddModelError("FullName", "Vui lòng nhập họ và tên");
            }
            if (string.IsNullOrWhiteSpace(vm.Email))
            {
                errors.Add("Vui lòng nhập email");
                ModelState.AddModelError("Email", "Vui lòng nhập email");
            }
            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                errors.Add("Vui lòng nhập mật khẩu");
                ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu");
            }

            if (errors.Any())
            {
                await PopulateRolesAsync(vm.RoleID);
                return View(vm);
            }

            // Email format
            if (!IsValidEmail(vm.Email))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ");
                await PopulateRolesAsync(vm.RoleID);
                return View(vm);
            }

            // Duplicate email
            if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại");
                await PopulateRolesAsync(vm.RoleID);
                return View(vm);
            }

            // Role exists
            if (!await _db.Roles.AnyAsync(r => r.RoleID == vm.RoleID))
            {
                ModelState.AddModelError("RoleID", "Vai trò không tồn tại");
                await PopulateRolesAsync(vm.RoleID);
                return View(vm);
            }

            // Create entity
            var user = new User
            {
                RoleID = vm.RoleID,
                FullName = vm.FullName.Trim(),
                Email = vm.Email.Trim().ToLower(),
                Password = vm.Password, // TODO: hash
                Phone = vm.Phone?.Trim(),
                CreatedAt = DateTime.Now
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thêm user thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /ad/Users
        public async Task<IActionResult> Index(string search)
        {
            ViewBag.Search = search;
            var list = await _repo.GetAllAsync(search);
            return View(list);
        }

        // GET: /ad/Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) return BadRequest();
            var u = await _repo.GetByIdAsync(id);
            if (u == null) return NotFound();

            await PopulateRolesAsync(u.RoleID);
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

        // POST: /ad/Users/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel vm)
        {
            var errors = new List<string>();
            if (!vm.UserID.HasValue || vm.UserID <= 0)
            {
                errors.Add("UserID không hợp lệ");
                ModelState.AddModelError("UserID", "UserID không hợp lệ");
            }
            if (vm.RoleID <= 0)
            {
                errors.Add("Vui lòng chọn vai trò");
                ModelState.AddModelError("RoleID", "Vui lòng chọn vai trò");
            }
            if (string.IsNullOrWhiteSpace(vm.FullName))
            {
                errors.Add("Vui lòng nhập họ và tên");
                ModelState.AddModelError("FullName", "Vui lòng nhập họ và tên");
            }
            if (string.IsNullOrWhiteSpace(vm.Email))
            {
                errors.Add("Vui lòng nhập email");
                ModelState.AddModelError("Email", "Vui lòng nhập email");
            }
            if (errors.Any())
            {
                await PopulateRolesAsync(vm.RoleID);
                return View(vm);
            }
            if (!IsValidEmail(vm.Email))
            {
                ModelState.AddModelError("Email", "Email không hợp lệ");
                await PopulateRolesAsync(vm.RoleID);
                return View(vm);
            }

            var u = await _repo.GetByIdAsync(vm.UserID.Value);
            if (u == null) return NotFound();
            if (await _db.Users.AnyAsync(x => x.Email == vm.Email && x.UserID != vm.UserID))
            {
                ModelState.AddModelError("Email", "Email này đã tồn tại");
                await PopulateRolesAsync(vm.RoleID);
                return View(vm);
            }
            if (!await _db.Roles.AnyAsync(r => r.RoleID == vm.RoleID))
            {
                ModelState.AddModelError("RoleID", "Vai trò không tồn tại");
                await PopulateRolesAsync(vm.RoleID);
                return View(vm);
            }

            u.RoleID = vm.RoleID;
            u.FullName = vm.FullName.Trim();
            u.Email = vm.Email.Trim().ToLower();
            u.Phone = vm.Phone?.Trim();

            _db.Users.Update(u);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Cập nhật user thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /ad/Users/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try { var addr = new System.Net.Mail.MailAddress(email); return addr.Address == email; }
            catch { return false; }
        }
    }
}
using Homestay1.Data;
using Homestay1.Models;
using Homestay1.Repositories;
using Homestay1.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Homestay1.Controllers
{
    [Area("ad")]
    public class HomestaysController : Controller
    {
        private readonly IHomestayRepository _repo;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public HomestaysController(IHomestayRepository repo, IWebHostEnvironment env, ApplicationDbContext context)
        {
            _repo = repo;
            _env = env;
            _context = context;
        }

        public async Task<IActionResult> Index(string search)
        {
            ViewData["CurrentFilter"] = search;
            ViewData["Success"] = TempData["Success"];
            var list = await _repo.GetAllAsync(search);
            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            var homestay = await _repo.GetByIdWithRoomsAsync(id);
            if (homestay == null) return NotFound();
            return View(homestay);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile? ImageFile)
        {
            var homestay = new Homestay
            {
                OwnerID = int.Parse(Request.Form["OwnerID"]),
                Name = Request.Form["Name"],
                Address = Request.Form["Address"],
                Description = Request.Form["Description"]
            };
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "images");
                var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await ImageFile.CopyToAsync(stream);
                homestay.ImageUrl = "/images/" + fileName;
            }

            await _repo.AddAsync(homestay);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, redirectUrl = Url.Action("Index") });

            TempData["Success"] = "Thêm homestay thành công";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var homestay = await _repo.GetByIdAsync(id);
            if (homestay == null) return NotFound();
            return View(homestay);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int HomestayID, [FromForm] Homestay homestay, IFormFile ImageFile)
        {
            var existing = await _context.Homestays.FindAsync(HomestayID);
            if (existing == null) return NotFound();

            existing.Name = homestay.Name;
            existing.Address = homestay.Address;
            existing.Description = homestay.Description;
            existing.OwnerID = homestay.OwnerID;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var path = Path.Combine(_env.WebRootPath, "Img", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                existing.ImageUrl = "/Img/" + fileName;
            }

            _context.Homestays.Update(existing);
            await _context.SaveChangesAsync();

            return Json(new { success = true, redirectUrl = Url.Action("Index") });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var homestay = await _repo.GetByIdAsync(id);
            if (homestay == null) return NotFound();
            return View(homestay);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repo.DeleteAsync(id);
            TempData["Success"] = "Xóa homestay thành công";
            return RedirectToAction("Index");
        }
    }
}

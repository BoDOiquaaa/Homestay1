// ViewModels/UserViewModel.cs
using Homestay1.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Homestay1.ViewModels
{
    public class UserViewModel
    {
        public int? UserID { get; set; }
        public int RoleID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }

        // danh sách dropdown roles
        public IEnumerable<SelectListItem> Roles { get; set; }
    }
}
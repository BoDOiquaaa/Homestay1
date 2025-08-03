using Homestay1.Models;
using Homestay1.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Homestay1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts)
            : base(opts) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Homestay> Homestays { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình cho bảng Homestay
            builder.Entity<Homestay>(entity =>
            {
                entity.HasKey(h => h.HomestayID);
                entity.Property(h => h.Name).HasMaxLength(100).IsRequired();
                entity.Property(h => h.Address).HasMaxLength(200).IsRequired();
                entity.Property(h => h.ImageUrl).HasMaxLength(255);
                entity.Property(h => h.MapUrl).HasMaxLength(500); // Cấu hình cho MapUrl
                entity.Property(h => h.CreatedAt).HasDefaultValueSql("GETDATE()");

                // Quan hệ với Room
                entity.HasMany(h => h.Rooms)
                      .WithOne(r => r.Homestay)
                      .HasForeignKey(r => r.HomestayID);
            });

            // Cấu hình cho bảng Room
            builder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.RoomID);
                entity.Property(r => r.RoomName).HasMaxLength(100).IsRequired();
                entity.Property(r => r.PricePerNight).HasColumnType("decimal(10,2)");
                entity.Property(r => r.Status).HasMaxLength(20).HasDefaultValue("Available");
                entity.Property(r => r.ImageUrl).HasMaxLength(255);
            });

            // seed roles nếu cần
            builder.Entity<Role>().HasData(
                new Role { RoleID = 1, RoleName = "Admin" },
                new Role { RoleID = 2, RoleName = "Owner" },
                new Role { RoleID = 3, RoleName = "Staff" },
                new Role { RoleID = 4, RoleName = "Customer" }
            );
        }
    }
}
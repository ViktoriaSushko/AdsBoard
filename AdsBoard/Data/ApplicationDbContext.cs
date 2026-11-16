using AdsBoard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdsBoard.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Advertisement> Advertisements => Set<Advertisement>();
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Category>().HasData(
                new Category() { Id = 1, Name = "Компютери" },
                  new Category() { Id = 2, Name = "Автомобілі" },
                   new Category() { Id = 3, Name = "Телефони" }
                );
            builder.Entity<Advertisement>().HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Advertisement>().HasOne(a => a.Category).WithMany(c=>c.Advertisements).HasForeignKey(a => a.CategoryId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}

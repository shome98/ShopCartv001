using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Retail_MVC.Models;

namespace Retail_MVC.DataAccess.Data
{
    public class ApplicationDbContext:IdentityDbContext<IdentityUser>
	{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
            
        }

        public DbSet<Category> categories { get; set; }
        public DbSet<Product> products { get; set; }

        public DbSet<Vendor> vendors { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }

        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "sci-fi", DisplayOrder = 2 },
                new Category { Id = 3, Name = "horror", DisplayOrder = 3 }
                );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id=1,Name="product1",Description="desc1",Price=700.00,  CategoryId=1,ImageUrl="" },
                new Product { Id = 2, Name = "product2", Description = "desc2", Price = 800.00,CategoryId=1, ImageUrl = "" },
                new Product { Id = 3, Name = "product3", Description = "desc3", Price = 900.00 ,  CategoryId =2, ImageUrl = "" }


                );
            modelBuilder.Entity<Vendor>().HasData(
                new Vendor { Id = 1, Name = "Vendor1", StreetAddress="xyz1",City="abc1",State="state1",PostalCode="876543",PhoneNumber="9876543210"},
               new Vendor { Id = 2, Name = "Vendor2", StreetAddress = "xyz2", City = "abc2", State = "state2", PostalCode = "876542", PhoneNumber = "9876543211" },
               new Vendor { Id = 3, Name = "Vendor3", StreetAddress = "xyz3", City = "abc3", State = "state3", PostalCode = "876541", PhoneNumber = "9876543212" }
                );
        }
    }
}

using EfCoreDeleteExample.Models;
using Microsoft.EntityFrameworkCore;

namespace EfCoreDeleteExample;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product 
            { 
                Id = 1, 
                Name = "Product 1", 
                Price = 100, 
                Stock = 10, 
                IsActive = true, 
                CreatedDate = DateTime.UtcNow 
            },
            new Product 
            { 
                Id = 2, 
                Name = "Product 2", 
                Price = 200, 
                Stock = 20, 
                IsActive = true, 
                CreatedDate = DateTime.UtcNow 
            }
        );
    }
}
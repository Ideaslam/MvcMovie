using Microsoft.EntityFrameworkCore;
using MvcMovie.Entities;

namespace MvcMovie.Data;

 

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

    public DbSet<Product> Products { get; set; }
}
using KrishiBondhu___Smart_Agricultural_Assistance_Platform.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Farmer> Farmers { get; set; }

    public DbSet<Farm> Farms { get; set; }

    public DbSet<Crop> Crops { get; set; }

    public DbSet<Cultivation> Cultivations { get; set; }

    public DbSet<SoilTest> SoilTests { get; set; }

    public DbSet<Disease> Diseases { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cultivation date fields
        modelBuilder.Entity<Cultivation>()
            .Property(c => c.PlantingDate)
            .HasColumnType("timestamp without time zone");

        modelBuilder.Entity<Cultivation>()
            .Property(c => c.HarvestDate)
            .HasColumnType("timestamp without time zone");

        // Soil Test date field
        modelBuilder.Entity<SoilTest>()
            .Property(s => s.TestDate)
            .HasColumnType("timestamp without time zone");
    }
}
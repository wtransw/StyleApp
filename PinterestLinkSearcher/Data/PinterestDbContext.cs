using Microsoft.EntityFrameworkCore;
using PinterestLinkSearcher.Models;

namespace PinterestLinkSearcher.Data;

public sealed class PinterestDbContext(DbContextOptions<PinterestDbContext> options) : DbContext(options)
{
    public DbSet<PinterestPin> Pins => Set<PinterestPin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var pin = modelBuilder.Entity<PinterestPin>();
        pin.ToTable("PinterestPins");
        pin.HasKey(item => item.Id);
        pin.Property(item => item.Id).ValueGeneratedOnAdd();
        pin.Property(item => item.DateTime).HasColumnName("datetime").HasColumnType("datetime").IsRequired();
        pin.Property(item => item.Url).HasColumnType("nvarchar(2048)").IsRequired();
        pin.Property(item => item.Board).HasColumnType("nvarchar(200)").IsRequired();
        pin.Property(item => item.Tags).HasColumnType("nvarchar(max)").IsRequired();
        pin.Property(item => item.UrlHash).HasColumnType("binary(32)").IsRequired();
        pin.HasIndex(item => item.UrlHash).IsUnique();
    }
}

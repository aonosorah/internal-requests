using InternalRequests.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternalRequests.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Demand> Demands => Set<Demand>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Demand>(entity =>
        {
            entity.ToTable("Demands");

            entity.HasKey(d => d.Id);

            entity.Property(d => d.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(d => d.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(d => d.RequestDate)
                .IsRequired();

            entity.Property(d => d.Sector)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(d => d.Status)
                .IsRequired();

            entity.Property(d => d.Priority)
                .IsRequired();
        });
    }
}

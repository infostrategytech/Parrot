using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parrot.Application.Models;
using Parrot.Domain.Entities;

namespace Parrot.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Agent> Agents => Set<Agent>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.BusinessName).HasMaxLength(256).IsRequired();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        builder.Entity<Agent>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).HasMaxLength(256).IsRequired();
            entity.Property(a => a.Description).HasMaxLength(2000).IsRequired();
            entity.Property(a => a.MascotUrl).HasMaxLength(2048).IsRequired();
            entity.Property(a => a.UserId).IsRequired();
            entity.Property(a => a.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(a => a.UserId);
        });
    }
}

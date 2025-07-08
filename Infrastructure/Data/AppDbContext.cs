using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Workspace> Workspaces { get; set; }
    public DbSet<Note> Notes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Workspace → ApplicationUser (UserId)
        modelBuilder.Entity<Workspace>()
            .HasOne(w => w.User)
            .WithMany(u => u.Workspaces)
            .HasForeignKey(w => w.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Note → Workspace
        modelBuilder.Entity<Note>()
            .HasOne(n => n.Workspace)
            .WithMany(w => w.Notes)
            .HasForeignKey(n => n.WorkspaceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
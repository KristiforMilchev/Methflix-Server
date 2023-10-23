using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Context;

public class MethflixContext : DbContext
{
    
    public MethflixContext(DbContextOptions<MethflixContext> options) : base(options) { }
    
    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccessRole> AccessRoles { get; set; }
    public DbSet<AccessKey> AccessKeys { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<DTorrent> DTorrents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgres");
  
        
        // Configure auto-increment for the IDs
        modelBuilder.Entity<AccessKey>().Property(a => a.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<AccessRole>().Property(a => a.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Account>().Property(a => a.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Category>().Property(a => a.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<DTorrent>().Property(a => a.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Movie>().Property(a => a.Id).ValueGeneratedOnAdd();

        // Define relationships
        modelBuilder.Entity<Account>()
            .HasMany(a => a.Keys)
            .WithOne(k => k.Account)
            .HasForeignKey(k => k.AccountId);

        modelBuilder.Entity<AccessRole>()
            .HasMany(ar => ar.Accounts)
            .WithOne(a => a.AccessRole)
            .HasForeignKey(a => a.AccessRoleId);

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Movies)
            .WithOne(m => m.Category)
            .HasForeignKey(m => m.CategoryId);

        modelBuilder.Entity<Movie>()
            .HasOne(m => m.Category)
            .WithMany(c => c.Movies)
            .HasForeignKey(m => m.CategoryId);

        modelBuilder.Entity<DTorrent>()
            .HasOne(t => t.RequestedBy)
            .WithMany(a => a.DTorrents)
            .HasForeignKey(t => t.RequestedById);
    }
}

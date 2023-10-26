using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Context;

public partial class MethflixContext : DbContext
{
    public MethflixContext()
    {
    }

    public MethflixContext(DbContextOptions<MethflixContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessKey> AccessKeys { get; set; }

    public virtual DbSet<AccessRole> AccessRoles { get; set; }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Dtorrent> Dtorrents { get; set; }

    public virtual DbSet<FileExtension> FileExtensions { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Upload> Uploads { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AccessKeys_pk");

            entity.HasOne(d => d.Account).WithMany(p => p.AccessKeys)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("AccessKeys_Accounts_Id_fk");
        });

        modelBuilder.Entity<AccessRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AccessRoles_pk");

            entity.Property(e => e.IsDeleted).HasDefaultValueSql("false");
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Accounts_pk");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Accounts_Roles_RoleId_fk");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Category_pk");

            entity.ToTable("Category");
        });

        modelBuilder.Entity<Dtorrent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DTorrents_pk");

            entity.ToTable("DTorrents");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.Dtorrents)
                .HasForeignKey(d => d.RequestedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DTorrents_Torrent_RequestedBy_fk");
        });

        modelBuilder.Entity<FileExtension>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FileExtensions_pk");

            entity.HasOne(d => d.AddedByNavigation).WithMany(p => p.FileExtensions)
                .HasForeignKey(d => d.AddedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FileExtensions_Accounts_Id_fk");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Movies_pk");

            entity.HasOne(d => d.Category).WithMany(p => p.Movies)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Movies_Category_Id_fk");

            entity.HasOne(d => d.Download).WithMany(p => p.Movies)
                .HasForeignKey(d => d.DownloadId)
                .HasConstraintName("Movies_Uploads_Id_fk");

            entity.HasOne(d => d.ExtensionNavigation).WithMany(p => p.Movies)
                .HasForeignKey(d => d.Extension)
                .HasConstraintName("Movies_FileExtensions_Id_fk");

            entity.HasOne(d => d.Torrent).WithMany(p => p.Movies)
                .HasForeignKey(d => d.TorrentId)
                .HasConstraintName("Movies_DTorrents_Id_fk");
        });

        modelBuilder.Entity<Upload>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Uploads_pk");

            entity.HasOne(d => d.Category).WithMany(p => p.Uploads)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Uploads_Category_Id_fk");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.Uploads)
                .HasForeignKey(d => d.RequestedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Uploads_Accounts_Id_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

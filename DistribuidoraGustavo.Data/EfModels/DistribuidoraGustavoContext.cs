using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class DistribuidoraGustavoContext : DbContext
{
    public DistribuidoraGustavoContext()
    {
    }

    public DistribuidoraGustavoContext(DbContextOptions<DistribuidoraGustavoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("PK__Clients__05623063FF67CADC");

            entity.Property(e => e.ClientId).HasColumnName("CLientId");
            entity.Property(e => e.InvoicePrefix)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name).IsUnicode(false);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAB5A88DBB4F");

            entity.HasIndex(e => e.InvoiceNumber, "UQ__Invoices__D776E9814058E364").IsUnique();

            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Client).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoices_ClientID");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD6051C251");

            entity.Property(e => e.Active)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Code).IsUnicode(false);
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Name).IsUnicode(false);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CBC734F0E");

            entity.Property(e => e.Active)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Password).IsUnicode(false);
            entity.Property(e => e.Username).IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

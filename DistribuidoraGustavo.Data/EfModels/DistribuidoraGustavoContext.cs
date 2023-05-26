﻿using System;
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

    public virtual DbSet<PriceList> PriceLists { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductsPriceList> ProductsPriceLists { get; set; }

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

            entity.HasOne(d => d.PriceList).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.PriceListId)
                .HasConstraintName("FK_Invoices_PriceListID");
        });

        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.HasKey(e => e.PriceListId).HasName("PK__PriceLis__1E30F3AC15AD6B80");

            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Percent).HasColumnType("decimal(10, 2)");
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
        });

        modelBuilder.Entity<ProductsPriceList>(entity =>
        {
            entity.HasKey(e => e.ProductPriceListId).HasName("PK__Products__9384CA09AEAE7C28");

            entity.ToTable("Products_PriceLists");

            entity.Property(e => e.ProductPriceListId).HasColumnName("Product_PriceListID");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PriceListId).HasColumnName("PriceListID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.PriceList).WithMany(p => p.ProductsPriceLists)
                .HasForeignKey(d => d.PriceListId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_PriceLists_PriceListID");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductsPriceLists)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_PriceLists_ProductID");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__tmp_ms_x__1788CC4CF22E9043");

            entity.Property(e => e.Active)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.Name).IsUnicode(false);
            entity.Property(e => e.Password).IsUnicode(false);
            entity.Property(e => e.Username).IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

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

    public virtual DbSet<Family> Families { get; set; }

    public virtual DbSet<ImportedFile> ImportedFiles { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoicesProduct> InvoicesProducts { get; set; }

    public virtual DbSet<PriceList> PriceLists { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductsPriceList> ProductsPriceLists { get; set; }

    public virtual DbSet<ProductsFamily> ProductsFamilies { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId);

            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.ActualBalance)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DefaultPriceListId).HasColumnName("DefaultPriceListID");
            entity.Property(e => e.InvoicePrefix)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name).IsUnicode(false);

            entity.HasOne(d => d.DefaultPriceList).WithMany(p => p.Clients)
                .HasForeignKey(d => d.DefaultPriceListId)
                .HasConstraintName("FK_Clients_PriceListID");
        });

        modelBuilder.Entity<Family>(entity =>
        {
            entity.HasKey(e => e.FamilyId);

            entity.HasIndex(e => e.Name, "UQ_Families_Name").IsUnique();
            entity.HasIndex(e => e.CodeSuffix, "UQ_Families_CodeSuffix").IsUnique();

            entity.Property(e => e.FamilyId).HasColumnName("FamilyID");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.CodeSuffix)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ImportedFile>(entity =>
        {
            entity.HasKey(e => e.ImportedFileId);

            entity.Property(e => e.ImportedFileId).HasColumnName("ImportedFileID");
            entity.Property(e => e.DateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId);

            entity.HasIndex(e => e.InvoiceNumber, "UQ_Invoices_InvoiceNumber").IsUnique();

            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.PriceListId).HasColumnName("PriceListID");
            entity.Property(e => e.Active).HasDefaultValue(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("getutcdate()")
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

        modelBuilder.Entity<InvoicesProduct>(entity =>
        {
            entity.HasKey(e => e.InvoicesProductsId);

            entity.ToTable("Invoices_Products");

            entity.Property(e => e.InvoicesProductsId).HasColumnName("InvoicesProductsID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoicesProducts)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoices_Products_InvoiceID");

            entity.HasOne(d => d.Product).WithMany(p => p.InvoicesProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invoices_Products_ProductID");
        });

        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.HasKey(e => e.PriceListId);

            entity.Property(e => e.Name)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Percent).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Active)
                .IsRequired()
                .HasDefaultValue(true);
            entity.Property(e => e.BasePrice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.QuantityPerBox).HasDefaultValue(0);
            entity.Property(e => e.Code).IsUnicode(false);
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Name).IsUnicode(false);
        });

        modelBuilder.Entity<ProductsFamily>(entity =>
        {
            entity.HasKey(e => e.ProductFamilyId);

            entity.ToTable("Products_Families");

            // one family per product for now; drop this index to allow several
            entity.HasIndex(e => e.ProductId, "UQ_Products_Families_ProductID").IsUnique();

            entity.Property(e => e.ProductFamilyId).HasColumnName("ProductFamilyID");
            entity.Property(e => e.FamilyId).HasColumnName("FamilyID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Family).WithMany(p => p.ProductsFamilies)
                .HasForeignKey(d => d.FamilyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Families_FamilyID");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductsFamilies)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Families_ProductID");
        });

        modelBuilder.Entity<ProductsPriceList>(entity =>
        {
            entity.HasKey(e => e.ProductPriceListId);

            entity.ToTable("Products_PriceLists");

            entity.Property(e => e.ProductPriceListId).HasColumnName("ProductPriceListID");
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

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId);

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("getutcdate()")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(255);

            entity.HasOne(d => d.Client).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaction_ClientID");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Active)
                .IsRequired()
                .HasDefaultValue(true);
            entity.Property(e => e.Name).IsUnicode(false);
            entity.Property(e => e.Password).IsUnicode(false);
            entity.Property(e => e.Username).IsUnicode(false);
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.UserTokenId);

            entity.Property(e => e.UserTokenId).HasColumnName("UserTokenID");
            entity.Property(e => e.ExpireTime).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserTokens_UserID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

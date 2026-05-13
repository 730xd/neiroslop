using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace rpm1920.Models;

public partial class WarehouseDbContext : DbContext
{
    public WarehouseDbContext()
    {
    }

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<IssueRequest> IssueRequests { get; set; }

    public virtual DbSet<Movement> Movements { get; set; }

    public virtual DbSet<PriceList> PriceLists { get; set; }

    public virtual DbSet<ReceiptInvoice> ReceiptInvoices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\sqlexpress;Database=WarehouseDB;Trusted_Connection=True;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IssueRequest>(entity =>
        {
            entity.HasKey(e => new { e.RequestNumber, e.Code });

            entity.ToTable("IssueRequest");

            entity.Property(e => e.RequestNumber).HasMaxLength(20);

            entity.HasOne(d => d.CodeNavigation).WithMany(p => p.IssueRequests)
                .HasForeignKey(d => d.Code)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Issue_Movement");
        });

        modelBuilder.Entity<Movement>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PK__Movement__A25C5AA6A10E74E2");

            entity.ToTable("Movement");

            entity.Property(e => e.Code).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasDefaultValue(0);
        });

        modelBuilder.Entity<PriceList>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("PK__PriceLis__A25C5AA6B7CBB48F");

            entity.ToTable("PriceList");

            entity.Property(e => e.Code).ValueGeneratedNever();
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.CodeNavigation).WithOne(p => p.PriceList)
                .HasForeignKey<PriceList>(d => d.Code)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Price_Movement");
        });

        modelBuilder.Entity<ReceiptInvoice>(entity =>
        {
            entity.HasKey(e => new { e.InvoiceNumber, e.Code });

            entity.ToTable("ReceiptInvoice");

            entity.Property(e => e.InvoiceNumber).HasMaxLength(20);

            entity.HasOne(d => d.CodeNavigation).WithMany(p => p.ReceiptInvoices)
                .HasForeignKey(d => d.Code)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Receipt_Movement");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

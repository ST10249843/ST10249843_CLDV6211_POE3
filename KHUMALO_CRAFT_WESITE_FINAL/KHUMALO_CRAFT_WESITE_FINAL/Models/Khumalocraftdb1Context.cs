using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KHUMALO_CRAFT_WESITE_FINAL.Models;

public partial class Khumalocraftdb1Context : DbContext
{
    public Khumalocraftdb1Context()
    {
    }

    public Khumalocraftdb1Context(DbContextOptions<Khumalocraftdb1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=SAJANA_BIDESI;Initial Catalog=KHUMALOCRAFTDB1;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__CART__51BCD797E99FDF8B");

            entity.ToTable("CART");

            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Userid).HasColumnName("USERID");

            entity.HasOne(d => d.Product).WithMany(p => p.Carts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__CART__ProductID__44FF419A");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("FK__CART__USERID__45F365D3");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__PRODUCTS__B40CC6ED241C9F4F");

            entity.ToTable("PRODUCTS");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.ProductAvailability)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.ProductCategory)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProductDescription).HasColumnType("text");
            entity.Property(e => e.ProductImage)
                .HasMaxLength(800)
                .IsUnicode(false);
            entity.Property(e => e.ProductName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Userid).HasColumnName("USERID");

            entity.HasOne(d => d.User).WithMany(p => p.Products)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("FK__PRODUCTS__USERID__4222D4EF");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("PK__USERS__7B9E7F3516C35235");

            entity.ToTable("USERS");

            entity.Property(e => e.Userid).HasColumnName("USERID");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("PASSWORD");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

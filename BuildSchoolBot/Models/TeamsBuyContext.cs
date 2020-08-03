using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BuildSchoolBot.Models
{
    public partial class TeamsBuyContext : DbContext
    {
        public TeamsBuyContext()          
        {
        }

        public TeamsBuyContext(DbContextOptions<TeamsBuyContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Additional> Additional { get; set; }
        public virtual DbSet<AdditionalDetail> AdditionalDetail { get; set; }
        public virtual DbSet<CustomizedOrder> CustomizedOrder { get; set; }
        public virtual DbSet<CustomizedOrderDetail> CustomizedOrderDetail { get; set; }
        public virtual DbSet<Library> Library { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderDetail> OrderDetail { get; set; }
        public virtual DbSet<Schedule> Schedule { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=groupbuyserver.database.windows.net;Database=TeamsBuy;Trusted_Connection=False;User ID=groupbuy;Password=GBuy@123;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Additional>(entity =>
            {
                entity.HasKey(e => e.AddId);

                entity.Property(e => e.AddId).ValueGeneratedNever();

                entity.Property(e => e.AddType).HasMaxLength(200);
            });

            modelBuilder.Entity<AdditionalDetail>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.AddItem).HasMaxLength(200);

                entity.HasOne(d => d.Add)
                    .WithMany()
                    .HasForeignKey(d => d.AddId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AdditionalDetail_Additional");
            });

            modelBuilder.Entity<CustomizedOrder>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(d => d.CustomizedMenu)
                    .WithMany()
                    .HasForeignKey(d => d.CustomizedMenuId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CustomizedOrder_CustomizedOrderDetail");
            });

            modelBuilder.Entity<CustomizedOrderDetail>(entity =>
            {
                entity.HasKey(e => e.CustomizedMenuId);

                entity.Property(e => e.CustomizedMenuId).ValueGeneratedNever();

                entity.Property(e => e.Money).HasColumnType("money");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<Library>(entity =>
            {
                entity.Property(e => e.LibraryId).ValueGeneratedNever();

                entity.Property(e => e.LibraryName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Uri).IsRequired();
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.OrderId).ValueGeneratedNever();

                entity.Property(e => e.Date).HasColumnType("datetime");
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasNoKey();

                entity.HasIndex(e => e.OrderId)
                    .HasName("IX_OrderDetail");

                entity.Property(e => e.Amount).HasColumnType("money");

                entity.Property(e => e.Mark).HasMaxLength(2000);

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(d => d.Order)
                    .WithMany()
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderDetail_Order");
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.Property(e => e.ScheduleId).ValueGeneratedNever();

                entity.Property(e => e.MenuUri).HasMaxLength(1000);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

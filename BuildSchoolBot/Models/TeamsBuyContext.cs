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

        public virtual DbSet<Library> Library { get; set; }
        public virtual DbSet<MenuDetail> MenuDetail { get; set; }
        public virtual DbSet<MenuOrder> MenuOrder { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderDetail> OrderDetail { get; set; }
        public virtual DbSet<Payment> Payment { get; set; }
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
            modelBuilder.Entity<Library>(entity =>
            {
                entity.Property(e => e.LibraryId).ValueGeneratedNever();

                entity.Property(e => e.LibraryName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.MemberId).IsRequired();

                entity.Property(e => e.Uri).IsRequired();
            });

            modelBuilder.Entity<MenuDetail>(entity =>
            {
                entity.Property(e => e.MenuDetailId).ValueGeneratedNever();

                entity.Property(e => e.Amount).HasColumnType("money");

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<MenuOrder>(entity =>
            {
                entity.HasKey(e => e.MenuId)
                    .HasName("PK_CustomizedOrder");

                entity.Property(e => e.MenuId).ValueGeneratedNever();

                entity.Property(e => e.Store)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.TeamsId)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.Menu)
                    .WithOne(p => p.MenuOrder)
                    .HasForeignKey<MenuOrder>(d => d.MenuId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CustomizedOrder_CustomizedOrderDetail");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.OrderId).ValueGeneratedNever();

                entity.Property(e => e.ChannelId).IsRequired();

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.StoreName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsFixedLength();
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasIndex(e => e.OrderId)
                    .HasName("IX_OrderDetail");

                entity.Property(e => e.OrderDetailId).ValueGeneratedNever();

                entity.Property(e => e.Amount).HasColumnType("money");

                entity.Property(e => e.Mark).HasMaxLength(2000);

                entity.Property(e => e.MemberId).IsRequired();

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderDetail)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrderDetail_Order");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.MemberId);

                entity.Property(e => e.MemberId).HasMaxLength(100);

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(100);
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

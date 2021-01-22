using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WFHMicrositeMaintenance.Models
{
    public partial class WFHMicrositeContext : DbContext
    {
        public WFHMicrositeContext()
        {
        }

        public WFHMicrositeContext(DbContextOptions<WFHMicrositeContext> options)
            : base(options)
        {
        }

        public virtual DbSet<MasterPostalZipP2> MasterPostalZipP2 { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductImage> ProductImage { get; set; }
        public virtual DbSet<ProductOption> ProductOption { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserLog> UserLog { get; set; }
        public virtual DbSet<UserNote> UserNote { get; set; }
        public virtual DbSet<UserSelection> UserSelection { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MasterPostalZipP2>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PostalCode)
                    .HasMaxLength(15)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.Chair)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Config).HasMaxLength(100);

                entity.Property(e => e.DealerCode)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.InstallGuide).HasMaxLength(50);

                entity.Property(e => e.Language)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.LogoFile).HasMaxLength(100);

                entity.Property(e => e.LogoFile2).HasMaxLength(100);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Ponumber)
                    .HasColumnName("PONumber")
                    .HasMaxLength(30);

                entity.Property(e => e.Shipper).HasMaxLength(10);

                entity.Property(e => e.SitFitGuide).HasMaxLength(50);

                entity.Property(e => e.UserGuide).HasMaxLength(50);

                entity.Property(e => e.VideoUrl).HasMaxLength(50);
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.Property(e => e.FileName).HasMaxLength(100);
            });

            modelBuilder.Entity<ProductOption>(entity =>
            {
                entity.Property(e => e.ProductOptionId).HasColumnName("ProductOptionID");

                entity.Property(e => e.FileName).HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.StockCode).HasMaxLength(50);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(10);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.Address1).HasMaxLength(100);

                entity.Property(e => e.Address2).HasMaxLength(100);

                entity.Property(e => e.AttnName).HasMaxLength(50);

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.Completed).HasColumnType("datetime");

                entity.Property(e => e.Country).HasMaxLength(10);

                entity.Property(e => e.EmailAddress)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Emailed).HasColumnType("datetime");

                entity.Property(e => e.InProduction).HasColumnType("datetime");

                entity.Property(e => e.Language)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.PhoneNumber).HasMaxLength(20);

                entity.Property(e => e.Pin)
                    .HasColumnName("PIN")
                    .HasMaxLength(10);

                entity.Property(e => e.PostalZip).HasMaxLength(15);

                entity.Property(e => e.ProductId).HasColumnName("ProductID");

                entity.Property(e => e.ProvinceState).HasMaxLength(50);

                entity.Property(e => e.Shipped).HasColumnType("datetime");

                entity.Property(e => e.SpecialInstructions).HasMaxLength(1000);

                entity.Property(e => e.Submitted).HasColumnType("datetime");

                entity.Property(e => e.TrackingNumber).HasMaxLength(50);
            });

            modelBuilder.Entity<UserLog>(entity =>
            {
                entity.Property(e => e.UserLogId).HasColumnName("UserLogID");

                entity.Property(e => e.Details)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Updated).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<UserNote>(entity =>
            {
                entity.Property(e => e.UserNoteId).HasColumnName("UserNoteID");

                entity.Property(e => e.Csuser)
                    .IsRequired()
                    .HasColumnName("CSUser")
                    .HasMaxLength(50);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Note).IsRequired();

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            modelBuilder.Entity<UserSelection>(entity =>
            {
                entity.Property(e => e.UserSelectionId).HasColumnName("UserSelectionID");

                entity.Property(e => e.ProductOptionId).HasColumnName("ProductOptionID");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.UserId).HasColumnName("UserID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

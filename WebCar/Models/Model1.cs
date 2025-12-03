using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace WebCar.Models
{
    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model1")
        {
        }

        public virtual DbSet<ACCOUNT_ROLE> ACCOUNT_ROLE { get; set; }
        public virtual DbSet<AUDIT_LOG> AUDIT_LOG { get; set; }
        public virtual DbSet<CAR> CARs { get; set; }
        public virtual DbSet<CUSTOMER> CUSTOMERs { get; set; }
        public virtual DbSet<ENCRYPTION_KEY> ENCRYPTION_KEY { get; set; }
        public virtual DbSet<FEEDBACK> FEEDBACKs { get; set; }
        public virtual DbSet<ORDER_DETAIL> ORDER_DETAIL { get; set; }
        public virtual DbSet<ORDER> ORDERS { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ACCOUNT_ROLE>()
                .Property(e => e.MATK)
                .HasPrecision(38, 0);

            modelBuilder.Entity<ACCOUNT_ROLE>()
                .Property(e => e.MAKH)
                .HasPrecision(38, 0);

            modelBuilder.Entity<ACCOUNT_ROLE>()
                .Property(e => e.ROLENAME)
                .IsUnicode(false);

            modelBuilder.Entity<AUDIT_LOG>()
                .Property(e => e.MALOG)
                .HasPrecision(38, 0);

            modelBuilder.Entity<AUDIT_LOG>()
                .Property(e => e.MATK)
                .HasPrecision(38, 0);

            modelBuilder.Entity<AUDIT_LOG>()
                .Property(e => e.HANHDONG)
                .IsUnicode(false);

            modelBuilder.Entity<AUDIT_LOG>()
                .Property(e => e.BANGTACDONG)
                .IsUnicode(false);

            modelBuilder.Entity<AUDIT_LOG>()
                .Property(e => e.IP)
                .IsUnicode(false);

            modelBuilder.Entity<CAR>()
                .Property(e => e.MAXE)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CAR>()
                .Property(e => e.TENXE)
                .IsUnicode(false);

            modelBuilder.Entity<CAR>()
                .Property(e => e.HANGXE)
                .IsUnicode(false);

            modelBuilder.Entity<CAR>()
                .Property(e => e.GIA)
                .HasPrecision(12, 2);

            modelBuilder.Entity<CAR>()
                .Property(e => e.MOTA)
                .IsUnicode(false);

            modelBuilder.Entity<CAR>()
                .Property(e => e.HINHANH)
                .IsUnicode(false);

            modelBuilder.Entity<CAR>()
                .Property(e => e.TRANGTHAI)
                .IsUnicode(false);

            modelBuilder.Entity<CAR>()
                .HasMany(e => e.ORDER_DETAIL)
                .WithRequired(e => e.CAR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CUSTOMER>()
                .Property(e => e.MAKH)
                .HasPrecision(38, 0);

            modelBuilder.Entity<CUSTOMER>()
                .Property(e => e.HOTEN)
                .IsUnicode(false);

            modelBuilder.Entity<CUSTOMER>()
                .Property(e => e.EMAIL)
                .IsUnicode(false);

            modelBuilder.Entity<CUSTOMER>()
                .Property(e => e.SDT)
                .IsUnicode(false);

            modelBuilder.Entity<CUSTOMER>()
                .Property(e => e.MATKHAU)
                .IsUnicode(false);

            modelBuilder.Entity<CUSTOMER>()
                .Property(e => e.DIACHI)
                .IsUnicode(false);

            modelBuilder.Entity<ENCRYPTION_KEY>()
                .Property(e => e.KEYID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<ENCRYPTION_KEY>()
                .Property(e => e.KEYTYPE)
                .IsUnicode(false);

            modelBuilder.Entity<ENCRYPTION_KEY>()
                .Property(e => e.PUBLICKEY)
                .IsUnicode(false);

            modelBuilder.Entity<ENCRYPTION_KEY>()
                .Property(e => e.PRIVATEKEY)
                .IsUnicode(false);

            modelBuilder.Entity<FEEDBACK>()
                .Property(e => e.MAFB)
                .HasPrecision(38, 0);

            modelBuilder.Entity<FEEDBACK>()
                .Property(e => e.MAKH)
                .HasPrecision(38, 0);

            modelBuilder.Entity<FEEDBACK>()
                .Property(e => e.MAXE)
                .HasPrecision(38, 0);

            modelBuilder.Entity<FEEDBACK>()
                .Property(e => e.NOIDUNG)
                .IsUnicode(false);

            modelBuilder.Entity<ORDER_DETAIL>()
                .Property(e => e.MADON)
                .HasPrecision(38, 0);

            modelBuilder.Entity<ORDER_DETAIL>()
                .Property(e => e.MAXE)
                .HasPrecision(38, 0);

            modelBuilder.Entity<ORDER_DETAIL>()
                .Property(e => e.SOLUONG)
                .HasPrecision(38, 0);

            modelBuilder.Entity<ORDER_DETAIL>()
                .Property(e => e.DONGIA)
                .HasPrecision(12, 2);

            modelBuilder.Entity<ORDER>()
                .Property(e => e.MADON)
                .HasPrecision(38, 0);

            modelBuilder.Entity<ORDER>()
                .Property(e => e.MAKH)
                .HasPrecision(38, 0);

            modelBuilder.Entity<ORDER>()
                .Property(e => e.TONGTIEN)
                .HasPrecision(12, 2);

            modelBuilder.Entity<ORDER>()
                .Property(e => e.TRANGTHAI)
                .IsUnicode(false);

            modelBuilder.Entity<ORDER>()
                .HasMany(e => e.ORDER_DETAIL)
                .WithRequired(e => e.ORDER)
                .WillCascadeOnDelete(false);
        }
    }
}

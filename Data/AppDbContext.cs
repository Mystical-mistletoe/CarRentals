using CarRentals.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentals.Data
{
    internal class AppDbContext : DbContext
    {
        //для операций, с колонками из моделей
        public DbSet<Car> Cars { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<User> Users { get; set; } 
        public DbSet<AdAccount> AdAccounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=CarRentalsDB;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Car>(entity =>
            {
                entity.ToTable("Cars");
                entity.HasKey(e => e.CarId);

                entity.Property(e => e.Brand)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Model)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.YearCar)
                    .IsRequired();

                entity.Property(e => e.LicensePlate)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasIndex(e => e.LicensePlate)
                    .IsUnique();

                entity.Property(e => e.PricePerHour)
                    .HasPrecision(10, 2);

                entity.Property(e => e.PricePerDay)
                    .HasPrecision(10, 2);

            });


            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");
                entity.HasKey(e => e.BookingId);

                entity.Property(e => e.StartDateTime)
                    .IsRequired();

                entity.Property(e => e.EndDateTime)
                    .IsRequired();

                entity.Property(e => e.TotalPrice)
                    .HasPrecision(10, 2);

                entity.HasOne(e => e.Car)
                    .WithMany(e => e.Bookings)
                    .HasForeignKey(e => e.CarId)
                    .OnDelete(DeleteBehavior.Restrict); //запр удаление

                entity.HasIndex(e => e.CarId);
                entity.HasIndex(e => e.StatusBooking); //индексы для поиска по ид и статусу

                entity.HasOne(e => e.User)
                .WithMany(e => e.Bookings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.UserId);
            });

     
            modelBuilder.Entity<Fine>(entity =>
            {
                entity.ToTable("Fines");
                entity.HasKey(e => e.FineId);

                entity.Property(e => e.Amount)
                    .HasPrecision(10, 2);

                entity.Property(e => e.Reason)
                    .HasMaxLength(500);

                entity.HasOne(e => e.Booking)
                    .WithMany(e => e.Fines)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.BookingId);
                entity.HasIndex(e => e.StatusFine);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.LastName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });
        }

    }


}

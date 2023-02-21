using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MotelBookingApp.Models;
using System.Collections.Generic;

namespace MotelBookingApp.Data
{
    public class MotelDbContext : IdentityDbContext<AppUser, AppRole,int>
    {

        public MotelDbContext(DbContextOptions<MotelDbContext> options)
          : base(options)
        {
        }

        public DbSet<BookedRecord> BookedRecords { get; set; }
        public DbSet<BookingCart> BookingCarts { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<RoomType> Types { get; set; }
        public DbSet<Motel> Motels { get; set; }
        public DbSet<FavoriteMotel> FavoriteMotels { get; set; }
        public DbSet<Comment> Comments { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data source=MotelApp.sqlite");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            base.OnModelCreating(modelBuilder);
          
        }
    }
}

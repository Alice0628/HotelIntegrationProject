using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MotelBookingApp.Models;
using System.Collections.Generic;

namespace MotelBookingApp.Data
{
    public class MotelDbContext : IdentityDbContext<AppUser, AppRole, int>
    {

        public MotelDbContext(DbContextOptions<MotelDbContext> options)
          : base(options)
        {
        }

        public DbSet<BookedRecord> BookedRecords { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }
        public DbSet<BookingCart> BookingCarts { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Motel> Motels { get; set; }
        public DbSet<FavoriteMotelList> FavoriteMotelLists { get; set; }
        public DbSet<Comment> Comments { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlite(@"Data source=MotelApp.sqlite");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(modelBuilder);

        }
    }
}

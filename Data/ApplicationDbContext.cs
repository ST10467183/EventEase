using Microsoft.EntityFrameworkCore;
using EventEase.Models;

namespace EventEase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        
        public DbSet<EventType> EventTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           // Seed Event Types
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, TypeName = "Conference", Description = "Business conferences and seminars" },
                new EventType { EventTypeId = 2, TypeName = "Wedding", Description = "Wedding ceremonies and receptions" },
                new EventType { EventTypeId = 3, TypeName = "Birthday Party", Description = "Birthday celebrations" },
                new EventType { EventTypeId = 4, TypeName = "Corporate Meeting", Description = "Company meetings and workshops" },
                new EventType { EventTypeId = 5, TypeName = "Concert", Description = "Live music performances" },
                new EventType { EventTypeId = 6, TypeName = "Exhibition", Description = "Art or trade exhibitions" },
                new EventType { EventTypeId = 7, TypeName = "Private Function", Description = "Private gatherings" },
                new EventType { EventTypeId = 8, TypeName = "Other", Description = "Other event types" }
            );
        }
    }
}
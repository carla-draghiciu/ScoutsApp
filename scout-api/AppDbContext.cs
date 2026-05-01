using Microsoft.EntityFrameworkCore;
using scout_api.Models;

namespace scout_api
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<ScoutEvent> Events { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<EventAttendee> EventAttendees { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<UserBadge> UsersBadges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventAttendee>().HasKey(ea => new { ea.ScoutEventId, ea.AttendeeId });
            modelBuilder.Entity<UserBadge>().HasKey(ub => new { ub.UserId, ub.BadgeId });

            modelBuilder.Entity<ScoutEvent>()
                .HasOne(e => e.Creator)
                .WithMany(u => u.CreatedEvents)
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.Attendee)
                .WithMany(u => u.AttendedEvents)
                .HasForeignKey(ea => ea.AttendeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

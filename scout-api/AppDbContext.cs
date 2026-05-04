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
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }
        public DbSet<ObservationEntry> ObservationList { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventAttendee>().HasKey(ea => new { ea.ScoutEventId, ea.AttendeeId });
            modelBuilder.Entity<UserBadge>().HasKey(ub => new { ub.UserId, ub.BadgeId });
            modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });

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

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            );

            // Seed Permissions
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "manage_badges" },
                new Permission { Id = 2, Name = "create_event" },
                new Permission { Id = 3, Name = "update_event" },
                new Permission { Id = 4, Name = "delete_event" },
                new Permission { Id = 5, Name = "view_events" },
                new Permission { Id = 6, Name = "join_event" }
            );

            // Admin gets all permissions
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RoleId = 1, PermissionId = 1 }, // manage_badges
                new RolePermission { RoleId = 1, PermissionId = 2 }, // create_event
                new RolePermission { RoleId = 1, PermissionId = 3 }, // update_event
                new RolePermission { RoleId = 1, PermissionId = 4 }, // delete_event
                new RolePermission { RoleId = 1, PermissionId = 5 }, // view_events
                new RolePermission { RoleId = 1, PermissionId = 6 }  // join_event
            );

            // Normal user
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RoleId = 2, PermissionId = 2 }, // create_event
                new RolePermission { RoleId = 2, PermissionId = 3 }, // update_event
                new RolePermission { RoleId = 2, PermissionId = 4 }, // delete_event
                new RolePermission { RoleId = 2, PermissionId = 5 }, // view_events
                new RolePermission { RoleId = 2, PermissionId = 6 }  // join_event
            );
        }
    }
}

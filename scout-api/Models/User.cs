using scout_api.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace scout_api.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ScoutId { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public ScoutLevel ScoutLevel { get; set; }

        [Required]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;


        public List<UserBadge> EarnedBadges { get; set; } = new();
        public List<ScoutEvent> CreatedEvents { get; set; } = new();
        public List<EventAttendee> AttendedEvents { get; set; } = new();
    }
}

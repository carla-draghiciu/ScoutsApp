using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace scout_api.Models
{
    [Table("ScoutEvents")]
    public class ScoutEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        [MaxLength(500)]
        public string Equipment { get; set; } = string.Empty;
        public List<EventAttendee> Attendees { get; set; } = new();

        // user foreign key
        [Required]
        public int CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public User Creator { get; set; }

        // badge foreign key
        public int? BadgeId { get; set; }

        [ForeignKey("BadgeId")]
        public Badge? EventBadge { get; set; }
    }
}

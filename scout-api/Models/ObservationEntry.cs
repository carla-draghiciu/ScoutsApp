using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace scout_api.Models
{
    [Table("ObservationList")]
    public class ObservationEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        [MaxLength(255)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public DateTime FlaggedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;
    }
}
